"""Repository-split inventory generator.

Emits a deterministic machine-readable graph of the monorepo's build shape so the
repository-per-microservice migration can be planned and drift-checked against a
committed baseline.

    python eng/repository-split/inventory.py            # regenerate inventory.json
    python eng/repository-split/inventory.py --check    # fail if the tree has drifted

The generator reads only tracked source; it runs no build and contacts no network.
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
OUTPUT = Path(__file__).resolve().parent / "inventory.json"

# Target repository ownership, per REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md.
# An area absent from this map is reported as unassigned rather than guessed.
AREA_TARGETS = {
    "Concertable.B2B": "b2b",
    "Concertable.Customer": "customer",
    "Concertable.Payment": "payment",
    "Concertable.Search": "search",
    "Concertable.Auth": "auth",
    "Concertable.Auth.Contracts": "auth",
    "Concertable.Shared": "platform-dotnet",
    "Concertable.Messaging": "platform-dotnet",
    "Concertable.DataAccess": "platform-dotnet",
    "Concertable.ServiceDefaults": "platform-dotnet",
    "Concertable.AppHost.Shared": "platform-dotnet",
    "Concertable.Frontend.Hosting": "platform-dotnet",
    "Concertable.AppHost": "system",
    "tests": "system",
}

FRONTEND_TARGETS = {
    "app/shared": "platform-web",
    "app/web/shared": "platform-web",
    "app/mobile/shared": "platform-web",
    "app/web/b2b": "b2b",
    "app/b2b/shared": "b2b",
    "app/mobile/b2b": "b2b",
    "app/web/admin": "b2b",
    "app/web/customer": "customer",
    "app/customer/shared": "customer",
    "app/mobile/customer": "customer",
}

# The npm workspace root exists only to bind the monorepo's workspaces together and
# has no successor: each target repo declares its own root manifest at the cut.
DISSOLVES_AT_CUT = {"app"}


def tracked_files() -> list[str]:
    out = subprocess.run(
        ["git", "ls-files"],
        cwd=REPO_ROOT,
        capture_output=True,
        text=True,
        check=True,
    ).stdout
    return out.splitlines()


def area_of(rel_path: str) -> str | None:
    parts = rel_path.split("/")
    if len(parts) < 2 or parts[0] != "api":
        return None
    return parts[1]


TEST_KINDS = {"unit-test", "integration-test", "architecture-test", "fixture", "composition-test"}
SERVICE_OWNED_E2E_SUFFIXES = (".E2ETests.Server", ".E2ETests.Web", ".E2ETests.Workers", ".E2ETests.Stripe")
SOURCE_MODE_E2E_COMPOSITION = "api/tests/Concertable.E2E.Source/Concertable.E2E.Source.csproj"


def is_e2e_name(name: str) -> bool:
    return ".E2ETests" in name or ".E2E." in name or name.endswith(".E2E")


def classify(rel_path: str) -> str:
    name = Path(rel_path).stem
    if is_e2e_name(name):
        return "e2e"
    if name.endswith(".TestKit"):
        return "testkit"
    if name.endswith(("Fixtures", ".IntegrationTests.Fixtures")):
        return "fixture"
    if name.endswith(".IntegrationTests"):
        return "integration-test"
    if name.endswith(".UnitTests"):
        return "unit-test"
    if name.endswith(".ArchitectureTests"):
        return "architecture-test"
    if name.endswith(".CompositionTests"):
        return "composition-test"
    if name.endswith(".AppHost"):
        return "apphost"
    if ".AppHost" in name:
        return "apphost-support"
    return "runtime"


def target_of(rel_path: str, area: str | None) -> str | None:
    name = Path(rel_path).stem
    if is_e2e_name(name):
        if name.endswith(SERVICE_OWNED_E2E_SUFFIXES):
            return AREA_TARGETS.get(area) if area else None
        return "system"
    return AREA_TARGETS.get(area) if area else None


def read(path: Path) -> str:
    try:
        return path.read_text(encoding="utf-8-sig")
    except (OSError, UnicodeDecodeError):
        return ""


def build_dotnet(files: list[str]) -> dict:
    csprojs = sorted(f for f in files if f.endswith(".csproj"))
    projects: dict[str, dict] = {}
    edges: list[dict] = []
    project_edges: list[dict] = []

    for rel in csprojs:
        text = read(REPO_ROOT / rel)
        area = area_of(rel)
        packable = re.search(r"<IsPackable>\s*true\s*</IsPackable>", text, re.I) is not None
        projects[rel] = {
            "name": Path(rel).stem,
            "area": area,
            "target": target_of(rel, area),
            "kind": classify(rel),
            "packable": packable,
        }

        base = (REPO_ROOT / rel).parent
        for ref in re.findall(r'<ProjectReference\s+Include="([^"]+)"', text):
            resolved = (base / ref.replace("\\", "/")).resolve()
            try:
                target_rel = resolved.relative_to(REPO_ROOT).as_posix()
            except ValueError:
                continue
            src_area, dst_area = area, area_of(target_rel)
            if src_area and dst_area:
                edge = {
                    "from": rel,
                    "fromArea": src_area,
                    "fromTarget": target_of(rel, src_area),
                    "to": target_rel,
                    "toArea": dst_area,
                    "toTarget": target_of(target_rel, dst_area),
                    "fromKind": classify(rel),
                }
                project_edges.append(edge)
                if src_area != dst_area:
                    edges.append(edge)

    cross_area_by_kind: dict[str, int] = defaultdict(int)
    for e in edges:
        cross_area_by_kind[e["fromKind"]] += 1

    # Only an edge crossing a future REPOSITORY boundary matters. An edge between two
    # areas that land in the same target repo (DataAccess -> Kernel, both
    # platform-dotnet) survives the split untouched.
    cross_target = [e for e in project_edges if e["fromTarget"] != e["toTarget"]]

    # Composition-time (*.Hosting) and test-tree edges are resolved by their own
    # checkpoints (Hosting packages; moving full-system E2E to `system`). A cross-target
    # edge from a production runtime project is the hard blocker: that deployable
    # closure would not compile once the repos are separate.
    def is_runtime_closure(rel: str) -> bool:
        return "/tests/" not in rel and not Path(rel).stem.endswith(".Hosting")

    blocking = sorted(
        (e for e in cross_target if e["fromKind"] == "runtime" and is_runtime_closure(e["from"])),
        key=lambda e: (e["from"], e["to"]),
    )

    cross_target_by_kind: dict[str, int] = defaultdict(int)
    for e in cross_target:
        cross_target_by_kind[e["fromKind"]] += 1

    source_mode_e2e = sorted(
        (e for e in cross_target if e["from"] == SOURCE_MODE_E2E_COMPOSITION),
        key=lambda e: (e["from"], e["to"]),
    )
    blocking_e2e = sorted(
        (
            e
            for e in cross_target
            if e["fromKind"] == "e2e" and e["from"] != SOURCE_MODE_E2E_COMPOSITION
        ),
        key=lambda e: (e["from"], e["to"]),
    )

    # EnforceServiceBoundary exempts the test tier, so this is the only check that sees such an edge.
    # A *.Hosting target waits on the packable-Hosting stage and E2E on the E2E-to-system stage, so
    # neither counts yet.
    blocking_test = sorted(
        (
            e
            for e in cross_target
            if e["fromKind"] in TEST_KINDS and not Path(e["to"]).stem.endswith(".Hosting")
        ),
        key=lambda e: (e["from"], e["to"]),
    )

    forbidden_runtime_tooling = sorted(
        (
            e
            for e in project_edges
            if classify(e["from"]) == "runtime"
            and is_runtime_closure(e["from"])
            and (
                Path(e["to"]).stem.endswith(".Hosting")
                or Path(e["to"]).stem.endswith(".TestKit")
            )
        ),
        key=lambda e: (e["from"], e["to"]),
    )

    return {
        "projectCount": len(projects),
        "projects": projects,
        "crossAreaEdgeCount": len(edges),
        "crossAreaEdgesByKind": dict(sorted(cross_area_by_kind.items())),
        "crossTargetEdgeCount": len(cross_target),
        "crossTargetEdgesByKind": dict(sorted(cross_target_by_kind.items())),
        "crossTargetEdges": sorted(cross_target, key=lambda e: (e["from"], e["to"])),
        "blockingRuntimeEdges": blocking,
        "blockingTestEdges": blocking_test,
        "blockingE2EEdges": blocking_e2e,
        "sourceModeE2EEdges": source_mode_e2e,
        "forbiddenRuntimeToolingEdges": forbidden_runtime_tooling,
        "packableProjects": sorted(p for p, v in projects.items() if v["packable"]),
    }


def build_frontend(files: list[str]) -> dict:
    manifests = sorted(
        f
        for f in files
        if f.endswith("package.json") and "node_modules" not in f and f.startswith("app/")
    )
    workspaces: dict[str, dict] = {}
    for rel in manifests:
        try:
            data = json.loads(read(REPO_ROOT / rel) or "{}")
        except json.JSONDecodeError:
            data = {}
        folder = str(Path(rel).parent).replace("\\", "/")
        deps = {
            **(data.get("dependencies") or {}),
            **(data.get("devDependencies") or {}),
        }
        target = next(
            (t for prefix, t in FRONTEND_TARGETS.items() if folder == prefix or folder.startswith(prefix + "/")),
            "(dissolves)" if folder in DISSOLVES_AT_CUT else None,
        )
        workspaces[folder] = {
            "packageName": data.get("name"),
            "private": bool(data.get("private", False)),
            "target": target,
            "concertableDeps": sorted(d for d in deps if d.startswith("@concertable/")),
        }
    return {"workspaceCount": len(workspaces), "workspaces": workspaces}


def build_migrations(files: list[str]) -> dict:
    snapshots = sorted(f for f in files if f.endswith("ModelSnapshot.cs"))
    by_area: dict[str, list[str]] = defaultdict(list)
    for rel in snapshots:
        by_area[area_of(rel) or "unknown"].append(Path(rel).stem)
    return {
        "snapshotCount": len(snapshots),
        "byArea": {k: sorted(v) for k, v in sorted(by_area.items())},
    }


def build_unassigned(dotnet: dict, frontend: dict) -> dict:
    areas = sorted({v["area"] for v in dotnet["projects"].values() if v["area"]})
    return {
        "dotnetAreas": [a for a in areas if a not in AREA_TARGETS],
        "frontendWorkspaces": sorted(
            path for path, v in frontend["workspaces"].items() if v["target"] is None
        ),
    }


def generate() -> dict:
    files = tracked_files()
    dotnet = build_dotnet(files)
    frontend = build_frontend(files)
    return {
        "_comment": (
            "Generated by eng/repository-split/inventory.py. Do not hand-edit; "
            "run the generator and commit the result."
        ),
        "dotnet": dotnet,
        "frontend": frontend,
        "migrations": build_migrations(files),
        "unassigned": build_unassigned(dotnet, frontend),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--check", action="store_true", help="fail if the committed inventory is stale")
    args = parser.parse_args()

    inventory = generate()
    current = json.dumps(inventory, indent=2, sort_keys=True) + "\n"

    if args.check:
        if not OUTPUT.exists():
            print(f"MISSING: {OUTPUT.relative_to(REPO_ROOT)} has not been generated", file=sys.stderr)
            return 1
        if OUTPUT.read_text(encoding="utf-8") != current:
            print(
                f"DRIFT: {OUTPUT.relative_to(REPO_ROOT)} is stale. "
                "Run: python eng/repository-split/inventory.py",
                file=sys.stderr,
            )
            return 1
        regressed = inventory["dotnet"]["blockingTestEdges"]
        if regressed:
            print(
                "TEST-TIER CROSS-REPOSITORY ProjectReference(s) — consume the platform library as a "
                "PackageReference; api/PlatformSourcePackages.targets swaps it back to source in-repo:",
                file=sys.stderr,
            )
            for e in regressed:
                print(f"  {e['from']} -> {e['to']}", file=sys.stderr)
            return 1
        blocking_e2e = inventory["dotnet"]["blockingE2EEdges"]
        if blocking_e2e:
            print(
                "E2E CROSS-REPOSITORY ProjectReference(s) OUTSIDE THE SOURCE-MODE COMPOSITION:",
                file=sys.stderr,
            )
            for e in blocking_e2e:
                print(f"  {e['from']} -> {e['to']}", file=sys.stderr)
            return 1
        forbidden_tooling = inventory["dotnet"]["forbiddenRuntimeToolingEdges"]
        if forbidden_tooling:
            print(
                "RUNTIME CLOSURE REFERENCES HOSTING/TESTKIT TOOLING:",
                file=sys.stderr,
            )
            for e in forbidden_tooling:
                print(f"  {e['from']} -> {e['to']}", file=sys.stderr)
            return 1
        print("inventory.json is current; no test-tier cross-repository ProjectReference")
        return 0

    OUTPUT.write_text(current, encoding="utf-8")
    print(f"wrote {OUTPUT.relative_to(REPO_ROOT)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
