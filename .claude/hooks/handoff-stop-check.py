"""Stop hook: enforce the plan-handoff invariant.

Blocks a turn from ending ONLY when ALL of these hold:
  1. the CURRENT turn edited a plan ledger (Edit/Write/MultiEdit whose file_path
     ends in `_PROGRESS.md`), and
  2. that edited ledger's `## Next Steps` section is non-terminal, and
  3. the turn's last assistant message carries no handoff pointer.

Rationale: the invariant only matters on a turn that actually touched a ledger.
The old behaviour fired on EVERY turn whenever any `plans/**/*_PROGRESS.md` in the
repo had an open `## Next Steps`, so unrelated/conversational turns got nagged and
were handed a pointer to a plan they never touched. Now it is scoped to the ledger
you edited this turn. Silent otherwise. Fail-open: any error exits 0 so a broken
check never wedges the session.
"""

import json
import os
import re
import sys

TERMINAL = {"", "none", "none yet", "n/a", "na", "nothing", "done", "complete",
            "completed", "closed", "terminal"}

EDIT_TOOLS = {"Edit", "Write", "MultiEdit"}


def next_steps_body(text):
    m = re.search(r"^##+\s*Next Steps\s*$(.*?)(^##\s|\Z)", text,
                  re.MULTILINE | re.DOTALL)
    return m.group(1).strip() if m else None


def is_nonterminal_ledger(path):
    try:
        with open(path, encoding="utf-8") as fh:
            body = next_steps_body(fh.read())
    except OSError:
        return False
    if body is None:
        return False
    stripped = re.sub(r"[`*_#>\-\s.]", "", body).lower()
    return bool(stripped) and body.strip().lower() not in TERMINAL and len(stripped) > 12


def read_transcript(transcript_path):
    if not transcript_path or not os.path.exists(transcript_path):
        return []
    out = []
    with open(transcript_path, encoding="utf-8") as fh:
        for line in fh:
            line = line.strip()
            if not line:
                continue
            try:
                out.append(json.loads(line))
            except ValueError:
                continue
    return out


def blocks(obj):
    content = obj.get("message", {}).get("content")
    return content if isinstance(content, list) else []


def is_user_turn_start(obj):
    # A genuine user message, not a tool_result echoed back as a "user" line.
    if obj.get("type") != "user":
        return False
    content = obj.get("message", {}).get("content")
    if isinstance(content, str):
        return True
    if isinstance(content, list):
        return not any(isinstance(b, dict) and b.get("type") == "tool_result"
                       for b in content)
    return False


def ledgers_edited_this_turn(lines):
    start = 0
    for i in range(len(lines) - 1, -1, -1):
        if is_user_turn_start(lines[i]):
            start = i + 1
            break
    paths = set()
    for obj in lines[start:]:
        if obj.get("type") != "assistant":
            continue
        for block in blocks(obj):
            if not isinstance(block, dict) or block.get("type") != "tool_use":
                continue
            if block.get("name") not in EDIT_TOOLS:
                continue
            fp = (block.get("input") or {}).get("file_path", "")
            if isinstance(fp, str) and fp.endswith("_PROGRESS.md"):
                paths.add(fp)
    return paths


def last_assistant_text(lines):
    text = None
    for obj in lines:
        if obj.get("type") != "assistant":
            continue
        content = obj.get("message", {}).get("content")
        if isinstance(content, str):
            text = content
        elif isinstance(content, list):
            text = "".join(p.get("text", "") for p in content
                           if isinstance(p, dict) and p.get("type") == "text")
    return text


def has_handoff(text):
    if not text:
        return False
    low = text.lower()
    return "next steps" in low or ("read @" in low and "_progress" in low)


def main():
    try:
        data = json.load(sys.stdin)
    except ValueError:
        sys.exit(0)

    if data.get("stop_hook_active"):
        sys.exit(0)

    try:
        lines = read_transcript(data.get("transcript_path"))
        active = [p for p in ledgers_edited_this_turn(lines)
                  if is_nonterminal_ledger(p)]
        if not active:
            sys.exit(0)
        if has_handoff(last_assistant_text(lines)):
            sys.exit(0)
    except Exception:
        sys.exit(0)

    names = ", ".join(sorted(os.path.basename(p) for p in active))
    reason = (
        "HANDOFF INVARIANT (PROMPTS.md): you edited a plan ledger this turn (" + names +
        ") whose `## Next Steps` is still open, but your reply has no handoff pointer. "
        "Ensure that ledger's `## Next Steps` reflects what changed this turn, then end "
        "with: `cd <worktree>` then \"Read @plans/<PLAN>_PLAN.md and "
        "@plans/<ledger>_PROGRESS.md and do what its `## Next Steps` says.\" "
        "(If the plan is closed out, clear its Next Steps instead.)"
    )
    print(json.dumps({"decision": "block", "reason": reason}))


if __name__ == "__main__":
    main()
