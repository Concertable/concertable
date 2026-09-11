from pathlib import Path

import yaml


WORKFLOW = Path(__file__).resolve().parents[1] / "publish-images.yml"


def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(f"FAIL: {message}")
    print(f"ok  {message}")


def main() -> None:
    spec = yaml.safe_load(WORKFLOW.read_text(encoding="utf-8"))
    triggers = spec.get("on", spec.get(True))
    paths = triggers["push"]["paths"]
    require(
        paths.index("api/**") < paths.index("!api/**/*.md"),
        "API Markdown is excluded after the API source inclusion",
    )
    require(
        ".github/workflows/publish-images.yml" in paths,
        "image-publication repairs trigger their own acceptance publish",
    )
    publish = spec["jobs"]["publish"]
    steps = publish["steps"]
    prepare = next(step for step in steps if step.get("name") == "Prepare local platform packages")
    image = next(step for step in steps if step.get("name") == "Build and push the image")
    require(
        steps.index(prepare) < steps.index(image),
        "local platform packages are prepared before image publication",
    )
    require(
        prepare["run"] == "pwsh ./scripts/local-platform.ps1 prepare",
        "image publication uses the canonical local platform preparation entry point",
    )
    require(
        'pwsh ./scripts/local-platform.ps1 publish "${{ matrix.project }}"' in image["run"],
        "each image publishes through the canonical local platform entry point",
    )
    require("dotnet publish" not in image["run"], "image publication cannot restore the stale feed pin")
    require("/t:PublishContainer" in image["run"], "SDK container publication remains enabled")
    require(
        "-p:ContainerImageTag=${{ github.sha }}" in image["run"],
        "published images retain the immutable commit tag",
    )


if __name__ == "__main__":
    main()
