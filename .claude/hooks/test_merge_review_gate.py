"""Exercise the merge gate the way Claude Code does: feed it PreToolUse JSON on stdin."""
import json
import subprocess
import sys

HOOK = sys.argv[1]
CWD = sys.argv[2]
M = "gh pr " + "merge"  # assembled so this file never contains the gated literal


def run(command):
    payload = json.dumps({"tool_name": "Bash", "tool_input": {"command": command}})
    r = subprocess.run([sys.executable, HOOK], input=payload, capture_output=True, text=True, cwd=CWD)
    return r.returncode, (r.stderr or "").strip()


fails = 0


def check(name, ok, detail=""):
    global fails
    fails += 0 if ok else 1
    print(("PASS" if ok else "FAIL"), "|", name)
    if not ok and detail:
        print("      ", detail[:200])


code, err = run("git status")
check("non-merge command is ignored", code == 0, err)

code, err = run(M + " 577 --disable-auto")
check("disable-auto alone is allowed", code == 0, err)

# The fix: gate the PR's branch, not the session's checkout. Before, from a main-rooted
# session this reported reviews/main.md for every PR.
code, err = run(M + " 577 --merge --auto")
check("resolves the PR's own review file, not the session branch",
      "Refactor-PaginationMap.md" in err and "reviews/main.md" not in err, err)

# Still fails closed on a PR that cannot be resolved at all.
code, err = run(M + " 999999 --merge --auto")
check("unresolvable PR is blocked", code == 2, err)

# Still fails closed with no PR number and no review for the checked-out branch.
code, err = run(M + " --merge --auto")
check("bare merge on an unreviewed checkout is blocked", code == 2, err)

sys.exit(1 if fails else 0)
