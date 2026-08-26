"""Prove the CI service-scope classifier only ever narrows CI safely.

The classifier decides which services a diff can affect, so an unrelated service's
suites and standalone carve never gate a PR. Getting it wrong is silent: a suite
that should have run simply does not. The block under test is extracted from
test.yml itself rather than restated here, so the test cannot drift from the gate.

    python .github/workflows/tests/test_service_scope.py
"""

from __future__ import annotations

import re
import subprocess
import sys
import tempfile
from pathlib import Path

import yaml

WORKFLOW = Path(__file__).resolve().parents[1] / "test.yml"

ALL = "ALL"

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
        proc = subprocess.run([bash(), path], capture_output=True, text=True)
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
    print(f"\n{len(CASES) - failures}/{len(CASES)} passed")
    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
