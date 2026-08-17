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


ROUTES_FILE = ".agents/skill-routes.json"
# Lowercased, because the two harnesses do not agree on casing or on names. Claude writes through
# Write/Edit/MultiEdit/NotebookEdit; Codex writes through apply_patch, and matching only Claude's
# vocabulary is how this hook spent its first life doing nothing at all in a Codex session.
WRITE_TOOLS = {
    "write",
    "edit",
    "multiedit",
    "notebookedit",
    "apply_patch",
    "edit_file",
    "write_file",
    "multi_edit",
}
PATH_KEYS = ("file_path", "notebook_path", "path", "filepath")
# An apply_patch envelope names its files inside the patch body, so a key lookup alone sees none of them.
PATCH_FILE_TARGET = re.compile(r"\*\*\*\s+(?:Add|Update|Delete)\s+File:\s*([^\r\n]+)", re.IGNORECASE)
PATCH_ADDED_LINE = re.compile(r"^\+(?!\+\+)(.*)$", re.MULTILINE)
SKILL_ROOTS = (Path.home() / ".agents" / "skills", Path.home() / ".claude" / "skills")
QUERY_FLAG = "--skills-for"


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


def skill_description(name):
    """The skill's own description, so the rule text has exactly one home."""
    for root in SKILL_ROOTS:
        skill = root / name / "SKILL.md"
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


def load_routes(root):
    try:
        return json.loads((root / ROUTES_FILE).read_text(encoding="utf-8")).get("routes", [])
    except (OSError, ValueError):
        return None


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
    routes = load_routes(root) if root else None
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


def main():
    if QUERY_FLAG in sys.argv[1:]:
        sys.exit(query(sys.argv[1:]))

    try:
        data = json.load(sys.stdin)
    except ValueError:
        sys.exit(0)

    tool_name = data.get("tool_name")
    if not isinstance(tool_name, str) or tool_name.lower() not in WRITE_TOOLS:
        sys.exit(0)

    tool_input = data.get("tool_input") or {}
    targets = written_targets(tool_input)
    if not targets:
        sys.exit(0)

    cwd = data.get("cwd") or os.getcwd()
    root = find_repo_root(cwd)
    if root is None:
        sys.exit(0)
    routes = load_routes(root)
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
        seen.add(key)

    if not pending:
        sys.exit(0)

    save_seen(session, seen)

    lines = ["SKILL ROUTER - a standard owns this path, and it has not been loaded this session:", ""]
    for rel, route in pending:
        lines += [f"  {rel}", ""]
        for name in route.get("skills") or []:
            desc = skill_description(name)
            lines.append(f"  * {name}")
            if desc:
                lines.append(f"      {desc}")
            else:
                lines.append(
                    "      NOT INSTALLED - no SKILL.md under ~/.agents/skills or ~/.claude/skills. A "
                    "route pointing at a missing skill is a deployment fault, not a reason to proceed."
                )
        if route.get("note"):
            lines.append(f"      NOTE: {route['note']}")
    lines += [
        "",
        "Invoke the skill(s) above, then repeat this write. The file was NOT written. This fires once "
        "per path pattern per session, so it will not interrupt you again for this route.",
    ]
    sys.stderr.write("\n".join(lines))
    sys.exit(2)


if __name__ == "__main__":
    main()
