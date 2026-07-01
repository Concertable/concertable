# Service structure cleanup — `src/` + `tests/`, per service, monorepo and mirror

**Goal.** Give every backend service folder a consistent, clean, *standalone-service-repo*-shaped
layout — `src/` for deployable code, `tests/` for service-level suites — reflected **identically in
the monorepo and its auto-generated mirror** (the mirror is a faithful `git subtree split`, so the
only honest way to reshape it is to reshape the monorepo folder). Today each `api/Concertable.<Svc>/`
is a flat root dump of host projects + `Modules/` + `Seed/` + `Tests/` + config, and the layout is
*inconsistent between services* (B2B/Customer modular; Payment/Search flat; Auth a rooted,
source-globbing single project). This makes each service read as the self-contained microservice it
already is.

**Why now / vision.** The monorepo's job is **local orchestration of otherwise-independent services**
(run them together for dev ease); deployment is a future Terraform plan that consumes each service on
its own, at which point the monorepo is abandoned. So each `api/Concertable.<Svc>/` folder *should*
already look like a real service repo. This is that alignment — purely structural, no behavior change.

## Non-goals (explicitly out of scope)

- **Unifying modular vs flat.** Not forcing Payment/Search into `Modules/` (or B2B/Customer out of
  it). That's a structural-design opinion, not cleanup — its own later plan if ever.
