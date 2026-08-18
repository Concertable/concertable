"""Exercise the merge gate the way Claude Code does: feed it PreToolUse JSON on stdin.

Run: python3 .claude/hooks/test_merge_review_gate.py <hook-path> <repo-root>
"""
import importlib.util
import json
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

HOOK = str(Path(sys.argv[1]).resolve())  # absolute: cases run the hook from other directories
CWD = sys.argv[2]
M = "gh pr " + "merge"  # assembled so this file never contains the gated literal

spec = importlib.util.spec_from_file_location("gate", HOOK)
gate = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gate)

fails = 0


def check(name, ok, detail=""):
    global fails
    fails += 0 if ok else 1
    print(("PASS" if ok else "FAIL"), "|", name)
    if not ok and detail:
        print("      ", str(detail)[:200])


def run_in(command, cwd):
    payload = json.dumps({"tool_name": "Bash", "cwd": cwd, "tool_input": {"command": command}})
    r = subprocess.run([sys.executable, HOOK], input=payload, capture_output=True, text=True, cwd=cwd)
    return r.returncode, (r.stderr or "").strip()


def run(command):
    return run_in(command, CWD)


# --- invocation matching: only a real merge is gated -------------------------------
check("a real invocation is detected", gate.invokes_merge(M + " 613 --merge --auto"))
check("chained after && is detected", gate.invokes_merge("git fetch && " + M + " 613 --auto"))
check("env-prefixed invocation is detected", gate.invokes_merge("GH_TOKEN=x " + M + " 613 --auto"))
check("merely QUOTING the command is not a merge",
      not gate.invokes_merge('echo "run ' + M + ' 613 --merge --auto to land it"'))
check("writing it into a file is not a merge",
      not gate.invokes_merge("cat > pr.md <<'EOF'\nUse " + M + " 613 --admin\nEOF"))

# --- enabling-token detection ------------------------------------------------------
check("--disable-auto alone is NOT an enable", not gate.is_merge_enable(M + " 613 --disable-auto"))
check("--auto is an enable", gate.is_merge_enable(M + " 613 --merge --auto"))
check("the documented re-assert compound still gates",
      gate.is_merge_enable(M + " 613 --disable-auto && " + M + " 613 --merge --auto"))
check("bare merge still gates", gate.is_merge_enable(M + " 613"))

# --- worktree resolution (adopted from #495) ---------------------------------------
check("last cd before the merge wins",
      gate.merge_target_dir('cd /a && cd "/b" && ' + M + " 1 --auto", {}) == "/b")
check("a cd AFTER the merge is ignored",
      gate.merge_target_dir(M + " 1 --auto && cd /elsewhere", {"cwd": "/main"}) == "/main")
check("quoted paths are unwrapped",
      gate.merge_target_dir("cd '/repos/my wt' && " + M + " 1 --auto", {}) == "/repos/my wt")

# --- jurisdiction: this gate speaks for THIS repo only -----------------------------
# `reviews/<branch>.md` is this repo's convention. Running `gh` in the hook's own directory
# instead of the merge's meant a sibling repo's PR #N resolved to THIS repo's PR #N - a real
# merge was blocked citing an unrelated, long-merged branch that merely shared the number.
_other = tempfile.mkdtemp()
subprocess.run(["git", "init", "-q", _other], check=True)
check("a sibling repository is not this gate's business",
      run_in(M + " 1 --squash", _other)[0] == 0)
check("this repo's own merge is still gated",
      run_in(M + " 637 --merge", CWD)[0] != 0)
shutil.rmtree(_other, ignore_errors=True)

# --- end-to-end: still fails closed -------------------------------------------------
code, err = run("git status")
check("non-merge command is ignored", code == 0, err)

code, err = run(M + " 999999 --merge --auto")
check("unresolvable PR is blocked", code == 2, err)

code, err = run(M + " --merge --auto")
check("bare merge on an unreviewed checkout is blocked", code == 2, err)

sys.exit(1 if fails else 0)
