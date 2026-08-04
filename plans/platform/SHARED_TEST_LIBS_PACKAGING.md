# RFC / plan — shared test libraries: publish as packages, or document carve-exempt

> **Status:** decision pending (architecture, root-level per [`api/ARCHITECTURE.md`](../../api/ARCHITECTURE.md)).
> Captured from the door-revenue work (a shared `Money` test helper landed same-PR only because the
> shared test libs are ProjectReferenced — a carve leak). Tracked in [`api/TECH_DEBT.md`](../../api/TECH_DEBT.md).
> **Not** to be bundled into a feature PR — its own change once decided.

## The problem

`Concertable.Testing`, `Concertable.Testing.Integration`, and the shared `Concertable.E2ETests` harness
live under `api/Concertable.Shared/tests/` — the Shared "repo". But every consuming test project reaches
them by a `ProjectReference` that **escapes its own service folder**, e.g.

```
api/Concertable.B2B/src/Modules/Concert/Tests/…/*.csproj
    → ..\..\..\..\..\..\Concertable.Shared\tests\Concertable.Testing\Concertable.Testing.csproj
```

That is the exact cross-folder escape the **runtime carve** forbids for service projects (the
`PackageReference, never a ProjectReference` guard in the service `.csproj`s, enforced by
`Directory.Build.props`). The carve is about **repo independence**, not only runtime: when
`Concertable.Shared/` becomes its own repo, anything in another repo that depends on it — runtime *or
test* — must consume it as a **package**, because a cross-repo ProjectReference cannot exist. So the
shared test libs are a genuine leak: runtime shared libs (Kernel, Messaging) publish + are pinned, but
the shared test libs alone reach straight into every service's test tree.

`Concertable.Testing` even declares `IsPackable=true` with **zero** package consumers — a half-committed
intent that confirms it was meant to be a package.

## Options

### A — Publish the shared test libs as (test-support) packages *(recommended)*
Consume by pinned `PackageReference` (`ConcertablePlatformVersion`), exactly like Kernel/Messaging.
- **Pro:** carve-consistent; split-ready; kills the leak; publishable test-support packages are normal
  (`Microsoft.AspNetCore.Mvc.Testing`).
- **Con:** every shared-test-helper edit then takes the **publish-first + pin-bump cycle** (as we just
  did for the Kernel combinator) — for code that churns more than Kernel. Mitigated by the existing
  `UseLocalCore` inner-loop swap being extended to the test libs (source locally, package in CI/split).

### B — Document test infra as carve-exempt
State that dev-only test utilities are shared by ProjectReference (they never ship in a service
runtime), and **delete** the misleading `IsPackable=true`.
- **Pro:** keeps test-infra iteration fast (no publish cycle).
- **Con:** a real repo split must then publish/vendor the test libs as a one-time migration; the carve
  is no longer uniform. Requires a written exemption in `api/ARCHITECTURE.md` so it isn't just a leak.

## Recommendation

**A.** The Shared tree is a repo boundary; test deps across it should obey the same rule as runtime
deps. Pay the publish friction uniformly (as already accepted for Kernel), and lean on `UseLocalCore`
to keep the inner loop fast.

## Execution (if A)

1. Add `Concertable.Testing`, `Concertable.Testing.Integration`, `Concertable.E2ETests` to the platform
   **publish set** + pin them in `Directory.Packages.props` (`ConcertablePlatformVersion`).
2. Convert **every** consumer `ProjectReference` → `PackageReference` — all module unit/integration test
   projects across **B2B, Customer, Search** + the B2B/Customer E2E projects (~15+ `.csproj`s).
3. Extend the `UseLocalCore` (`Directory.Build.targets`) swap to cover the test libs for local dev.
4. **Publish-first ordering:** this lands as its own PR *before* any later PR that adds a new shared
   test helper (same boundary as Kernel — a consumer can't pin a helper that isn't published yet).
5. Verify: full `Concertable.slnx` build + a green integration + UI-E2E run against the pinned packages.

## Non-goals / notes

- Not bundled into the door-revenue PR (#113) — `Money` rides there fine as a ProjectReference today.
- If B is chosen instead: delete `IsPackable=true` from `Concertable.Testing`, add the carve-exemption
  paragraph to `api/ARCHITECTURE.md`, and this plan is deleted.
