r"""PreToolUse hook: the file you are about to write decides which standard you must read.

The failure this exists for: an agent added a test, never invoked `unit-testing` or
`integration-testing` though both were installed and listed, misclassified the test, and its own
`/review` repeated the blind spot and returned clean. Nothing was unreachable. The standard was
optional, and optional lost.

So the trigger stops being "the model decided this topic is relevant" and becomes "the path being
written". `.agents/skill-routes.json` maps path -> owning skill; a new concern is a new row there.

Two behaviours, and the difference matters:

- **First write into a routed path in a session -> block once** with the owning skills and their own
  descriptions, then allow every later write to that route. PreToolUse has no way to add context
  without stopping the call, and stopping once is the point: it puts the standard in context *before*
  the file exists, which costs one tool call and removes the discretion. State lives in a per-session
  file, so it is one interruption per route, not per edit.
- **A deny pattern -> block every time.** Those are mechanically decidable violations, so they are not
  advice. A repo whose rules are also expressible in its build should enforce them there too - a hook
  matcher is per-tool and never sees `dotnet new`, a shell heredoc or an MCP write.

Contract: exit 0 = allow, exit 2 = block with stderr fed back to the agent. Anything unexpected exits
0 - a broken router must not wedge every write, since a build gate is the tier that guarantees.

The same table also answers the question after the fact: `--skills-for <paths>` (or paths on stdin)
prints which skills a set of changed files obliges a reader to load. That is what a review runs, so a
review cannot miss what its author was required to load - the other half of the failure above, where
the follow-up review repeated the identical blind spot and returned clean.

Ships in the `agent-process` plugin, so both harnesses run this one file. They share the exit-2 block
contract but NOT the payload: Claude sends a PascalCase tool name and one path under `file_path`,
while Codex sends `apply_patch` with the paths named inside the patch body. Matching only Claude's
shape is not a partial rollout - it is a hook that allows every Codex write while looking wired. A
repo opts in by carrying `.agents/skill-routes.json`; without one the hook exits 0 and does nothing.
"""

import hashlib
import json
import os
import re
import sys
import tempfile
from pathlib import Path

from hook_runtime import claim_invocation, own_payload_root

# Skill descriptions carry non-ASCII punctuation, and this text is what the agent acts on.
# Windows defaults these streams to cp1252, which renders it as mojibake.
for _stream in (sys.stdout, sys.stderr):
    if hasattr(_stream, "reconfigure"):
        _stream.reconfigure(encoding="utf-8", errors="replace")


ROUTES_FILE = ".agents/skill-routes.json"
# Lowercased, because the two harnesses do not agree on casing or on names. Claude writes through
# Write/Edit/MultiEdit/NotebookEdit; Codex writes through apply_patch, and matching only Claude's
# vocabulary is how this hook spent its first life doing nothing at all in a Codex session.
CLAUDE_WRITE_TOOLS = {
    "write",
    "edit",
    "multiedit",
    "notebookedit",
}
CODEX_WRITE_TOOLS = {
    "apply_patch",
    "edit_file",
    "write_file",
    "multi_edit",
}
WRITE_TOOLS = CLAUDE_WRITE_TOOLS | CODEX_WRITE_TOOLS
PATH_KEYS = ("file_path", "notebook_path", "path", "filepath")
# An apply_patch envelope names its files inside the patch body, so a key lookup alone sees none of them.
PATCH_FILE_TARGET = re.compile(r"\*\*\*\s+(?:Add|Update|Delete)\s+File:\s*([^\r\n]+)", re.IGNORECASE)
PATCH_ADDED_LINE = re.compile(r"^\+(?!\+\+)(.*)$", re.MULTILINE)
QUERY_FLAG = "--skills-for"
VERIFY_FLAG = "--verify-install"
# Junction/copy deployment: one flat namespace per harness root.
LINKED_SKILL_ROOTS = {
    "claude": ("skills",),
    "codex": (".agents/skills", ".codex/skills"),
}
CODEX_PLUGIN_CACHE = ".codex/plugins/cache"
# Some uninstall paths leave the cache directory behind carrying this marker. It is a useful hint but
# NOT a reliable one: removing a marketplace dropped its plugins from the manifest while leaving the
# whole payload in the cache with no marker at all. So the manifest below is the authority where one
# exists, and this only filters what the directory walk turns up.
ORPHAN_MARKER = ".orphaned_at"


