import json
import sys

try:
    data = json.load(sys.stdin)
except Exception:
    data = {}

# Second pass (this hook already fired and forced a continue) -> let the turn end.
if data.get("stop_hook_active"):
    sys.exit(0)

reason = (
    "HANDOFF SELF-CHECK (PROMPTS.md + AGENTS.md). Before this turn ends, verify: is ANY work "
    "unfinished, or is any push / merge / publish / platform-sync / PR / plan gate still pending? "
    "If YES, the turn MUST end with the handoff prompt, not a status report alone. For plan work "
    "that is the ledger pointer: `cd <worktree>` then \"Read @plans/<PLAN>.md and "
    "@plans/<PLAN>_PROGRESS.md, then do what the ledger's `## Next Steps` says.\" If everything is "
    "genuinely complete and no gate is pending, stop normally now."
)

print(json.dumps({"decision": "block", "reason": reason}))
