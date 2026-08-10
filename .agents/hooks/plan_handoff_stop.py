import json
import re
import subprocess
import sys
from pathlib import Path


TERMINAL = {
    "",
    "closed",
    "complete",
    "completed",
    "done",
    "n/a",
    "na",
    "none",
    "nothing",
    "terminal",
}

ABSOLUTE_LEDGER = re.compile(
    r"[A-Za-z]:[\\/][^\"'\r\n<>|]*?[\\/]plans[\\/][A-Za-z0-9_.() \\/-]+?_PROGRESS\.md",
    re.IGNORECASE,
)
RELATIVE_LEDGER = re.compile(
    r"plans[\\/][A-Za-z0-9_.() \\/-]+?_PROGRESS\.md",
    re.IGNORECASE,
)
WORKDIR = re.compile(r"[\"']?(?:workdir|cwd)[\"']?\s*:\s*[\"']([^\"']+)", re.IGNORECASE)
CD_WORKDIR = re.compile(
    r"^\s*cd\s+(?:\"([^\"]+)\"|'([^']+)'|([^\r\n]+?))\s*$",
    re.IGNORECASE | re.MULTILINE,
)
REGISTERED_OWNER_WAIT = re.compile(r"\bdownstream handoffs\b", re.IGNORECASE)
BLOCKED_OR_WAITING = re.compile(r"\b(?:blocked|waiting)\b", re.IGNORECASE)
SUPPRESS_RESUME_PROMPT = re.compile(
    r"\bdo not\b(?:(?![.!?](?:\s|$)).){0,240}"
    r"\b(?:emit|include|surface)\b(?:(?![.!?](?:\s|$)).){0,120}"
    r"\b(?:resume|continuation|handoff)(?:\s+prompt)?\b",
    re.IGNORECASE | re.DOTALL,
)
MUTATING_TOOL_NAMES = {
    "apply_patch",
    "edit",
    "edit_file",
    "multiedit",
    "multi_edit",
    "write",
    "write_file",
}
MUTATING_PATH_KEYS = {"path", "filepath"}
PATCH_FILE_TARGET = re.compile(
    r"\*\*\*\s+(?:Add|Update|Delete)\s+File:\s*"
    r"((?:[A-Za-z]:[\\/])?[^\"'\r\n<>|]*?_PROGRESS\.md)",
    re.IGNORECASE,
)


def next_steps(text):
    match = re.search(
        r"^##+\s*Next Steps\s*$(.*?)(^##\s|\Z)",
        text,
        re.MULTILINE | re.DOTALL | re.IGNORECASE,
    )
    return match.group(1).strip() if match else None


def is_terminal(body):
    if body is None:
        return True
    normalized = re.sub(r"[`*_#>\-\s.]", "", body).lower()
    plain = body.strip().lower()
    return not normalized or plain in TERMINAL


def blocker_details(body):
    if body is None:
        return None
    lines = [line.strip() for line in body.splitlines() if line.strip()]
    names = ("Blocked", "Unblock action", "Resume when")
    if len(lines) < len(names):
        return None
    values = []
    for line, name in zip(lines, names):
        prefix = f"{name}:"
        if not line.startswith(prefix):
            return None
        value = line.removeprefix(prefix).strip()
        if not value:
            return None
        values.append(value)
    return tuple(values)


def looks_like_legacy_blocker(body):
    if body is None:
        return False
    first_line = next((line.strip().lower() for line in body.splitlines() if line.strip()), "")
    return (
        first_line.startswith("waiting for ")
        or first_line.startswith("blocked:")
        or (
            BLOCKED_OR_WAITING.search(body)
            and REGISTERED_OWNER_WAIT.search(body)
            and SUPPRESS_RESUME_PROMPT.search(body)
        )
    )


def strings(value):
    if isinstance(value, str):
        yield value
    elif isinstance(value, dict):
        for item in value.values():
            yield from strings(item)
    elif isinstance(value, list):
        for item in value:
            yield from strings(item)


def workdirs(value):
    if isinstance(value, str):
        for match in WORKDIR.finditer(value):
            yield match.group(1)
        for match in CD_WORKDIR.finditer(value):
            yield next(item for item in match.groups() if item is not None).strip()
    elif isinstance(value, dict):
        for name, item in value.items():
            if name.lower() in {"cwd", "workdir"} and isinstance(item, str):
                yield item
            else:
                yield from workdirs(item)
    elif isinstance(value, list):
        for item in value:
            yield from workdirs(item)


def user_content(value):
    if isinstance(value, str):
        yield value
    elif isinstance(value, list):
        for item in value:
            if isinstance(item, str):
                yield item
            elif isinstance(item, dict) and item.get("type") in {"text", "input_text"}:
                text = item.get("text")
                if isinstance(text, str):
                    yield text