def claude_config_root(home):
    configured = os.environ.get("CLAUDE_CONFIG_DIR")
    return Path(configured) if configured else home / ".claude"


def manifest_install_roots(home):
    """Claude's authoritative installed set. None when unreadable, meaning fall back to walking.

    A cache directory is evidence a plugin WAS installed, never that it still is.
    """
    manifest = claude_config_root(home) / "plugins" / "installed_plugins.json"
    try:
        data = json.loads(manifest.read_text(encoding="utf-8-sig"))
    except (OSError, ValueError):
        return None
    plugins = data.get("plugins")
    if not isinstance(plugins, dict):
        return None
    roots = []
    for installs in plugins.values():
        if not isinstance(installs, list):
            continue
        for install in installs:
            path = install.get("installPath") if isinstance(install, dict) else None
            if path:
                roots.append(Path(path))
    return roots


def active_harness(tool_name):
    lowered = tool_name.lower()
    if lowered in CLAUDE_WRITE_TOOLS:
        return "claude"
    if lowered in CODEX_WRITE_TOOLS:
        return "codex"
    return None


def skill_search_dirs(harness):
    """Every directory that may hold `<name>/SKILL.md`, nearest delivery first.

    Ordered own-plugin, then linked roots, then other plugins: the first is exact and needs no globbing,
    and it is the one that answers when this hook is itself running from an installed plugin.
    """
    home = Path.home()

    # The hook's own payload is independent of the harness variable vocabulary. Codex injects
    # PLUGIN_ROOT; Claude injects CLAUDE_PLUGIN_ROOT; a vendored copy resolves beside itself.
    yield own_payload_root(__file__) / "skills"

    linked_base = claude_config_root(home) if harness == "claude" else home
    for relative in LINKED_SKILL_ROOTS[harness]:
        yield linked_base / relative

    if harness == "claude":
        manifest_roots = manifest_install_roots(home)
        if manifest_roots is not None:
            for root in manifest_roots:
                yield root / "skills"
            return
        cache = claude_config_root(home) / "plugins" / "cache"
    else:
        cache = home / CODEX_PLUGIN_CACHE

    if not cache.is_dir():
        return
    try:
        versions = sorted(cache.glob("*/*/*"))
    except OSError:
        return
    for version in versions:
        if (version / ORPHAN_MARKER).exists():
            continue
        yield version / "skills"


def repo_relative(path, cwd):
    """POSIX repo-relative path, so a route regex never has to know about drive letters.

    Resolved against the payload's cwd, not the hook process's - a patch body names its files
    relative to the session, and `Path.resolve()` alone would anchor them wherever python started.
    """
    try:
        p = Path(path)
        if not p.is_absolute():
            p = Path(cwd) / p
        p = p.resolve()
    except OSError:
        return str(path).replace("\\", "/")
    for base in (Path(cwd).resolve(), *Path(cwd).resolve().parents):
        if (base / ".git").exists():
            try:
                return p.relative_to(base).as_posix()
            except ValueError:
                break
    return p.as_posix()


def strings(value):
    """Every string anywhere in the payload - the two harnesses nest their write differently."""
    if isinstance(value, str):
        yield value
    elif isinstance(value, dict):
        for item in value.values():
            yield from strings(item)
    elif isinstance(value, list):
        for item in value:
            yield from strings(item)


