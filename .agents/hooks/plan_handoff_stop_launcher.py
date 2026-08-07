import json
import runpy
import subprocess
import sys
from pathlib import Path


IMPLEMENTATION = ".agents/hooks/plan_handoff_stop.py"


def blob_oid(root, revision=None):
    target = f"{revision}:{IMPLEMENTATION}" if revision else str(root / IMPLEMENTATION)
    command = ["git", "-C", str(root), "rev-parse", target] if revision else [
        "git",
        "-C",
        str(root),
        "hash-object",
        target,
    ]
    try:
        result = subprocess.run(
            command,
            capture_output=True,
            text=True,
            timeout=5,
        )
    except (OSError, subprocess.SubprocessError):
        return None
    return result.stdout.strip() if result.returncode == 0 else None


def implementation_is_current(root):
    checked_out = blob_oid(root)
    origin_main = blob_oid(root, "origin/main")
    return bool(checked_out and origin_main and checked_out == origin_main)


def main():
    root = Path(__file__).resolve().parents[2]
    if not implementation_is_current(root):
        result = {
            "decision": "block",
            "reason": (
                "HANDOFF GATE ERROR: this checkout's plan handoff hook differs from origin/main. "
                "Sync this branch with current main before relying on plan handoffs."
            ),
        }
        json.dump(result, sys.stdout)
        sys.stdout.write("\n")
        return
    runpy.run_path(str(root / IMPLEMENTATION), run_name="__main__")


if __name__ == "__main__":
    main()