def payload(value):
    if not isinstance(value, dict):
        return None
    if isinstance(value.get("payload"), dict):
        return value["payload"]
    if value.get("type") in {"user", "assistant"} and isinstance(value.get("message"), dict):
        return value["message"]
    return value


def genuine_user_message(value):
    candidate = payload(value)
    if candidate is None or candidate.get("role") != "user":
        return False
    content = candidate.get("content")
    if isinstance(content, list):
        if any(
            isinstance(item, dict) and item.get("type") in {"tool_result", "function_call_output"}
            for item in content
        ):
            return False
    return not any("<hook_prompt" in text for text in user_content(content))


def structured_mutation_targets(value):
    if isinstance(value, dict):
        for name, item in value.items():
            normalized = name.casefold().replace("-", "").replace("_", "")
            if normalized in MUTATING_PATH_KEYS and isinstance(item, str):
                yield item
            elif isinstance(item, (dict, list)):
                yield from structured_mutation_targets(item)
    elif isinstance(value, list):
        for item in value:
            yield from structured_mutation_targets(item)


def patch_mutation_targets(value):
    for item in strings(value):
        yield from (match.group(1) for match in PATCH_FILE_TARGET.finditer(item))


def mutating_tool_input(name, tool_input):
    normalized = (name or "").casefold().replace("-", "_")
    targets = list(patch_mutation_targets(tool_input))
    if normalized.rsplit(".", 1)[-1] in MUTATING_TOOL_NAMES:
        targets.extend(structured_mutation_targets(tool_input))
    if not targets:
        return None
    return {
        "targets": list(dict.fromkeys(targets)),
        "workdirs": [{"workdir": item} for item in workdirs(tool_input)],
    }


def intentional_contexts(record):
    candidate = payload(record)
    if candidate is None:
        return
    if genuine_user_message(record):
        yield {
            "source": "user",
            "values": list(user_content(candidate.get("content"))),
        }
    if candidate.get("type") in {"custom_tool_call", "function_call"}:
        tool_input = candidate.get("input", candidate.get("arguments"))
        context = mutating_tool_input(candidate.get("name"), tool_input)
        if context is not None:
            context["source"] = "mutation"
            yield context
    content = candidate.get("content")
    if isinstance(content, list):
        for item in content:
            if isinstance(item, dict) and item.get("type") in {"tool_use", "function_call"}:
                tool_input = item.get("input", item.get("arguments"))
                context = mutating_tool_input(item.get("name"), tool_input)
                if context is not None:
                    context["source"] = "mutation"
                    yield context


def transcript_turn(path):
    if not path or not Path(path).is_file():
        return []
    records = []
    with Path(path).open(encoding="utf-8") as stream:
        for line in stream:
            try:
                records.append(json.loads(line))
            except (TypeError, ValueError):
                continue
    start = 0
    for index in range(len(records) - 1, -1, -1):
        if genuine_user_message(records[index]):
            start = index
            break
    return records[start:]


def git_root(cwd):
    try:
        result = subprocess.run(
            ["git", "-C", str(cwd), "rev-parse", "--show-toplevel"],
            capture_output=True,
            check=True,
            text=True,
            timeout=5,
        )
        return Path(result.stdout.strip()).resolve()
    except (OSError, subprocess.SubprocessError):
        return None


def git_branch(root):
    try:
        result = subprocess.run(
            ["git", "-C", str(root), "branch", "--show-current"],
            capture_output=True,
            check=True,
            text=True,
            timeout=5,
        )
        return result.stdout.strip()
    except (OSError, subprocess.SubprocessError):
        return ""


def metadata(text, name):
    match = re.search(rf"^- {re.escape(name)}:\s*`?([^`\r\n]+)`?\s*$", text, re.MULTILINE)
    return match.group(1).strip() if match else ""


def add_if_ledger(paths, candidate):
    try:
        resolved = candidate.resolve()
    except OSError:
        return
    if resolved.is_file() and resolved.name.upper().endswith("_PROGRESS.MD"):
        paths.add(resolved)


def context_root(path):
    candidate = Path(path).resolve()
    return git_root(candidate) or candidate


def context_owns_ledger(path, bases, allow_missing_owner=False):
    text = path.read_text(encoding="utf-8")
    declared_worktree = metadata(text, "Worktree")
    roots = {context_root(base) for base in bases}
    if declared_worktree:
        owner = Path(declared_worktree).resolve()
        if owner.is_dir():
            return owner in roots
        return allow_missing_owner
    owner_branch = metadata(text, "Branch")
    if owner_branch:
        return any(git_branch(root) == owner_branch for root in roots)
    return ledger_root(path) in roots