def written_content(tool_input):
    """Every string the tool would put into the file, concatenated for pattern matching.

    Only text being ADDED. A deny pattern must never fire on the text a call is deleting - matching a
    patch blob whole would block the very edit that removes the violation.
    """
    parts = [
        tool_input.get("content"),
        tool_input.get("new_string"),
        tool_input.get("new_source"),
    ]
    for edit in tool_input.get("edits") or []:
        if isinstance(edit, dict):
            parts.append(edit.get("new_string"))
    for blob in strings(tool_input):
        if PATCH_FILE_TARGET.search(blob):
            parts.extend(PATCH_ADDED_LINE.findall(blob))
    return "\n".join(p for p in parts if isinstance(p, str))


def written_targets(tool_input):
    """Every path this call would write, in order and deduplicated.

    Claude names one file per call under a known key; Codex's apply_patch can carry many, named only
    inside the patch body.
    """
    found = []
    for key in PATH_KEYS:
        value = tool_input.get(key)
        if isinstance(value, str) and value:
            found.append(value)
    for blob in strings(tool_input):
        for match in PATCH_FILE_TARGET.findall(blob):
            found.append(match.strip().strip("\"'"))

    ordered, seen = [], set()
    for target in found:
        if target and target not in seen:
            seen.add(target)
            ordered.append(target)
    return ordered


def plugin_of(skills_dir):
    """The plugin a `<...>/skills` directory belongs to, or None for a linked (non-plugin) root.

    Cache layout is `<cache>/<marketplace>/<plugin>/<version>/skills`, so the plugin is two levels up
    from the yielded directory; an own-plugin root is `<plugin-root>/skills`, one level up. A linked
    root is `~/.claude/skills`, which belongs to no plugin - checked by name, since it is the only
    shape without a plugin above it.
    """
    parents = skills_dir.parents
    # <cache>/<marketplace>/<plugin>/<version>/skills - plugin is two above, cache four above.
    if len(parents) >= 4 and parents[3].name == "cache":
        return parents[1].name
    parent = skills_dir.parent
    if parent.name in {".agents", ".claude"}:
        return None
    return parent.name


def skill_description(name, harness):
    """The skill's own description, so the rule text has exactly one home.

    A name may be plugin-qualified (`dotnet:persistence`). It has to be able to be: a local roster and
    its generic counterpart deliberately share a skill name, so an unqualified lookup returns whichever
    root is walked first and silently hides the other - the same shadowing that once made
    agent-standards' PERSISTENCE.md resolve to dotagents'. Unqualified still works for a skill with one
    home, which is every utility and every route that names only one side.
    """
    wanted_plugin, _, bare = name.rpartition(":")
    for root in skill_search_dirs(harness):
        if wanted_plugin and plugin_of(root) != wanted_plugin:
            continue
        skill = root / bare / "SKILL.md"
        if not skill.is_file():
            continue
        try:
            text = skill.read_text(encoding="utf-8-sig")
        except OSError:
            continue
        m = re.search(r"^description:[ \t]*(.+?)(?=^\w+:|^---)", text, re.M | re.S)
        if m:
            return " ".join(m.group(1).split())
    return None


def state_path(session_id):
    key = hashlib.sha256((session_id or "nosession").encode()).hexdigest()[:16]
    return Path(tempfile.gettempdir()) / f"skill-router-{key}.json"


def load_seen(session_id):
    try:
        return set(json.loads(state_path(session_id).read_text(encoding="utf-8")))
    except (OSError, ValueError):
        return set()


def save_seen(session_id, seen):
    try:
        state_path(session_id).write_text(json.dumps(sorted(seen)), encoding="utf-8")
    except OSError:
        pass  # a router that cannot persist should nag, never crash


def find_repo_root(cwd):
    base = Path(cwd).resolve()
    for candidate in (base, *base.parents):
        if (candidate / ROUTES_FILE).is_file():
            return candidate
    return None


class RoutesUnusable(Exception):
    """The repo opted into routing and the table cannot be read. Never silently ignored."""


