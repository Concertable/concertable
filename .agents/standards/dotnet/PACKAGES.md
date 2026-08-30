# Packages — every service builds from its own published closure

**This is executed, not aspirational.** Every backend service consumes the shared platform and cross-service
contracts as private NuGet `PackageReference`s from the org feed `https://nuget.pkg.github.com/Concertable`,
never as `ProjectReference`s reaching into sibling folders. Carving any service into its own tree produces a
build that restores and compiles.

| Project type | Today | Split-repo |
|---|---|---|
| `Concertable.X.Contracts` (events, DTOs) | `PackageReference` (feed) | Private NuGet — unchanged |
| `Concertable.X.Seed.Contracts` (canonical seed data) | `PackageReference` (feed) | Private NuGet — unchanged |
| Shared platform and seeding infra (`Kernel`, `Messaging.*`, `Seed.Shared`) | `PackageReference` (feed) | Private NuGet — unchanged |
| `Concertable.X.Seed.Simulator` (Worker host) | `AddProject<Projects.X>()` in AppHost | Container image, `AddContainer(...)` |

Within a service, intra-folder references stay `ProjectReference`. Two layers are **exempt** and keep their
cross-folder `ProjectReference`s by design: the **AppHost composition layer** (the executable `*.AppHost`
projects plus service-owned `*.Hosting` libraries, which reference sibling deployables to orchestrate the dev
topology) and the **full-stack E2E harness**, which boots several services together — exempt only until it
moves to its own repository.

**A service's own test tier is not exempt.** Unit, integration, fixture and architecture projects consume the
shared platform test libraries (`Concertable.Testing`, `.Testing.Architecture`, `.Testing.Integration`, plus
`Seed.*` and `Messaging.*` where a fixture needs them) as `PackageReference`s, because the build-time
guardrail below exempts test projects — so a cross-folder `ProjectReference` there is invisible in the
monorepo and a project-not-found the moment the service is extracted. To keep the inner loop free of a
publish round-trip, `api/PlatformSourcePackages.targets` maps each of those packages to its in-repo source and
swaps the reference back to a `ProjectReference` while that source is on disk. A carved repo has neither the
targets file nor the source, so the package declaration stands **with no edit**. Force package mode in place
with `-p:UseLocalPlatformPackages=true` (what `scripts/local-platform.ps1` passes).

## Per-folder build closures — never repo-root config

Each service folder and `api/Concertable.Shared/` carries its **own** `nuget.config`,
`Directory.Packages.props` (CPM), and `Directory.Build.props`/`.targets`. There is deliberately **no**
repo-root or `api/`-root *version* config: a carve takes only the service folder, so anything above it would
be left behind and break the standalone restore. Adding a root `Directory.Packages.props` because it is "the
monorepo idiom" is the trap this separation exists to avoid.

Shared build infra *may* sit at `api/` root — `TestConventions.targets`, `BannedSymbols.txt`,
`PlatformSourcePackages.targets` all do — on one condition: **every reference to it is `Exists()`-guarded, so
a carve that leaves it behind degrades to the intended standalone shape rather than failing.** Two traps when
adding one:

- **A nested `Directory.Build.props`/`.targets` shadows the one above it** — MSBuild imports only the nearest,
  so a folder that introduces its own must re-import whatever the outer file supplied. It fails silently: the
  build still succeeds, minus whatever the shadowed file was enforcing.
- Unguarded, or holding a version, it becomes exactly the repo-root config this section forbids.

## `UseLocalCore` is a local inner loop, never committed

The churny shared core (`Concertable.Kernel`, `Concertable.Messaging.*`) is consumed as packages by default,
which the standalone carve and CI require — they have no sibling source on disk. Because B2B and Customer
co-change with the core constantly, pass `-p:UseLocalCore=true` (or set `CONCERTABLE_LOCAL_CORE=1`) to swap
those packages for in-repo `ProjectReference`s and skip the publish/restore round-trip. The swap is
implemented in each folder's `Directory.Build.targets`, where `ChurnyCorePackage` is the id-to-path source of
truth.

