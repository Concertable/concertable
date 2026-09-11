from __future__ import annotations

import argparse
import json
import zipfile
from pathlib import Path
from xml.etree import ElementTree


# A service leaves RETAINED_TARGETS when it starts publishing its own packages from its own
# repository, at its checkpoint 10C. The monorepo still builds and packs its projects — removing it
# here only stops the push, so versions already on the feed keep resolving for pinned consumers and
# nothing is withdrawn. It stays in KNOWN_TARGETS because the inventory still carries its projects
# until 10F removes the source; dropping it from there instead makes load_ownership reject them.
#
# Two publishers of one package id is what this prevents: the second one to push a version that does
# not advance the id's feed history is rejected outright, which took the monorepo's publisher down for
# a day on 2026-09-10.
PROMOTED_TARGETS = frozenset({"auth"})
RETAINED_TARGETS = frozenset({"b2b", "customer", "payment", "search"})
KNOWN_TARGETS = RETAINED_TARGETS | PROMOTED_TARGETS | {"platform-dotnet", "system"}


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


def package_dependencies(package_path: Path) -> list[tuple[str, str]]:
    with zipfile.ZipFile(package_path) as archive:
        nuspecs = [name for name in archive.namelist() if name.lower().endswith(".nuspec")]
        if len(nuspecs) != 1:
            raise ValueError(f"{package_path.name} contains {len(nuspecs)} nuspec files")
        root = ElementTree.fromstring(archive.read(nuspecs[0]))
    return [
        (element.attrib.get("id", "").strip(), element.attrib.get("version", "").strip())
        for element in root.iter()
        if element.tag.rsplit("}", 1)[-1] == "dependency"
    ]


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


def validate_platform_dependencies(
    package_dir: Path, ownership: dict[str, str], expected_version: str
) -> int:
    platform_packages = {
        package_id_value.casefold() for package_id_value in packages_for(ownership, "platform-dotnet")
    }
    checked = 0
    failures: list[str] = []
    for artifact in sorted(package_dir.glob("*.nupkg")):
        artifact_id = package_id(artifact)
        if ownership.get(artifact_id) not in RETAINED_TARGETS:
            raise ValueError(f"Package '{artifact_id}' is not service-owned")
        for dependency_id, dependency_version in package_dependencies(artifact):
            if dependency_id.casefold() not in platform_packages:
                continue
            checked += 1
            if dependency_version != expected_version:
                failures.append(
                    f"{artifact_id} -> {dependency_id} ({dependency_version or '<missing>'})"
                )
    if failures:
        raise ValueError(
            f"Service packages do not target platform {expected_version}: {', '.join(failures)}"
        )
    if checked == 0:
        raise ValueError("Service packages declare no platform dependencies")
    return checked


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("inventory", type=Path)
    subparsers = parser.add_subparsers(dest="command", required=True)
    filter_parser = subparsers.add_parser("filter")
    filter_parser.add_argument("package_dir", type=Path)
    list_parser = subparsers.add_parser("list")
    list_parser.add_argument("group", choices=["retained", "platform-dotnet", "system"])
    validate_parser = subparsers.add_parser("validate-platform")
    validate_parser.add_argument("package_dir", type=Path)
    validate_parser.add_argument("expected_version")
    args = parser.parse_args()

    ownership = load_ownership(args.inventory)
    if args.command == "filter":
        retained, removed = filter_batch(args.package_dir, ownership)
        print(f"Retained {len(retained)} service packages; removed {len(removed)} foreign packages.")
    elif args.command == "list":
        print("\n".join(packages_for(ownership, args.group)))
    else:
        checked = validate_platform_dependencies(args.package_dir, ownership, args.expected_version)
        print(f"Validated {checked} service-to-platform package dependencies.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