def load_routes(root):
    """Absent table -> None, the repo has not opted in. Present but unusable -> raises.

    Failing open on a malformed table is the ENF1 failure exactly: enforcement that is inert while
    looking wired. A repo that created this file asked for routing, so a typo in it is a loud stop.
    """
    path = root / ROUTES_FILE
    if not path.is_file():
        return None
    try:
        parsed = json.loads(path.read_text(encoding="utf-8"))
    except OSError as error:
        raise RoutesUnusable(f"{ROUTES_FILE} exists but could not be read: {error}") from error
    except ValueError as error:
        raise RoutesUnusable(f"{ROUTES_FILE} exists but is not valid JSON: {error}") from error
    routes = parsed.get("routes")
    if routes is None:
        raise RoutesUnusable(f"{ROUTES_FILE} has no `routes` key.")
    if not isinstance(routes, list):
        raise RoutesUnusable(f"{ROUTES_FILE} `routes` must be a list, got {type(routes).__name__}.")
    return routes


def matching_routes(routes, rel, content):
    """One matcher for both callers, so a review resolves a path exactly as the write-time block did."""
    for route in routes:
        pattern = route.get("path")
        if not pattern or not re.search(pattern, rel):
            continue
        needle = route.get("content_requires")
        if needle and not re.search(needle, content):
            continue
        yield route


def deny_hits(route, rel, content):
    for rule in route.get("deny") or []:
        pattern = rule.get("pattern")
        if pattern and re.search(pattern, content):
            yield rel, rule.get("reason", "")


def query(argv):
    """`--skills-for <paths>` / paths on stdin -> the skills those files oblige a reader to load."""
    paths = [arg for arg in argv if not arg.startswith("-")]
    if not paths:
        paths = [line.strip() for line in sys.stdin.read().splitlines() if line.strip()]

    cwd = os.getcwd()
    root = find_repo_root(cwd)
    try:
        routes = load_routes(root) if root else None
    except RoutesUnusable as error:
        print(f"SKILL ROUTER - {error}")
        print("The table exists, so routing was asked for. Fix it; do not review against a dead table.")
        return 2
    if not routes:
        print(f"no readable {ROUTES_FILE} above {cwd} - no skill is owed")
        return 0

    owed, violations = {}, []
    for given in paths:
        rel = repo_relative(given, cwd)
        try:
            content = (root / rel).read_text(encoding="utf-8", errors="replace")
        except OSError:
            content = ""  # deleted or unreadable: the path still routes, the content gates cannot
        for route in matching_routes(routes, rel, content):
            for name in route.get("skills") or []:
                owed.setdefault(name, []).append(rel)
            violations.extend(deny_hits(route, rel, content))

    if "--json" in argv:
        json.dump({"skills": owed, "violations": violations}, sys.stdout, indent=2)
        sys.stdout.write("\n")
        return 0

    if not owed:
        print(f"no routed paths among the {len(paths)} given - no skill is owed")
    else:
        print("skill-routes.json owns these changed paths. Load each skill before judging its files:")
        for name in sorted(owed):
            files = sorted(set(owed[name]))
            print(f"\n  * {name}  ({len(files)} file(s))")
            for rel in files:
                print(f"      {rel}")
    for rel, reason in violations:
        print(f"\nDENY PATTERN HIT - a decidable violation in the tree, report it:\n  {rel}\n      {reason}")
    return 0


def verify_install(argv):
    index = argv.index(VERIFY_FLAG)
    harness = argv[index + 1].lower() if len(argv) > index + 1 else ""
    if harness not in {"claude", "codex"}:
        print(f"usage: {VERIFY_FLAG} <claude|codex>")
        return 2

    cwd = os.getcwd()
    root = find_repo_root(cwd)
    try:
        routes = load_routes(root) if root else None
    except RoutesUnusable as error:
        print(f"SKILL ROUTER - {error}")
        return 2
    if routes is None:
        print(f"no readable {ROUTES_FILE} above {cwd} - no installation to verify")
        return 2

    names = sorted({name for route in routes for name in route.get("skills") or []})
    missing = [name for name in names if skill_description(name, harness) is None]
    if missing:
        print(f"{harness} is missing {len(missing)} of {len(names)} routed skill(s):")
        for name in missing:
            print(f"  {name}")
        return 2

    print(f"{harness} resolves all {len(names)} routed skill(s) from {ROUTES_FILE}")
    return 0


