"""Emit one target's git-filter-repo paths file from the extraction map.

`map.yaml` is the extraction contract, so the directives handed to filter-repo have to be
derived from it rather than retyped beside it. `include` becomes path filters and `rename`
becomes `==>` directives, because filter-repo does not treat a rename as a filter:

    python eng/repository-split/emit_paths.py platform-frontend -o platform-frontend.paths
    git filter-repo --paths-from-file platform-frontend.paths --force
"""

from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path

import yaml

MAP = Path(__file__).resolve().parent / "map.yaml"
REPO_ROOT = Path(__file__).resolve().parents[2]


def tracked() -> list[str]:
    out = subprocess.run(
        ["git", "ls-files", "-z"], cwd=REPO_ROOT, capture_output=True, text=True, check=True
    ).stdout
    return [path for path in out.split("\0") if path]


def unmatched_includes(includes: list[str]) -> list[str]:
    """Includes matching no tracked path: filter-repo would keep them silently and emptily."""
    paths = tracked()
    return [
        include
        for include in includes
        if not any(
            path == include or path.startswith(include.rstrip("/") + "/") for path in paths
        )
    ]


def directives(target: dict) -> list[str]:
    lines = [f"literal:{path}" for path in target.get("include") or []]
    lines += [
        f"{source}==>{destination}" for source, destination in (target.get("rename") or {}).items()
    ]
    return lines


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("target", help="a target name declared in map.yaml")
    parser.add_argument("-o", "--output", type=Path, help="write here instead of stdout")
    args = parser.parse_args()

    spec = yaml.safe_load(MAP.read_text(encoding="utf-8"))
    targets = spec["targets"]
    if args.target not in targets:
        print(f"unknown target: {args.target}; declared: {', '.join(sorted(targets))}", file=sys.stderr)
        return 1

    target = targets[args.target]

    # `replicated` is not an extraction: the map says each repository needs its own copy, not the
    # monorepo's. Report it rather than emit nothing silently, because a caller otherwise cannot tell
    # an unrepresentable section from an overlooked one.
    replicated = spec.get("replicated") or []
    if replicated:
        print(
            f"note: {len(replicated)} `replicated` path(s) are not emitted "
            f"({', '.join(replicated)}); each target authors its own",
            file=sys.stderr,
        )

    unmatched = unmatched_includes(target.get("include") or [])
    if unmatched:
        joined = "\n  ".join(unmatched)
        print(
            f"{args.target} includes path(s) matching nothing tracked, which would carve empty:"
            f"\n  {joined}",
            file=sys.stderr,
        )
        return 1

    if target.get("exclude"):
        print(
            f"{args.target} declares `exclude`, which a paths file cannot express; "
            "pass those to filter-repo as --invert-paths in a second pass",
            file=sys.stderr,
        )
        return 1

    body = "\n".join(
        [
            f"# Generated from {MAP.name} for target {args.target} ({target['repo']}).",
            "# Do not edit: regenerate with eng/repository-split/emit_paths.py.",
            *directives(target),
            "",
        ]
    )

    if args.output:
        args.output.write_text(body, encoding="utf-8")
    else:
        sys.stdout.write(body)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
