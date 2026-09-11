"""Lock down the package publication rails that keep feed versions immutable and monotonic."""

import importlib.util
import json
import re
import sys
import tempfile
import zipfile
from pathlib import Path

import yaml


WORKFLOW = Path(__file__).resolve().parents[1] / "publish-packages.yml"
POLICY = Path(__file__).resolve().parents[2] / "scripts" / "package_publication_policy.py"
OWNERSHIP = Path(__file__).resolve().parents[2] / "scripts" / "package_ownership.py"
REPO_ROOT = Path(__file__).resolve().parents[3]
PLATFORM_PIN = REPO_ROOT / "api" / "Concertable.Shared" / "Directory.Packages.props"
RENOVATE = REPO_ROOT / "renovate.json"
RETIRED_PLATFORM_SYNC_PATHS = (
    ".github/scripts/bump-platform-version.sh",
    ".github/scripts/bump-platform-version.test.mjs",
    ".github/scripts/platform-sync-pr-action.mjs",
    ".github/scripts/platform-sync-pr-action.test.mjs",
    ".github/workflows/platform-sync-alert.yml",
    ".github/workflows/platform-sync.yml",
)

spec = importlib.util.spec_from_file_location("package_publication_policy", POLICY)
if spec is None or spec.loader is None:
    raise SystemExit("FAIL: could not load package publication policy")
policy = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = policy
spec.loader.exec_module(policy)

ownership_spec = importlib.util.spec_from_file_location("package_ownership", OWNERSHIP)
if ownership_spec is None or ownership_spec.loader is None:
    raise SystemExit("FAIL: could not load package ownership policy")
ownership = importlib.util.module_from_spec(ownership_spec)
ownership_spec.loader.exec_module(ownership)


def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(f"FAIL: {message}")
    print(f"ok  {message}")


