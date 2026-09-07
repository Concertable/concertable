"""Prove service scoping and queue E2E dependencies only ever narrow CI safely.

The classifier decides which services a diff can affect, so an unrelated service's
suites and standalone carve never gate a PR. Getting it wrong is silent: a suite
that should have run simply does not. The classifier block and dependency graph
are read from test.yml itself rather than restated here, so the test cannot drift
from the gate.

    python .github/workflows/tests/test_service_scope.py
"""

from __future__ import annotations

import os
import re
import subprocess
import sys
import tempfile
from pathlib import Path

import yaml

WORKFLOW = Path(__file__).resolve().parents[1] / "test.yml"

ALL = "ALL"
MATRIX_GUARDS = {
    "unit-tests": "unit_projects",
    "architecture-tests": "architecture_projects",
    "integration-tests": "integration_projects",
}
QUEUE_E2E_JOBS = ("e2e-api-tests", "e2e-ui-tests")

CASES: list[tuple[str, list[str], str]] = [
    ("payment only", ["api/Concertable.Payment/src/X/Y.cs"], "Payment"),
    ("b2b only", ["api/Concertable.B2B/src/Modules/Deal/D.cs"], "B2B"),
    ("auth only", ["api/Concertable.Auth/src/A.cs"], "Auth"),
    ("auth contracts sits outside the Auth folder but is Auth's", ["api/Concertable.Auth.Contracts/E.cs"], "Auth"),
    ("two services", ["api/Concertable.B2B/a.cs", "api/Concertable.Customer/b.cs"], "B2B Customer"),
    # Anything shared must widen to ALL: every service compiles against it.
    ("shared kernel", ["api/Concertable.Shared/src/Concertable.Kernel/K.cs"], ALL),
    ("messaging", ["api/Concertable.Messaging/M.cs"], ALL),
    ("api root build config", ["api/Directory.Build.targets"], ALL),
    ("umbrella apphost", ["api/Concertable.AppHost/P.cs"], ALL),
    ("service file plus a shared file", ["api/Concertable.Payment/a.cs", "api/Concertable.Shared/b.cs"], ALL),
    # The gate re-validating itself must never run a narrowed matrix.
    ("the workflow itself", [".github/workflows/test.yml"], ALL),
    ("workflow change alongside one service", [".github/workflows/test.yml", "api/Concertable.Payment/a.cs"], ALL),
    # No backend service is affected; the backend matrices are legitimately empty.
    ("frontend only", ["app/web/customer/src/App.tsx"], ""),
    ("scripts only", ["scripts/e2e.ps1"], ""),
]


def extract_block() -> str:
    spec = yaml.safe_load(WORKFLOW.read_text(encoding="utf-8"))
    steps = spec["jobs"]["changes"]["steps"]
    detect = next(s for s in steps if s.get("id") == "detect")
    run = detect["run"]
    start = run.index("# SERVICE SCOPE:")
    end = run.index('echo "services=$services"')
    block = run[start:end]
    if "SERVICE_DIRS" not in block:
        raise SystemExit("FAIL: could not extract the service-scope block from test.yml")
    return block


def bash() -> str:
    # On Windows a bare `bash` can resolve to a WSL stub that cannot exec; prefer Git Bash,
    # which is the shell this repository's other hooks already assume.
    for candidate in (r"C:\Program Files\Git\usr\bin\bash.exe", r"C:\Program Files\Git\bin\bash.exe"):
        if Path(candidate).exists():
            return candidate
    return "bash"


def run_case(block: str, files: list[str]) -> str:
    # The block narrates to stdout, so tag the value and read the tagged line.
    script = (
        f"set -eu\nfiles={sh_quote(chr(10).join(files))}\n{block}\n"
        'printf "SCOPE:%s\\n" "$services"\n'
    )
    with tempfile.NamedTemporaryFile("w", suffix=".sh", delete=False, newline="\n") as fh:
        fh.write(script)
        path = fh.name
    try:
        shell = bash()
        env = os.environ.copy()
        shell_path = Path(shell)
        if shell_path.is_absolute():
            env["PATH"] = os.pathsep.join(
                (str(shell_path.parent), env.get("PATH", ""))
            )
        proc = subprocess.run(
            [shell, path], capture_output=True, text=True, env=env
        )
        if proc.returncode != 0:
            raise SystemExit(f"FAIL: classifier errored: {proc.stderr.strip()}")
        line = next(
            (l for l in proc.stdout.splitlines() if l.startswith("SCOPE:")), None
        )
        if line is None:
            raise SystemExit(f"FAIL: classifier emitted no scope: {proc.stdout!r}")
        return line[len("SCOPE:") :].strip()
    finally:
        Path(path).unlink(missing_ok=True)


def sh_quote(s: str) -> str:
    return "'" + s.replace("'", "'\\''") + "'"


def matrix_guard_cases() -> list[tuple[str, bool, str]]:
    spec = yaml.safe_load(WORKFLOW.read_text(encoding="utf-8"))
    cases = []
    for job, output in MATRIX_GUARDS.items():
        condition = spec["jobs"][job].get("if", "")
        expected = f"needs.changes.outputs.{output} != '[]'"
        cases.append((job, expected in condition, condition))
    return cases


def dependency_closure(spec: dict, job: str) -> set[str]:
    closure: set[str] = set()
    pending = [job]
    while pending:
        current = pending.pop()
        needs = spec["jobs"][current].get("needs", [])
        if isinstance(needs, str):
            needs = [needs]
        for dependency in needs:
            if dependency not in closure:
                closure.add(dependency)
                pending.append(dependency)
    return closure


def queue_e2e_dependency_cases() -> list[tuple[str, bool, set[str]]]:
    """Queue E2E must survive the empty matrices produced by frontend-only diffs."""
    spec = yaml.safe_load(WORKFLOW.read_text(encoding="utf-8"))
    cases = []
    for job in QUEUE_E2E_JOBS:
        blocked_by = dependency_closure(spec, job).intersection(MATRIX_GUARDS)
        cases.append((job, not blocked_by, blocked_by))
    return cases


def main() -> int:
    block = extract_block()
    failures = 0
    for name, files, expected in CASES:
        actual = run_case(block, files)
        ok = actual == expected.strip()
        if not ok:
            failures += 1
        status = "ok  " if ok else "FAIL"
        print(f"{status} {name}: expected {expected!r}, got {actual!r}")
    for name, ok, condition in matrix_guard_cases():
        if not ok:
            failures += 1
        status = "ok  " if ok else "FAIL"
        print(f"{status} {name} empty-matrix guard: {condition!r}")
    for name, ok, blocked_by in queue_e2e_dependency_cases():
        if not ok:
            failures += 1
        status = "ok  " if ok else "FAIL"
        print(f"{status} {name} independent of empty matrices: {sorted(blocked_by)!r}")
    total = len(CASES) + len(MATRIX_GUARDS) + len(QUEUE_E2E_JOBS)
    print(f"\n{total - failures}/{total} passed")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
