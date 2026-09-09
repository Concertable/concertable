"""Emit one target's git-filter-repo paths file from the extraction map.

`map.yaml` is the extraction contract, so the directives handed to filter-repo have to be
derived from it rather than retyped beside it. `include`/`exclude` become path filters and
`rename` becomes `==>` directives, because filter-repo does not treat a rename as a filter:

    python eng/repository-split/emit_paths.py platform-frontend -o platform-frontend.paths
    git filter-repo --paths-from-file platform-frontend.paths --force
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

import yaml

MAP = Path(__file__).resolve().parent / "map.yaml"


def directives(target: dict) -> list[str]:
    lines: list[str] = []

    for path in target.get("include") or []:
        lines.append(f"literal:{path}")

    for source, destination in (target.get("rename") or {}).items():
        lines.append(f"{source}==>{destination}")

    return lines


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("target", help="a target name declared in map.yaml")
    parser.add_argument("-o", "--output", type=Path, help="write here instead of stdout")
    args = parser.parse_args()

    targets = yaml.safe_load(MAP.read_text(encoding="utf-8"))["targets"]
    if args.target not in targets:
        print(f"unknown target: {args.target}; declared: {', '.join(sorted(targets))}", file=sys.stderr)
        return 1

    target = targets[args.target]
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