def main() -> None:
    spec = yaml.safe_load(WORKFLOW.read_text(encoding="utf-8"))
    triggers = spec.get("on", spec.get(True))
    require(set(triggers) == {"push"}, "packages publish only from a main push")
    require(triggers["push"]["branches"] == ["main"], "push trigger is restricted to main")
    require(
        ".github/workflows/publish-packages.yml" in triggers["push"]["paths"],
        "publication-policy repairs trigger their own acceptance publish",
    )
    require(
        ".github/scripts/package_publication_policy.py" in triggers["push"]["paths"],
        "publication-policy implementation changes trigger acceptance publishing",
    )
    require(
        ".github/scripts/package_ownership.py" in triggers["push"]["paths"],
        "package-ownership changes trigger acceptance publishing",
    )
    require(
        "eng/repository-split/inventory.py" in triggers["push"]["paths"],
        "inventory generator changes trigger acceptance publishing",
    )
    require(
        "eng/repository-split/inventory.json" in triggers["push"]["paths"],
        "inventory changes trigger acceptance publishing",
    )
    require(
        "eng/repository-split/map.yaml" in triggers["push"]["paths"],
        "ownership-map changes trigger acceptance publishing",
    )
    require(
        all(not (REPO_ROOT / path).exists() for path in RETIRED_PLATFORM_SYNC_PATHS),
        "the bespoke platform-sync implementation is fully retired",
    )

    renovate = json.loads(RENOVATE.read_text(encoding="utf-8"))
    require(
        "github>Concertable/.github:renovate-config.json" in renovate["extends"],
        "Renovate extends the organization dependency policy",
    )
    platform_manager = next(
        manager
        for manager in renovate["customManagers"]
        if manager.get("description") == "Shared .NET platform train"
    )
    require(
        platform_manager["depNameTemplate"] == "Concertable.Build"
        and platform_manager["datasourceTemplate"] == "nuget"
        and platform_manager["versioningTemplate"] == "nuget",
        "Renovate follows the canonical platform bellwether",
    )
    platform_rule = next(
        rule
        for rule in renovate["packageRules"]
        if rule.get("matchDepNames") == ["Concertable.Build"]
    )
    require(
        platform_rule["groupName"] == "shared .NET platform train"
        and platform_rule["schedule"] == ["* 0-4 * * 1"]
        and platform_rule["automerge"] is False,
        "Renovate opens one non-blocking weekly platform refresh",
    )
    pin_pattern = re.compile(
        platform_manager["matchStrings"][0].replace("(?<currentValue>", "(?P<currentValue>")
    )
    pin_files = sorted(REPO_ROOT.glob("api/**/Directory.Packages.props"))
    managed_pins = [
        path for path in pin_files if pin_pattern.search(path.read_text(encoding="utf-8"))
    ]
    require(len(managed_pins) == 8, "Renovate manages every shared platform pin")
    require(
        len({pin_pattern.search(path.read_text(encoding="utf-8")).group("currentValue") for path in managed_pins})
        == 1,
        "shared platform pins remain lockstep",
    )

    publish = spec["jobs"]["publish"]
    require(
        publish["outputs"]["version"] == "${{ steps.version.outputs.version }}",
        "validated package version is exported to consumers",
    )
    pack_step = next(step for step in publish["steps"] if step.get("name") == "Pack publishable projects")
    require(
        "UseLocalPlatformPackages=true" in pack_step["run"]
        and "UseLocalPlatformSources" not in pack_step["run"],
        "service package metadata targets the published platform train",
    )
    version_step = next(step for step in publish["steps"] if step.get("id") == "version")
    version_script = version_step["run"]
    partition_step = next(step for step in publish["steps"] if step.get("name") == "Keep service-owned packages")
    require(
        publish["steps"].index(partition_step) < publish["steps"].index(version_step),
        "foreign packages are removed before version validation",
    )
    require(
        "package_ownership.py" in partition_step["run"]
        and "eng/repository-split/inventory.json" in partition_step["run"],
        "package ownership comes from the generated split inventory",
    )
    require("package_publication_policy.py" in version_script, "all packed artifacts use the tested policy")
    push_step = next(step for step in publish["steps"] if step.get("name") == "Push to GitHub Packages")
    require("--skip-duplicate" not in push_step["run"], "immutable package collisions cannot be hidden")

    compare_cases = [
        ("0.1.0-alpha.0.1329", "0.1.0-alpha.0.1330", True),
        ("0.1.0-alpha.0.1330", "0.1.0-alpha.0.1329", False),
        ("1.0.0-rc.2", "1.0.0", True),
        ("1.0.0", "1.0.0-rc.2", False),
        ("1.0.0-alpha.2", "1.0.0-alpha.10", True),
    ]
    for left, right, expected in compare_cases:
        require(
            (policy.parse_semver(left) < policy.parse_semver(right)) is expected,
            f"NuGet SemVer precedence: {left} < {right} is {expected}",
        )
    require(
        policy.parse_semver("0.1.0-alpha.0.1330") == policy.parse_semver("0.1.0-alpha.0.1330"),
        "equal NuGet versions compare equal",
    )

    packages = [
        policy.PackedPackage("Concertable.AppHost.Shared", "0.1.0-alpha.0.1330"),
        policy.PackedPackage("Concertable.Frontend.Hosting", "0.1.0-alpha.0.1330"),
    ]
    feed = {
        "concertable.apphost.shared": ["0.1.0-alpha.0.1329"],
        "concertable.frontend.hosting": ["0.1.0-alpha.0.1329"],
    }
    require(policy.validate_batch(packages, feed) == "0.1.0-alpha.0.1330", "fresh batch advances")
    feed["concertable.frontend.hosting"].append("0.1.0-alpha.0.1330")
    try:
        policy.validate_batch(packages, feed)
    except ValueError:
        print("ok  a non-bellwether package collision fails the whole batch")
    else:
        raise SystemExit("FAIL: a non-bellwether package collision was accepted")
    try:
        policy.validate_batch(
            packages,
            {
                "concertable.apphost.shared": ["0.1.0"],
                "concertable.frontend.hosting": ["0.1.0-alpha.0.1329"],
            },
        )
    except ValueError:
        print("ok  a prerelease cannot supersede a stable feed version")
    else:
        raise SystemExit("FAIL: SemVer downgrade below a stable package was accepted")
    try:
        policy.validate_batch(
            [
                packages[0],
                policy.PackedPackage("Concertable.Frontend.Hosting", "0.1.0-alpha.0.1331"),
            ],
            feed,
        )
    except ValueError:
        print("ok  mixed packed versions fail the lockstep batch")
    else:
        raise SystemExit("FAIL: mixed packed versions were accepted")

    verify = spec["jobs"]["verify-restore"]
    require(
        verify["env"]["VERSION"] == "${{ needs.publish.outputs.version }}",
        "restore consumes the published job's exact version",
    )
    pin_match = re.search(
        r"<ConcertableDotNetPlatformVersion>([^<]+)</ConcertableDotNetPlatformVersion>",
        PLATFORM_PIN.read_text(encoding="utf-8"),
    )
    require(pin_match is not None, "shared platform pin exists")
    restore_step = next(
        step for step in verify["steps"] if step.get("name", "").startswith("Restore the published")
    )
    require('Version=\\"$VERSION\\"' in restore_step["run"], "fresh restore pins every package exactly")
    require(
        'Version=\\"$platform_version\\"' in restore_step["run"]
        and "api/Concertable.Shared/Directory.Packages.props" in restore_step["run"]
        and pin_match.group(1) not in restore_step["run"],
        "fresh restore reads and pins the repository's platform version",
    )
    require(
        "list retained" in restore_step["run"] and "list platform-dotnet" in restore_step["run"],
        "restore partitions service and platform packages from inventory",
    )
    require("*-*" not in restore_step["run"], "floating restore cannot hide a version collision")

    with tempfile.TemporaryDirectory() as directory:
        root = Path(directory)
        inventory_path = root / "inventory.json"
        inventory_path.write_text(
            json.dumps(
                {
                    "dotnet": {
                        "projects": {
                            "platform": {
                                "name": "Concertable.DataAccess.Infrastructure",
                                "target": "platform-dotnet",
                                "packable": True,
                            },
                            "service": {
                                "name": "Concertable.B2B.Contracts",
                                "target": "b2b",
                                "packable": True,
                            },
                            "system": {
                                "name": "Concertable.Testing.E2E",
                                "target": "system",
                                "packable": True,
                            },
                        }
                    }
                }
            ),
            encoding="utf-8",
        )
        package_dir = root / "packages"
        package_dir.mkdir()
        for package_id in (
            "Concertable.DataAccess.Infrastructure",
            "Concertable.B2B.Contracts",
            "Concertable.Testing.E2E",
        ):
            with zipfile.ZipFile(package_dir / f"{package_id}.1.0.0.nupkg", "w") as archive:
                archive.writestr(
                    f"{package_id}.nuspec",
                    f"<package><metadata><id>{package_id}</id></metadata></package>",
                )
        package_targets = ownership.load_ownership(inventory_path)
        retained, removed = ownership.filter_batch(package_dir, package_targets)
        require(retained == ["Concertable.B2B.Contracts"], "service package remains in the publish batch")
        require(
            removed == ["Concertable.DataAccess.Infrastructure", "Concertable.Testing.E2E"],
            "platform and future-system packages leave the publish batch",
        )
        require(
            ownership.packages_for(package_targets, "platform-dotnet")
            == ["Concertable.DataAccess.Infrastructure"],
            "platform restore list excludes the future-system package",
        )


if __name__ == "__main__":
    main()
