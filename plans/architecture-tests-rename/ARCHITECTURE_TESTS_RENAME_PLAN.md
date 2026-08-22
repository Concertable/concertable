# Architecture-tests rename — collapse the "Composition" tier into "Architecture"

## Why

The DI/host-graph "composition" tests and the ArchUnitNET "architecture" tests are the same category —
architecture fitness functions (Ford's static-vs-dynamic is an axis *within* the family, not a boundary
between families). "CompositionTests" is a home-grown label off the "composition root" pattern, not a
recognized test tier; `ArchitectureTests` is the idiomatic umbrella. Collapse `Composition` into
`Architecture` so the repo has one name for one concept.

Confirmed decisions: **all six** composition-test projects collapse; the published helper package is
renamed too, as its own publish-then-bump chain (Phase 2).

## Structural constraint driving the shape

The tier gate keys on the project-name suffix `.ArchitectureTests` (`api/TestConventions.targets`), so a
service can hold at most **one** `.ArchitectureTests` project. B2B already has a static ArchUnit
`Concertable.B2B.ArchitectureTests`, so B2B's dynamic composition project is **merged into** it — one
architecture-test project per service. The other four services and the AppHost project have no collision
and are plain renames.

CI reality: there is **no architecture leg today** — `test.yml`/`test.ps1` gate Composition but never
discover `*.ArchitectureTests.csproj`, so B2B's existing architecture tests are currently unrun in CI. The
collapse builds an `architecture` leg, closing that pre-existing hole.

## Phase 1 — tier collapse — NON-BREAKING (shared lib untouched; every `using` and pin stays valid)

1. Rename projects `.CompositionTests` → `.ArchitectureTests` (folder, `.csproj`, `namespace`, test class,
   `AGENTS.md` title): AppHost, Auth, Payment, Search, Customer.
2. B2B: move `B2BCompositionTests.cs` into `Concertable.B2B.ArchitectureTests` (namespace →
   `Concertable.B2B.ArchitectureTests`), add that project's missing refs (composition-testing lib +
   `Concertable.B2B.Workers` / `.Seed.Simulator` / `.AppHost` / `Concertable.Auth.Hosting` /
   `Concertable.B2B.Admin.Contracts`); delete the `Concertable.B2B.CompositionTests` project + folder.
3. `api/TestConventions.targets`: delete the `.CompositionTests` tier line; drop `.CompositionTests` from
   the error message's suffix list.
4. `.agents/skill-routes.json`: repoint the `\.CompositionTests` route regex to `\.ArchitectureTests`
   (keep the `composition-testing` skill mapping — it documents the dynamic sub-tier activity).
5. `scripts/test.ps1`: `Composition` suite → `Architecture` suite in `all` and the subcommand
   (`\.ArchitectureTests$`).
6. `.github/workflows/test.yml`: `composition_projects`/`composition-tests` job + every `needs:` ref →
   `architecture_projects`/`architecture-tests`, glob `*.ArchitectureTests.csproj`.
7. `api/Concertable.slnx` project paths; docs: the affected `AGENTS.md`, `docs/INDEX.md`,
   `api/TECH_DEBT.md`, any `reviews/*` prose.
8. Gate: `dotnet build api/Concertable.slnx` to 0 errors; `./scripts/test.ps1 architecture` green.

## Phase 2 — published-package rename (separate publish-then-bump chain, after Phase 1 merges)

Rename `Concertable.Composition.Testing` → `Concertable.Testing.Architecture` (folder, `.csproj`,
`namespace`). It is a **published** package the six consumers pin via `Directory.Packages.props`, so per the
carve it cannot land in one PR:

- Producer PR: rename the lib; migrate ProjectReference consumers in-PR (AppHost, and B2B after the Phase 1
  merge). Merge → `publish-packages` publishes the new id.
- Platform-sync PR: the four PackageReference services go red on the old id; migrate their
  `PackageReference` + `PackageVersion` + `using` there, build `api/Concertable.slnx` to 0, push.
- Decide there whether to also rename the DI-validation *types* (`CompositionValidationOptions`,
  `ValidateComposition`, `CompositionTestArguments`). Default = keep — they name the composition-root
  validation *act*, where "composition" is accurate, unlike the tier label.

## Risks

- **B2B merge footprint:** folds the Aspire distributed-host runtime (AppHost/Workers/SeedSimulator/Auth
  hosting) into a previously-static ArchUnit project. Authorized as "same module" co-location; revisit if
  reader-clarity cost outweighs it.
- **CI leg swap:** the new `architecture-tests` leg must inherit the exact downstream `needs:` graph the
  `composition-tests` job fed, or gating silently drops.

## Done = grep gate

Phase 1 done when `grep -rniE "CompositionTests"` over the repo returns only the deliberate survivors
(the shared lib's `using Concertable.Composition.Testing` and package pins, cleared in Phase 2; unrelated
`TypedErrorCompositionTests` / `ConcertWorkflowCompositionTests` unit classes). Whole rename done when
`grep -rniE "composition\.testing|compositiontests"` is empty but that allowlist.