def main():
    if QUERY_FLAG in sys.argv[1:]:
        sys.exit(query(sys.argv[1:]))
    if VERIFY_FLAG in sys.argv[1:]:
        sys.exit(verify_install(sys.argv[1:]))

    try:
        data = json.load(sys.stdin)
    except ValueError:
        sys.exit(0)

    tool_name = data.get("tool_name")
    if not isinstance(tool_name, str):
        sys.exit(0)
    harness = active_harness(tool_name)
    if harness is None:
        sys.exit(0)

    tool_input = data.get("tool_input") or {}
    targets = written_targets(tool_input)
    if not targets:
        sys.exit(0)

    cwd = data.get("cwd") or os.getcwd()
    root = find_repo_root(cwd)
    if root is None:
        sys.exit(0)
    try:
        routes = load_routes(root)
    except RoutesUnusable as error:
        sys.stderr.write(
            "SKILL ROUTER - blocked, the routing table itself is broken:"
            "\n\n"
            f"  {error}"
            "\n\n"
            "This repo opted into routing by carrying the file, so a table that cannot be read is a"
            " stop, not a silent pass - otherwise every write from here on is unenforced and looks"
            " fine.\n"
        )
        sys.exit(2)
    if not routes:
        sys.exit(0)

    content = written_content(tool_input)

    matched = []
    for target in targets:
        rel = repo_relative(target, cwd)
        for route in matching_routes(routes, rel, content):
            matched.append((rel, route))
    if not matched:
        sys.exit(0)

    if not claim_invocation(data, "skill-router"):
        sys.exit(0)

    # Deny patterns first: a decidable violation blocks every time, not once per session.
    for rel, route in matched:
        for _, reason in deny_hits(route, rel, content):
            sys.stderr.write(
                "SKILL ROUTER - blocked, this is a rule violation, not a reminder:\n\n"
                f"  {rel}\n  {reason}\n\n"
                f"Read the {', '.join(route.get('skills') or []) or 'owning'} skill and fix the "
                "classification before writing this file. The file was NOT written."
            )
            sys.exit(2)

    session = data.get("session_id")
    seen = load_seen(session)
    pending = []
    for rel, route in matched:
        key = route.get("path")
        if key in seen:
            continue
        pending.append((rel, route))

    if not pending:
        sys.exit(0)

    descriptions = {}
    missing = set()
    for _, route in pending:
        for name in route.get("skills") or []:
            if name not in descriptions:
                descriptions[name] = skill_description(name, harness)
            if descriptions[name] is None:
                missing.add(name)

    lines = ["SKILL ROUTER - a standard owns this path, and it has not been loaded this session:", ""]
    for rel, route in pending:
        lines += [f"  {rel}", ""]
        for name in route.get("skills") or []:
            desc = descriptions[name]
            lines.append(f"  * {name}")
            if desc:
                lines.append(f"      {desc}")
            else:
                lines.append(
                    f"      NOT INSTALLED FOR {harness.upper()} - no SKILL.md is available to the "
                    "active harness. A route pointing at a missing skill is a deployment fault, not "
                    "a reason to proceed."
                )
        if route.get("note"):
            lines.append(f"      NOTE: {route['note']}")
    if missing:
        lines += [
            "",
            f"Install or enable the missing skill(s) for {harness}, then start a new session. The file "
            "was NOT written. This route remains blocked on every attempt until every owning skill "
            "is available.",
        ]
    else:
        for _, route in pending:
            seen.add(route.get("path"))
        save_seen(session, seen)
        lines += [
            "",
            "Invoke the skill(s) above, then repeat this write. The file was NOT written. This fires "
            "once per path pattern per session, so it will not interrupt you again for this route.",
        ]
    sys.stderr.write("\n".join(lines))
    sys.exit(2)


if __name__ == "__main__":
    main()
