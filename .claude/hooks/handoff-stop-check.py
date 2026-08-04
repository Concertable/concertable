"""Stop hook: enforce the plan-handoff invariant.

Blocks a turn from ending ONLY when both are true:
  1. an active plan ledger exists (plans/**/*_PROGRESS.md whose `## Next Steps`
     section is non-terminal), and
  2. the turn's last assistant message carries no handoff pointer to that ledger.

Silent otherwise — no active plan, or the pointer is already present. Fail-open:
any error exits 0 so a broken check never wedges the session.
"""

import glob
import json
import os
import re
import sys

TERMINAL = {"", "none", "none yet", "n/a", "na", "nothing", "done", "complete",
            "completed", "closed", "terminal"}


def project_dir():
    return os.environ.get("CLAUDE_PROJECT_DIR") or os.getcwd()


def next_steps_body(text):
    m = re.search(r"^##+\s*Next Steps\s*$(.*?)(^##\s|\Z)", text,
                  re.MULTILINE | re.DOTALL)
    return m.group(1).strip() if m else None


def has_active_ledger(root):
    for path in glob.glob(os.path.join(root, "plans", "**", "*_PROGRESS.md"),
                          recursive=True):
        try:
            with open(path, encoding="utf-8") as fh:
                body = next_steps_body(fh.read())
        except OSError:
            continue
        if body is None:
            continue
        stripped = re.sub(r"[`*_#>\-\s.]", "", body).lower()
        if stripped and body.strip().lower() not in TERMINAL and len(stripped) > 12:
            return True
    return False


def last_assistant_text(transcript_path):
    if not transcript_path or not os.path.exists(transcript_path):
        return None
    text = None
    with open(transcript_path, encoding="utf-8") as fh:
        for line in fh:
            line = line.strip()
            if not line:
                continue
            try:
                obj = json.loads(line)
            except ValueError:
                continue
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

    root = project_dir()
    try:
        if not has_active_ledger(root):
            sys.exit(0)
        if has_handoff(last_assistant_text(data.get("transcript_path"))):
            sys.exit(0)
    except Exception:
        sys.exit(0)

    reason = (
        "HANDOFF INVARIANT (PROMPTS.md): an active plan ledger has outstanding "
        "`## Next Steps`. FIRST rewrite that ledger's `## Next Steps` to the current, "
        "specific next action — reflecting what actually changed this turn (findings, "
        "blockers, decisions). Handing off a stale `## Next Steps` unchanged is the loop "
        "this guards against, NOT a pass. THEN end with the pointer: `cd <worktree>` "
        "then \"Read @plans/<PLAN>_PLAN.md and @plans/<ledger>_PROGRESS.md and do what "
        "its `## Next Steps` says.\" (If the plan is genuinely closed out, delete the "
        "ledger or clear its Next Steps instead.)"
    )
    print(json.dumps({"decision": "block", "reason": reason}))


if __name__ == "__main__":
    main()