def transcript_ledgers(records, cwd):
    paths = set()
    contexts = [context for record in records for context in intentional_contexts(record)]
    for context in contexts:
        values = list(strings(context))
        explicit_bases = {Path(path).resolve() for path in workdirs(context)}
        bases = explicit_bases or {Path(cwd).resolve()}
        candidates = set()
        for value in values:
            absolute_matches = list(ABSOLUTE_LEDGER.finditer(value))
            for match in absolute_matches:
                add_if_ledger(candidates, Path(match.group(0).rstrip("`),.;")))
            for match in RELATIVE_LEDGER.finditer(value):
                absolute_spans = (item.span() for item in absolute_matches)
                if any(start <= match.start() < end for start, end in absolute_spans):
                    continue
                relative = Path(match.group(0).replace("\\", "/").rstrip("`),.;"))
                for base in bases:
                    add_if_ledger(candidates, base / relative)
        for path in candidates:
            ownership_bases = bases
            if context.get("source") == "mutation" and not explicit_bases:
                ownership_bases = {ledger_root(path)}
            if context_owns_ledger(
                path,
                ownership_bases,
                allow_missing_owner=context.get("source") == "mutation",
            ):
                paths.add(path)
    return paths


def branch_ledgers(root):
    if root is None:
        return set()
    branch = git_branch(root)
    paths = set()
    for path in (root / "plans").rglob("*_PROGRESS.md"):
        try:
            text = path.read_text(encoding="utf-8")
        except OSError:
            continue
        worktree = metadata(text, "Worktree")
        owner_branch = metadata(text, "Branch")
        same_worktree = False
        if worktree:
            try:
                same_worktree = Path(worktree).resolve() == root
            except OSError:
                same_worktree = False
        if same_worktree or (branch and owner_branch == branch):
            paths.add(path.resolve())
    return paths


def ledger_root(path):
    for parent in path.parents:
        if parent.name.lower() == "plans":
            return parent.parent
    raise ValueError(f"Ledger is not below plans/: {path}")


def logical_ledger_key(path):
    return path.relative_to(ledger_root(path)).as_posix().casefold()


def owner_copy_score(path):
    text = path.read_text(encoding="utf-8")
    declared_worktree = metadata(text, "Worktree")
    if not declared_worktree:
        return 1
    try:
        return 0 if Path(declared_worktree).resolve() == ledger_root(path) else 1
    except OSError:
        return 1


def canonical_ledgers(paths):
    grouped = {}
    for path in paths:
        grouped.setdefault(logical_ledger_key(path), []).append(path)
    return {
        min(candidates, key=lambda path: (owner_copy_score(path), str(path).casefold()))
        for candidates in grouped.values()
    }


def expected_pointer(path):
    root = ledger_root(path)
    plan = path.with_name(path.name.removesuffix("_PROGRESS.md") + "_PLAN.md")
    if not plan.is_file():
        raise ValueError(f"Missing companion plan for {path}")
    text = path.read_text(encoding="utf-8")
    declared_worktree = metadata(text, "Worktree")
    owner_branch = metadata(text, "Branch")
    if declared_worktree and Path(declared_worktree).is_dir():
        opener_path = str(Path(declared_worktree).resolve())
        opener = f'cd "{opener_path}"' if " " in opener_path else f"cd {opener_path}"
    elif owner_branch:
        opener = f"/worktree create {owner_branch}"
    else:
        opener_path = str(root)
        opener = f'cd "{opener_path}"' if " " in opener_path else f"cd {opener_path}"
    plan_relative = plan.relative_to(root).as_posix()
    ledger_relative = path.relative_to(root).as_posix()
    return (
        f"{opener}\n"
        f"Read @{plan_relative} and @{ledger_relative} and do what its `## Next Steps` says."
    )


def handoff_summary(body):
    summary = re.sub(r"\s+", " ", body.split("\n\n", 1)[0]).strip()
    if len(summary) > 240:
        summary = summary[:237].rstrip() + "..."
    return summary


def handoff_reason(path, body):
    summary = handoff_summary(body)
    text = path.read_text(encoding="utf-8")
    worktree = metadata(text, "Worktree") or str(ledger_root(path))
    return (
        f"Why: `{path.name}` owns unfinished work from this turn: {summary}\n"
        f"Only run this continuation if no agent or session is already working in `{worktree}`."
    )


def expected_handoff(path, pointer, body):
    return f"{handoff_reason(path, body)}\n\n```text\n{pointer}\n```"


