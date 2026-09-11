from __future__ import annotations

import argparse
import json
import zipfile
from pathlib import Path
from xml.etree import ElementTree


RETAINED_TARGETS = frozenset({"auth", "b2b", "customer", "payment", "search"})
KNOWN_TARGETS = RETAINED_TARGETS | {"platform-dotnet", "system"}


def load_ownership(inventory_path: Path) -> dict[str, str]:
    inventory = json.loads(inventory_path.read_text(encoding="utf-8"))
    ownership: dict[str, str] = {}
    for project in inventory["dotnet"]["projects"].values():
        if not project["packable"]:
            continue
        package_id = project["name"]
        target = project["target"]
        if target not in KNOWN_TARGETS:
            raise ValueError(f"Packable package '{package_id}' has unsupported target '{target}'")
        if package_id in ownership:
            raise ValueError(f"Packable package '{package_id}' appears more than once")
        ownership[package_id] = target
    if not ownership:
        raise ValueError("Inventory contains no packable packages")
    return ownership


def package_id(package_path: Path) -> str:
    with zipfile.ZipFile(package_path) as archive:
        nuspecs = [name for name in archive.namelist() if name.lower().endswith(".nuspec")]
        if len(nuspecs) != 1:
            raise ValueError(f"{package_path.name} contains {len(nuspecs)} nuspec files")
        root = ElementTree.fromstring(archive.read(nuspecs[0]))
    ids = [
        (element.text or "").strip()
        for element in root.iter()
        if element.tag.rsplit("}", 1)[-1] == "id"
    ]
    if len(ids) != 1 or not ids[0]:
        raise ValueError(f"{package_path.name} has no unique package id")
    return ids[0]


def packages_for(ownership: dict[str, str], group: str) -> list[str]:
    if group == "retained":
        return sorted(package_id for package_id, target in ownership.items() if target in RETAINED_TARGETS)
    return sorted(package_id for package_id, target in ownership.items() if target == group)


def filter_batch(package_dir: Path, ownership: dict[str, str]) -> tuple[list[str], list[str]]:
    artifacts = sorted(package_dir.glob("*.nupkg"))
    discovered: dict[str, Path] = {}
    for artifact in artifacts:
        package_id_value = package_id(artifact)
        if package_id_value in discovered:
            raise ValueError(f"Package '{package_id_value}' was packed more than once")
        discovered[package_id_value] = artifact
    unknown = sorted(set(discovered) - set(ownership))
    missing = sorted(set(ownership) - set(discovered))
    if unknown or missing:
        raise ValueError(f"Package batch does not match inventory; unknown={unknown}, missing={missing}")
    retained = packages_for(ownership, "retained")
    removed = sorted(set(discovered) - set(retained))
    for package_id_value in removed:
        discovered[package_id_value].unlink()
    return retained, removed


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("inventory", type=Path)
    subparsers = parser.add_subparsers(dest="command", required=True)
    filter_parser = subparsers.add_parser("filter")
    filter_parser.add_argument("package_dir", type=Path)
    list_parser = subparsers.add_parser("list")
    list_parser.add_argument("group", choices=["retained", "platform-dotnet", "system"])
    args = parser.parse_args()

    ownership = load_ownership(args.inventory)
    if args.command == "filter":
        retained, removed = filter_batch(args.package_dir, ownership)
        print(f"Retained {len(retained)} service packages; removed {len(removed)} foreign packages.")
    else:
        print("\n".join(packages_for(ownership, args.group)))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
