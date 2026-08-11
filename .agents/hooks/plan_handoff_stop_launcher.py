import json
import runpy
import subprocess
import sys
from pathlib import Path


IMPLEMENTATION = ".agents/hooks/plan_handoff_stop.py"
TRUSTED_FILES = (
    ".agents/hooks/plan_handoff_stop_launcher.py",
    IMPLEMENTATION,
    ".agents/hooks/plan_graph.py",
)


def blob_oid(root, implementation=IMPLEMENTATION, revision=None):
    target = f"{revision}:{implementation}" if revision else str(root / implementation)
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
    return all(
        (checked_out := blob_oid(root, implementation))
        and (origin_main := blob_oid(root, implementation, "origin/main"))
        and checked_out == origin_main
        for implementation in TRUSTED_FILES
    )


def main():
    root = Path(__file__).resolve().parents[2]
    if not implementation_is_current(root):
        result = {
            "decision": "block",
            "reason": (
                "HANDOFF GATE ERROR: this checkout's plan handoff hook bundle differs from origin/main. "
                "Sync this branch with current main before relying on plan handoffs."
            ),
        }
        json.dump(result, sys.stdout)
        sys.stdout.write("\n")
        return
    runpy.run_path(str(root / IMPLEMENTATION), run_name="__main__")


if __name__ == "__main__":
    main()