def normalized_handoff_text(value):
    value = re.sub(r"```(?:text)?", "", value, flags=re.IGNORECASE)
    value = value.replace("`", "")
    value = re.sub(r"(?<=\S)-\s+(?=\S)", "-", value)
    return re.sub(r"\s+", " ", value).strip()


def handoff_present(message, path, pointer, body):
    text = path.read_text(encoding="utf-8")
    worktree = metadata(text, "Worktree") or str(ledger_root(path))
    parts = (
        f"Why: {path.name} owns unfinished work from this turn:",
        handoff_summary(body).removesuffix("..."),
        f"Only run this continuation if no agent or session is already working in {worktree}.",
        pointer,
    )
    normalized_message = normalized_handoff_text(message)
    position = 0
    for part in parts:
        normalized_part = normalized_handoff_text(part)
        match = normalized_message.find(normalized_part, position)
        if match < 0:
            return False
        position = match + len(normalized_part)
    return True


def ends_with_pointer(message, pointer):
    return normalized_handoff_text(message).endswith(normalized_handoff_text(pointer))


def evaluate(data):
    cwd = Path(data.get("cwd") or ".").resolve()
    records = transcript_turn(data.get("transcript_path") or data.get("transcriptPath"))
    ledgers = transcript_ledgers(records, cwd)
    if not records:
        ledgers |= branch_ledgers(git_root(cwd))
    ledgers = canonical_ledgers(ledgers)
    active = []
    blocked = []
    malformed_blockers = []
    for path in sorted(ledgers):
        body = next_steps(path.read_text(encoding="utf-8"))
        if body is None:
            return {
                "decision": "block",
                "reason": f"HANDOFF GATE: {path.name} is missing its required `## Next Steps` section.",
            }
        details = blocker_details(body)
        if details:
            blocked.append((path, expected_pointer(path), details))
        elif looks_like_legacy_blocker(body):
            malformed_blockers.append(path)
        elif not is_terminal(body):
            pointer = expected_pointer(path)
            active.append((path, pointer, body, expected_handoff(path, pointer, body)))
    if malformed_blockers:
        names = ", ".join(path.name for path in malformed_blockers)
        return {
            "decision": "block",
            "reason": (
                f"BLOCKER HANDOFF GATE: {names} describes blocked work without the required "
                "three-line contract. Start `## Next Steps` with non-empty `Blocked:`, "
                "`Unblock action:`, and `Resume when:` lines. Do not emit the blocked plan's "
                "continuation pointer."
            ),
        }
    active = list(
        {
            pointer: (path, pointer, body, handoff)
            for path, pointer, body, handoff in active
        }.values()
    )
    message = (data.get("last_assistant_message") or data.get("lastAssistantMessage") or "").replace(
        "\r\n", "\n"
    )
    message = "\n".join(line.rstrip() for line in message.split("\n"))
    blocked_failures = []
    for path, pointer, details in blocked:
        if pointer in message:
            blocked_failures.append(
                f"{path.name}: remove the blocked plan's continuation pointer"
            )
        required = tuple(
            f"{name}: {value}"
            for name, value in zip(("Blocked", "Unblock action", "Resume when"), details)
        )
        missing = [line for line in required if line not in message]
        if missing:
            blocked_failures.append(
                f"{path.name}: report these exact lines: " + "; ".join(missing)
            )
    failures = []
    if blocked_failures:
        failures.append("BLOCKER HANDOFF GATE: " + " | ".join(blocked_failures))
    if active:
        missing = [
            (path, pointer, body, handoff)
            for path, pointer, body, handoff in active
            if not handoff_present(message, path, pointer, body)
        ]
        ends_with_handoff = any(ends_with_pointer(message, pointer) for _, pointer, _, _ in active)
        if missing or not ends_with_handoff:
            if not missing:
                missing = active
            handoffs = "\n\n".join(handoff for _, _, _, handoff in missing)
            names = ", ".join(path.name for path, _, _, _ in missing)
            failures.append(
                f"HANDOFF GATE: {names} has non-terminal `## Next Steps`, but the final response "
                "omitted its explained, collision-safe continuation. Local implementation completion "
                f"is not lifecycle completion. End the response with:\n\n{handoffs}"
            )
    if failures:
        return {"decision": "block", "reason": "\n\n".join(failures)}
    return {}


def main():
    try:
        data = json.loads(sys.stdin.buffer.read().decode("utf-8-sig"))
        result = evaluate(data)
    except Exception as error:
        result = {
            "decision": "block",
            "reason": f"HANDOFF GATE ERROR: validation could not complete: {error}",
        }
    json.dump(result, sys.stdout)
    sys.stdout.write("\n")


if __name__ == "__main__":
    main()