- **The true-polyrepo cut.** Parked as future Terraform work (see git history of
  `POLYREPO_COMPLETION.md`, deleted alongside this plan's creation). This plan keeps the monorepo
  canonical; it only tidies folder shape.
- **No test *reorganization* beyond relocation.** Module tests stay co-located with their module (see
  the rule below) — we do **not** pull them into a global `tests/`.

## The tests rule (modular monolith — do not fight it)

A global `src`/`tests` split is a *layered-solution* convention; forcing it onto a modular monolith
shreds the vertical slices that are the whole point. So:

- **A module owns its tests.** Per-module test projects stay *inside* the module
  (`src/Modules/<Ctx>/Tests/…`). Yes, that means test projects live under `src/` — correct, because
  they're part of the slice, not free-floating.
- **`tests/` is for suites owned by the *service as a whole*** — the cross-cutting ones (e.g. B2B's
  `IntegrationTests.Fixtures`, `Workers.UnitTests`, `E2ETests`).

## Target layout (per service)

```
Concertable.<Svc>/
├── src/
│   ├── <host projects>            Web, Workers, AppHost, AppHost.Extensions, DataAccess/Client…
│   ├── Modules/                   (B2B, Customer only) each <Ctx>/ keeps its co-located Tests/
│   └── Seed/
├── tests/                         service-level cross-cutting suites only
├── Concertable.<Svc>.slnx         ── config stays at root (tooling expects it) ──
├── Directory.Build.props / .targets
├── Directory.Packages.props
├── nuget.config
└── README.md, ARCHITECTURE.md, TECH_DEBT.md, …
```

- **Payment/Search** (flat): `src/` holds the feature projects (Api/Application/Domain/Infrastructure/
  Web/Workers/Client); their `Tests/` → `tests/`.
- **Auth** (special — the ugliest): today a rooted single-project app that source-**globs** (hence the
  CS8802 glob-exclude hack from the AppHost work). Normalize into `src/Concertable.Auth/` (the web
  app) + `src/Concertable.Auth.AppHost/`, killing the glob-exclude. Any Auth tests → `tests/`.
- **Shared**: `src/` holds the libs (Kernel, Contracts, Shared.*, Seeding.*); `Tests/` → `tests/`.

## What every reshape must update in lockstep (per service, keep each phase green)

1. **The service's own `.slnx`** — every project path.
2. **The root `api/Concertable.slnx`** (166 project refs total) — that service's entries.
3. **`ProjectReference` paths** in every moved csproj that points at a sibling.
4. **`Directory.Build.props`/`.targets`** glob roots, if any glob is anchored below the service root.
5. **`EnforceServiceBoundary` guardrail** (in `Directory.Build.targets`) — confirm its escaping-ref
   detection still computes correctly with the extra `src/` segment (verify on the pilot).
6. **Carve gate in `.github/workflows/test.yml`** — the gate `git archive`s the whole folder (still
   captures `src/`+`tests/` fine), but its build target moves: `carve-auth` builds a root
   `Concertable.Auth.csproj` (→ `src/Concertable.Auth/…`); `carve-{payment,search,b2b,customer}` build
   a `Carve<Svc>.slnx` whose project paths shift.
7. **`mirror.yml`** — **unchanged**: the subtree prefix stays `api/Concertable.<Svc>`; the mirror
   inherits the new internal shape for free.
8. **Docs** referencing old paths (READMEs' build commands, `ARCHITECTURE.md`).

## Pilot recipe (established in Phase 1 — reuse for every remaining service)

The reshape is fully mechanical. Two scripts in this repo's scratchpad drove it; the logic:

1. **Move dirs (`git mv`).** Every top-level *project* dir (`<Svc>.Web/Workers/AppHost/
   AppHost.Extensions/DataAccess`, `Modules/`, `Seed/`) → `src/`. The service-level `Tests/` →
   `tests/` (lowercase; case-only rename needs a temp step on Windows: `git mv Tests _t && git mv _t
   tests`). Root files stay put: `.slnx`, `Directory.Build.props/.targets`, `Directory.Packages.props`,
   `nuget.config`, `*.md`. Module co-located `Tests/` stay capital-T under `src/Modules/<Ctx>/Tests/`.
2. **Rewrite paths.** Recompute *every* `Include`/`Project`/`Path` attribute (csproj `ProjectReference`
   **and** cross-dir `Content`, plus both `.slnx` files) whose resolved endpoint moves — relative-path
   recompute against new locations. Same-service refs recompute to identical values (idempotent);
   refs escaping the service gain one `../` (projects went one level deeper). Then update the
   plaintext path lists: `.github/workflows/test.yml` (unit/integration/E2E), `integration.ps1`,
   `unit.ps1`, `e2e.ps1`, `api/initial-migrations.ps1`.
3. **Guardrail regex — MUST fix per service.** Each service's `Directory.Build.props` gates
   `EnforceServiceBoundary` off for tests via a **case-sensitive** `[\\/]Tests[\\/]` regex. The new
   lowercase `tests/` won't match → change to `[\\/][Tt]ests[\\/]` (done for B2B). `ConcertableServiceRoot`
   is unchanged (props stays at service root), so the escaping-ref *math* needs no change — verified
   the guardrail still fires at the new `src/` depth.
4. **Carve gate (`test.yml`).** `carve-b2b`/`carve-customer` discover the closure via `find … -not
   -path '*/Tests/*'` — **add `-not -path '*/tests/*'`** so the lowercase service-level suites are
   still excluded (they reference cross-folder projects absent from the carve). `carve-payment`/
   `carve-search` use *explicit* `dotnet sln add` project lists instead → prefix those paths with
   `src/`. B2B closure = exactly 42 projects (unchanged count post-move).

## Phases — one service per phase, each independently shippable and green

Order: pilot with the most complex (B2B) to shake out the wiring recipe, then the rest, Auth's
normalize last-but-one, root sweep + E2E last.

- **Phase 1 — B2B (pilot). ✅ DONE.** Reshaped per target layout; recipe above. **Gate met:** full
  `dotnet build api/Concertable.slnx` green (0 errors); guardrail verified (fires on an injected escape
  at the new `src/` depth, no false positives); `carve-b2b` reproduced locally (build succeeded, 42
  closure projects); B2B integration green (sole red = `Update_ShouldReturn404_WhenNotOwner`, a SQL
  execution-timeout flake under 5-container load, passes clean in isolation).
- **Phase 2 — Customer** (also modular). **✅ DONE.** Same recipe as B2B (host projects + `Modules/` +
  `Seed/` → `src/`; `Tests/` → `tests/`; guardrail regex + `carve-customer` `find` fixed). **Gate met:**
  full `dotnet build` green (0 errors); `carve-customer` closure selects 34 projects (`tests/`
  excluded, no apphost); Customer integration green (Concert/Ticket/User pass; Review's 2 reds were
  SQL execution-timeout flakes under host load, pass 8/8 clean in isolation).
- **Phase 3 — Payment** (flat). **✅ DONE.** Flat feature projects + `Seed/` → `src/`; `Tests/` →
  `tests/`. Flat-service specifics vs the modular recipe: (a) `carve-payment` uses an **explicit**
  `dotnet sln add` list, not `find` — prefixed each entry with `src/` (`Seed/…` → `src/Seed/…`);
  (b) `initial-migrations.ps1` `--project`/`--startup-project` are path-*terminal* (`…/Concertable.
  Payment.Infrastructure`), so hand-edited rather than regex'd. Guardrail regex widened as usual.
  **Gate met:** full `dotnet build` green (0 errors); Payment UnitTests 25/25 (no integration project
  exists for Payment); all 9 carve-payment paths resolve under `src/`.
- **Phase 4 — Search** (flat). **Gate:** build + Search integration + `carve-search`.
- **Phase 5 — Auth (normalize).** Rooted-glob → `src/Concertable.Auth/`; drop the glob-exclude hack.
  Biggest single-service change. **Gate:** build + Auth boots standalone (`Concertable.Auth.AppHost`
  `/health` 200 + OIDC discovery) + `carve-auth`.
- **Phase 6 — Shared.** **Gate:** build + shared unit/integration where present.
- **Phase 7 — Root sweep + full verify.** Final pass over the root `Concertable.slnx`, any doc/path
  stragglers, delete leftover empty local dirs (`Modules/*/Concertable.<Ctx>.*` shells from the old
  rename — untracked, local-only). **Gate:** full `dotnet build` + **UI E2E** (`e2e-ui-debug`) — this
  phase's cumulative change touches build wiring across every service, so it meets the massive/risky
  bar in `plans/CLAUDE.md`.

## Branch

`Refactor/ServiceStructureCleanup` — this is code already on `master`, so a `Refactor/` branch, not a
`Feature/` one.
