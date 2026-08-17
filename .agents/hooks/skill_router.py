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
  advice. The build enforces the same test-tier rules (`api/TestConventions.targets`) because a hook
  matcher is per-tool and never sees `dotnet new` or a shell heredoc.

Contract: exit 0 = allow, exit 2 = block with stderr fed back to the agent. Anything unexpected exits
0 - a broken router must not wedge every write, since the build gate is the tier that guarantees.
"""

import hashlib
import json
import os
import re
import sys
import tempfile
from pathlib import Path


ROUTES_FILE = ".agents/skill-routes.json"
WRITE_TOOLS = {"Write", "Edit", "MultiEdit", "NotebookEdit"}
SKILL_ROOTS = (Path.home() / ".agents" / "skills", Path.home() / ".claude" / "skills")


def repo_relative(path, cwd):
    """POSIX repo-relative path, so a route regex never has to know about drive letters."""
    try:
        p = Path(path).resolve()
    except OSError:
        return str(path).replace("\\", "/")
    for base in (Path(cwd).resolve(), *Path(cwd).resolve().parents):
        if (base / ".git").exists():
            try:
                return p.relative_to(base).as_posix()
            except ValueError:
                break
    return p.as_posix()


def written_content(tool_input):
    """Every string the tool would put into the file, concatenated for pattern matching."""
    parts = [
        tool_input.get("content"),
        tool_input.get("new_string"),
        tool_input.get("new_source"),
    ]
    for edit in tool_input.get("edits") or []:
        if isinstance(edit, dict):
            parts.append(edit.get("new_string"))
    return "\n".join(p for p in parts if isinstance(p, str))


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


def main():
    try:
        data = json.load(sys.stdin)
    except ValueError:
        sys.exit(0)

    if data.get("tool_name") not in WRITE_TOOLS:
        sys.exit(0)

    tool_input = data.get("tool_input") or {}
    target = tool_input.get("file_path") or tool_input.get("notebook_path")
    if not isinstance(target, str) or not target:
        sys.exit(0)

    cwd = data.get("cwd") or os.getcwd()
    root = find_repo_root(cwd)
    if root is None:
        sys.exit(0)
    try:
        routes = json.loads((root / ROUTES_FILE).read_text(encoding="utf-8")).get("routes", [])
    except (OSError, ValueError):
        sys.exit(0)

    rel = repo_relative(target, cwd)
    content = written_content(tool_input)

    matched = []
    for route in routes:
        pattern = route.get("path")
        if not pattern or not re.search(pattern, rel):
            continue
        needle = route.get("content_requires")
        if needle and not re.search(needle, content):
            continue
        matched.append(route)

    if not matched:
        sys.exit(0)

    # Deny patterns first: a decidable violation blocks every time, not once per session.
    for route in matched:
        for rule in route.get("deny") or []:
            pattern = rule.get("pattern")
            if pattern and re.search(pattern, content):
                sys.stderr.write(
                    "SKILL ROUTER - blocked, this is a rule violation, not a reminder:\n\n"
                    f"  {rel}\n  {rule.get('reason', '')}\n\n"
                    f"Read the {', '.join(route.get('skills') or []) or 'owning'} skill and fix the "
                    "classification before writing this file. The file was NOT written."
                )
                sys.exit(2)

    session = data.get("session_id")
    seen = load_seen(session)
    pending = []
    for route in matched:
        key = route.get("path")
        if key in seen:
            continue
        pending.append(route)
        seen.add(key)

    if not pending:
        sys.exit(0)

    save_seen(session, seen)

    lines = [
        "SKILL ROUTER - a standard owns this path, and it has not been loaded this session:",
        "",
        f"  {rel}",
        "",
    ]
    for route in pending:
        for name in route.get("skills") or []:
            desc = skill_description(name)
            lines.append(f"  * {name}")
            if desc:
                lines.append(f"      {desc}")
            else:
                lines.append(
                    "      NOT INSTALLED - run dotagents/.agents/deploy-skills.ps1. A route pointing "
                    "at a missing skill is a deployment fault, not a reason to proceed."
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
