"""Lock down the package publication rails that keep feed versions immutable and monotonic."""

import importlib.util
import sys
from pathlib import Path

import yaml


WORKFLOW = Path(__file__).resolve().parents[1] / "publish-packages.yml"
POLICY = Path(__file__).resolve().parents[2] / "scripts" / "package_publication_policy.py"

spec = importlib.util.spec_from_file_location("package_publication_policy", POLICY)
if spec is None or spec.loader is None:
    raise SystemExit("FAIL: could not load package publication policy")
policy = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = policy
spec.loader.exec_module(policy)


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

    publish = spec["jobs"]["publish"]
    require(
        publish["outputs"]["version"] == "${{ steps.version.outputs.version }}",
        "validated package version is exported to consumers",
    )
    version_step = next(step for step in publish["steps"] if step.get("id") == "version")
    version_script = version_step["run"]
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
    restore_step = next(
        step for step in verify["steps"] if step.get("name", "").startswith("Restore the full")
    )
    require('Version=\\"$VERSION\\"' in restore_step["run"], "fresh restore pins every package exactly")
    require("*-*" not in restore_step["run"], "floating restore cannot hide a version collision")


if __name__ == "__main__":
    main()
