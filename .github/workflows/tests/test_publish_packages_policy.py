"""Lock down the package publication rails that keep feed versions immutable and monotonic."""

from pathlib import Path

import yaml


WORKFLOW = Path(__file__).resolve().parents[1] / "publish-packages.yml"


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

    publish = spec["jobs"]["publish"]
    require(
        publish["outputs"]["version"] == "${{ steps.version.outputs.version }}",
        "validated package version is exported to consumers",
    )
    version_step = next(step for step in publish["steps"] if step.get("id") == "version")
    version_script = version_step["run"]
    require("versions[@]" in version_script, "all packed artifacts must share one version")
    require("$FEED_DOWNLOAD/$BELLWETHER/index.json" in version_script, "feed latest is authoritative")
    require("is not newer than feed" in version_script, "non-advancing versions fail before push")

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