**Never set `UseLocalCore=true` in committed config.** It breaks the carve.

## Reunion is directly owned, never transitively supplied

Services reference the Reunion package family themselves, at the service's own pin.

| Package | Owns |
|---|---|
| `Reunion` | `Result`, `Result<TValue>`, `Result<TValue, TError>`, `UnitResult<TError>`, `Option<T>`, their named cases, composition, collection and task extensions |
| `Reunion.Errors` | `IError`, `ErrorDefinition`, `ErrorKind`, `ValidationErrors`, `ErrorCodeAttribute`, definition factories |
| `Reunion.Validation` | `ValidationResult`, its `Valid`/`Invalid` cases, validation accumulation |
| `Reunion.AspNetCore` | Minimal API and MVC terminal adapters |

Keep every Reunion package in a service on **one** version, in that service's own
`Directory.Packages.props`; the current baseline is `0.1.0-alpha.8`. Every project that compiles against a
Reunion carrier references its owning package directly rather than relying on another project's dependency.

When a published Concertable package exposes a Reunion carrier in its public API, that package also compiles
against Reunion but sets `PrivateAssets="all"` on the reference. Its packed nuspec must not declare Reunion as
a dependency. Every consuming project still owns a compatible direct Reunion pin; Kernel's private build
reference never supplies it transitively.

The legacy `Concertable.Kernel.Functional` carriers and `Concertable.Shared.Api.Results` terminals survive
only until their owning migrations remove them; new and changed contracts use Reunion directly.
`ReunionArchitectureTests` and `TypedResultArchitectureTests` reject those legacy surfaces. Package
inspection verifies private references are absent from nuspec dependencies, and service carve builds verify
that every consumer's direct closure restores and compiles.

The carriers may appear in application, module and published client signatures, but never as HTTP, protobuf,
event, persistence or other wire payloads — each transport maps to an owned wire contract at its service edge.

## How separation is enforced

- **Build-time guardrail, local and CI.** Each service folder's `Directory.Build.targets` fails the build if
  a deployable-closure project gains a `ProjectReference` escaping the service folder. AppHost composition
  (`*.AppHost`, `*.Hosting`), Tests projects and `UseLocalCore=true` builds are exempt
  (`EnforceServiceBoundary`).
- **Carve CI gates.** `carve-{auth,payment,search,b2b,customer}` jobs `git archive` each service folder,
  restore from the feed, and build the closure **and its non-E2E test tier** standalone — so an escaping
  reference, or a package missing from the feed, fails CI as a project-not-found.
- **`split-inventory` CI gate.** `eng/repository-split/inventory.py --check` fails on any test-tier
  cross-repository `ProjectReference` (`blockingTestEdges`), repo-wide and without a build — the enforcement
  the build-time guardrail structurally cannot give, since it exempts tests.

**Local prereq:** building any solution that consumes the feed needs a `GITHUB_PACKAGES_TOKEN` PAT with
`read:packages` in the environment. CI uses the repo `GITHUB_TOKEN`.

## A published contract change is a two-step release

Because consumers bind cross-service code as version-pinned packages, changing a *published* contract —
renaming or removing a public type consumers use, changing a return type, moving a DTO between packages, or
adding an abstract interface member — is inherently two steps: publish the new package, then bump each
consumer's `ConcertablePlatformVersion` and migrate. A pin can only move to a version already on the feed.
A new member is additive only when existing consumers require no source change; an abstract interface member
never qualifies.

This is not monorepo friction to delete. It is what independently-deployable services do, and it survives the
repo split.

A Result-based change owes one thing beyond the branch, definition and terminal coverage the generic skills
require: exact package versions with no mixed Reunion graph, and a service-carve restore and build for every
changed package closure. Build and test against that closure, not only the monorepo source graph.

`platform-sync.yml` automates the second step — after packages publish on main it opens one
`chore/platform-sync-<version>` PR bumping every service's pin. Non-breaking goes green and merges hands-off;
breaking goes red at exactly the consumers to migrate, and the migration is done *in that PR*, which is legal
now that the package exists. Owning that PR to green is the `merging` skill.
