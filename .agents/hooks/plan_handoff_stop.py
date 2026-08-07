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
WORKDIR = re.compile(r"(?:workdir|cwd)\s*:\s*[\"']([^\"']+)", re.IGNORECASE)


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
    first_line = next((line.strip().lower() for line in body.splitlines() if line.strip()), "")
    return (
        not normalized
        or plain in TERMINAL
        or first_line.startswith("waiting for ")
        or first_line.startswith("blocked: waiting for ")
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


def intentional_values(record):
    candidate = payload(record)
    if candidate is None:
        return
    if genuine_user_message(record):
        yield from user_content(candidate.get("content"))
    if candidate.get("type") in {"custom_tool_call", "function_call"}:
        tool_input = candidate.get("input", candidate.get("arguments"))
        yield from strings(tool_input)
    content = candidate.get("content")
    if isinstance(content, list):
        for item in content:
            if isinstance(item, dict) and item.get("type") in {"tool_use", "function_call"}:
                yield from strings(item.get("input", item.get("arguments")))


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


def transcript_ledgers(records, cwd):
    paths = set()
    values = [value for record in records for value in intentional_values(record)]
    bases = {Path(cwd).resolve()}
    for value in values:
        for match in WORKDIR.finditer(value):
            bases.add(Path(match.group(1)).resolve())
        for match in ABSOLUTE_LEDGER.finditer(value):
            add_if_ledger(paths, Path(match.group(0).rstrip("`),.;")))
    for value in values:
        for match in RELATIVE_LEDGER.finditer(value):
            relative = Path(match.group(0).replace("\\", "/").rstrip("`),.;"))
            for base in bases:
                add_if_ledger(paths, base / relative)
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


def evaluate(data):
    cwd = Path(data.get("cwd") or ".").resolve()
    records = transcript_turn(data.get("transcript_path") or data.get("transcriptPath"))
    ledgers = transcript_ledgers(records, cwd)
    if not records:
        ledgers |= branch_ledgers(git_root(cwd))
    active = []
    for path in sorted(ledgers):
        body = next_steps(path.read_text(encoding="utf-8"))
        if body is None:
            return {
                "decision": "block",
                "reason": f"HANDOFF GATE: {path.name} is missing its required `## Next Steps` section.",
            }
        if not is_terminal(body):
            active.append((path, expected_pointer(path)))
    if not active:
        return {}
    message = (data.get("last_assistant_message") or data.get("lastAssistantMessage") or "").replace(
        "\r\n", "\n"
    )
    missing = [(path, pointer) for path, pointer in active if pointer not in message]
    ending = message.rstrip()
    ends_with_pointer = any(
        ending.endswith(pointer) or ending.endswith(f"{pointer}\n```") for _, pointer in active
    )
    if not missing and ends_with_pointer:
        return {}
    if not missing:
        missing = active
    pointers = "\n\n".join(f"```text\n{pointer}\n```" for _, pointer in missing)
    names = ", ".join(path.name for path, _ in missing)
    return {
        "decision": "block",
        "reason": (
            f"HANDOFF GATE: {names} has non-terminal `## Next Steps`, but the final response "
            "omitted its exact continuation pointer. Local implementation completion is not lifecycle "
            f"completion. End the response with:\n\n{pointers}"
        ),
    }


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
