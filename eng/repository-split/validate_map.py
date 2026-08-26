"""Prove the extraction map claims every tracked path exactly once.

A path claimed by no target would be silently lost at the cut; a path claimed by
two targets would be duplicated into repositories that then drift. Both are
migration-blocking defects, so this runs as a gate rather than a report.

    python eng/repository-split/validate_map.py
"""

from __future__ import annotations

import subprocess
import sys
from collections import defaultdict
from pathlib import Path

import yaml

REPO_ROOT = Path(__file__).resolve().parents[2]
MAP = Path(__file__).resolve().parent / "map.yaml"


def tracked() -> list[str]:
    out = subprocess.run(
        ["git", "ls-files"], cwd=REPO_ROOT, capture_output=True, text=True, check=True
    ).stdout
    return out.splitlines()


def matches(path: str, prefix: str) -> bool:
    return path == prefix or path.startswith(prefix.rstrip("/") + "/")


def main() -> int:
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

    print(f"tracked paths        : {len(tracked())}")
    print(f"claimed by a target  : {len(claims)}")
    print(f"unclaimed            : {len(unclaimed)}")
    print(f"claimed by >1 target : {len(duplicated)}")

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

    return 1 if (unclaimed or duplicated) else 0


if __name__ == "__main__":
    raise SystemExit(main())
