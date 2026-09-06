"""Prove the extraction map claims every tracked path exactly once.

A path claimed by no target would be silently lost at the cut; a path claimed by
two targets would be duplicated into repositories that then drift. Both are
migration-blocking defects, so this runs as a gate rather than a report.

    python eng/repository-split/validate_map.py
"""

from __future__ import annotations

import argparse
import subprocess
import sys
from collections import defaultdict
from pathlib import Path

import yaml

REPO_ROOT = Path(__file__).resolve().parents[2]
MAP = Path(__file__).resolve().parent / "map.yaml"

MONOREPO_ONLY_OWNER_SCRIPTS = {
    "scripts/setup-local-dev.ps1",
    "scripts/sync-owner-tooling.ps1",
    "scripts/test-owner-operations.ps1",
}
SYSTEM_OWNER_SCRIPTS = {
    "scripts/system/OwnerOperations.psm1",
    "scripts/system/setup-local-dev.ps1",
    "scripts/test-system-bootstrap.ps1",
}
PLATFORM_OWNER_MODULE = "api/Concertable.Shared/tools/OwnerOperations.psm1"
PLATFORM_OWNER_MODULE_DESTINATION = "tools/OwnerOperations.psm1"


def tracked() -> list[str]:
    out = subprocess.run(
        ["git", "ls-files"], cwd=REPO_ROOT, capture_output=True, text=True, check=True
    ).stdout
    return out.splitlines()


def matches(path: str, prefix: str) -> bool:
    return path == prefix or path.startswith(prefix.rstrip("/") + "/")


def renamed_path(path: str, renames: dict[str, str]) -> str:
    result = path
    for source, destination in renames.items():
        if not matches(result, source):
            continue
        suffix = result[len(source.rstrip("/")) :].lstrip("/")
        result = "/".join(part for part in (destination.rstrip("/"), suffix) if part)
    return result


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--owner-operations-only",
        action="store_true",
        help="check only the M2 owner-operation extraction contract",
    )
    args = parser.parse_args()
    spec = yaml.safe_load(MAP.read_text(encoding="utf-8"))
    targets = spec["targets"]
    dissolves = spec.get("dissolves") or []
    archive_only = spec.get("archiveOnly") or []
    replicated = spec.get("replicated") or []

    claims: dict[str, list[str]] = defaultdict(list)
    unclaimed: list[str] = []

    for path in tracked():
        for name, t in targets.items():
            includes = t.get("include") or []
            excludes = t.get("exclude") or []
            if any(matches(path, i) for i in includes) and not any(
                matches(path, e) for e in excludes
            ):
                claims[path].append(name)

        if claims[path]:
            continue
        if any(matches(path, p) for p in dissolves + archive_only + replicated):
            continue
        unclaimed.append(path)

    duplicated = {p: t for p, t in claims.items() if len(t) > 1}
    semantic_errors: list[str] = []
    for path in sorted(MONOREPO_ONLY_OWNER_SCRIPTS):
        targets_for_path = claims.get(path, [])
        if targets_for_path:
            semantic_errors.append(f"monorepo-only owner script is extracted: {path} -> {targets_for_path}")
        if not any(matches(path, item) for item in dissolves):
            semantic_errors.append(f"monorepo-only owner script has no dissolve disposition: {path}")
    for path in sorted(SYSTEM_OWNER_SCRIPTS):
        targets_for_path = claims.get(path, [])
        if targets_for_path != ["system"]:
            semantic_errors.append(f"System owner script must be claimed only by system: {path} -> {targets_for_path}")
    platform_destination = renamed_path(
        PLATFORM_OWNER_MODULE,
        targets["platform-dotnet"].get("rename") or {},
    )
    if platform_destination != PLATFORM_OWNER_MODULE_DESTINATION:
        semantic_errors.append(
            "platform owner module has the wrong extraction destination: "
            f"{PLATFORM_OWNER_MODULE} -> {platform_destination}"
        )

    if args.owner_operations_only:
        print(f"owner-operation map errors: {len(semantic_errors)}")
        for error in semantic_errors:
            print(f"  {error}")
        return 1 if semantic_errors else 0

    print(f"tracked paths        : {len(tracked())}")
    print(f"claimed by a target  : {len(claims)}")
    print(f"unclaimed            : {len(unclaimed)}")
    print(f"claimed by >1 target : {len(duplicated)}")
    print(f"semantic errors      : {len(semantic_errors)}")

    if duplicated:
        print("\nDUPLICATE CLAIMS (a path would land in two repositories):")
        for p, t in sorted(duplicated.items())[:40]:
            print(f"  {p}  ->  {', '.join(t)}")

    if unclaimed:
        print("\nUNCLAIMED (would be lost at the cut) — top-level grouping:")
        groups: dict[str, int] = defaultdict(int)
        for p in unclaimed:
            parts = p.split("/")
            groups["/".join(parts[:2]) if len(parts) > 1 else parts[0]] += 1
        for g, n in sorted(groups.items(), key=lambda kv: -kv[1]):
            print(f"  {n:5}  {g}")

    if semantic_errors:
        print("\nSEMANTIC MAP ERRORS:")
        for error in semantic_errors:
            print(f"  {error}")

    return 1 if (unclaimed or duplicated or semantic_errors) else 0


if __name__ == "__main__":
    raise SystemExit(main())
