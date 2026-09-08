# Code review — Refactor/seeding-collapse-and-generated-ids

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `22278f8ba4e316a8e7b5f11529b2d5da51c4996d`  `(2026-09-08)`
**Judgment:** `approved`

## Review pass — 2026-09-08 — full

**Candidate base:** `ef8d505fdb0133d8b967d58634022192169e90e1`
**Candidate head:** `81e71ad9ae89ea5f9df0f33742dc35831f3f6c09`
**Candidate branch:** `Refactor/seeding-collapse-and-generated-ids`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:fa79363903d5aaa06bdaa1f6cbad68715ba32aa091fbfd919033f9a2cb206caa` `(12 paths)`
**Candidate bundle:** `C:/Users/TOMMYS~1/AppData/Local/Temp/claude/C--Users-TommySeery-source-repos-Concertable/aa836791-44eb-4f47-90ee-746a762db16f/scratchpad/review-bundle-seeding`
**Candidate bundle identity:** `sha256:185dbfdfe87d10cc291465b05ef873a10fcf7ccf2c5cb3f258036d58885d1d0e`
**Work-order path:** `reviews/Refactor-seeding-collapse-and-generated-ids.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

**Provenance caveat, stated plainly:** this pass is the authoring agent reviewing its own diff, not the
independent-lens fan-out the `review` skill dispatches — the session was instructed not to spawn
subagents. Read it as a self-review. Every claim below was verified by execution or by reading the frozen
bundle, never asserted from a name scan; the two findings it did surface were both real and are fixed.

### Findings

- [x] **SEED-1 — MEDIUM — correctness** — `api/Concertable.Shared/src/Seed/Concertable.Seed.Shared/Extensions/SeedingServiceCollectionExtensions.cs:18`
  `AddSeeder<TContext, TSeeder>()` constrained only `TSeeder : class, ISeeder`, and `IStandInSeeder`
  derives from `ISeeder` — so a stand-in seeder could be registered into the chain every host runs. That
  is the one thing `IStandInSeeder` exists to prevent: it writes rows whose only production write path is
  a handler reacting to a producer's event (`UserEntity`, `AdminProfileEntity`, the read-model
  projections), and a direct insert of those in dev or E2E is exactly what the `seeding` standard's
  forbidden-table inventory rules out. Worse, it would fail silently — the interface would still read as
  documentation while the guarantee was gone.
  Fixed in `0aad26c81`: `AddSeeder` throws for an `IStandInSeeder` and `AddStandInSeeder` accepts only
  one, so the split is enforced at registration rather than by call-site convention. Covered by
  `SeederRegistrationTests`; sabotage-verified (neutering the check fails only
  `AddSeeder_StandInSeeder_ThrowsAndRegistersNothing`).

- [x] **SEED-2 — LOW — observability** — `api/Concertable.Shared/src/Seed/Concertable.Seed.Shared/SeedChain.cs:22`
  `SeedChain.SeedAsync` emitted `BeginDbInitialization` / `DbInitializationComplete`, but it only seeds —
  migration stays with the caller and, under `UseSeeding`, with EF. Two phases claiming one message sends
  anyone diagnosing a seeding failure to the wrong phase, and PR 2 will have the initializer emitting the
  same lines for the migrate sweep.
  Fixed in `81e71ad9a`: added `BeginSeedChain` / `SeedChainComplete` and pointed `SeedChain` at them. The
  four original messages stay, because `InternalsVisibleTo` exposes them to `Concertable.Auth`,
  `Concertable.B2B.Web` and `Concertable.Customer.Web`, whose initializers still call them at the pinned
  package version.

### Verified, not findings

Each of these was a live suspicion checked against the code or by execution, and cleared:

- **Scoped resolution inside the EF callback.** `UseSeedChain` captures the `sp` from
  `AddDbContext<T>((sp, opt) => …)` and resolves keyed seeders from it. That is only safe if no context is
  pooled — `AddDbContextPool`/`AddPooledDbContextFactory` build options once against the *root* provider,
  and resolving a scoped seeder there would throw. Grepped: neither appears anywhere in `api/`, and all 27
  context registrations use the `(sp, opt)` `AddDbContext` overload. Safe.
- **Re-entrant context construction.** The callback resolves seeders whose constructors take the
  `DbContext`, but it runs from `Migrate()`/`MigrateAsync()` — after construction, with the instance
  already cached in the scope — so it returns the same context rather than recursing.
- **Both EF delegates registered.** EF's own resource strings confirm it throws when a store operation
  takes the path whose delegate is missing ("no synchronous seed delegate has been provided, however an
  asynchronous seed delegate was"). Both are registered. The sync path blocks with
  `GetAwaiter().GetResult()`; retained deliberately — it is the shape EF's documentation uses, ASP.NET
  Core has no synchronization context to deadlock against, every call site in this repo uses
  `MigrateAsync`, and it runs once at startup.
- **Unit-tier gate.** Project name ends `.UnitTests`, so `TestConventions.targets` resolves the Unit tier.
  It references no banned host package, and `api/BannedSymbols.UnitTests.txt` bans no EF Core symbol, so
  `StubDbContext : DbContext` (a type key, never instantiated or connected) is legitimate. Builds with 0
  warnings.
- **`IDbSeeder` retention is load-bearing, not caution.** Proven both ways by loading the built
  `Concertable.B2B.Venue.Infrastructure.dll` against each version of this assembly: without `IDbSeeder`,
  `Could not load type 'Concertable.Seed.Shared.IDbSeeder'`; with it, `VenueDevSeeder` resolves as
  `IDevSeeder, IDbSeeder, ISeeder`.

### Risk carried into PR 2

Not a defect in this diff — recorded because this diff is what creates the exposure, and PR 2 owns it.

`UseSeedChain` resolves `GetKeyedServices<ISeeder>(context.GetType())`, so a context with no registered
seeder seeds nothing and says nothing. That silence is correct for most contexts (read contexts, inbox,
outbox) and therefore cannot be made an error — but it means a mistyped or missing `AddSeeder<TContext, …>`
in PR 2 degrades to a silent no-op, which is precisely #633's failure shape. **The per-service seed-chain
integration test (part 4 of the plan) is the gate that catches it, and it must assert seeded rows exist
rather than merely that the chain completed.** A chain that ran nothing completes fine.

## Review pass — 2026-09-08 — incremental

**Candidate base:** `95d1b0d788b12a9c8181e299cd94bbea884899bb`
**Candidate head:** `22278f8ba4e316a8e7b5f11529b2d5da51c4996d`
**Candidate branch:** `Refactor/seeding-collapse-and-generated-ids`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:57930c2b6ebef2c04dac700eafb7ed52b7bd1b545479c5a2337e7c146b3900bb` `(2 paths)`
**Candidate bundle:** `C:/Users/TOMMYS~1/AppData/Local/Temp/claude/C--Users-TommySeery-source-repos-Concertable/aa836791-44eb-4f47-90ee-746a762db16f/scratchpad/review-bundle-seeding-inc`
**Candidate bundle identity:** `sha256:53c00f7ef47fa8072d4a234f94bb6c34fb2bffe8a9d31e091461ae9d442edf80`
**Work-order path:** `reviews/Refactor-seeding-collapse-and-generated-ids.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Adds `UseSeedChainTests`, closing the coverage gap the first pass left implicit: `SeedChain` was tested
in isolation, but nothing proved EF invokes the callback at all, so the headline change rested on the API
compiling. Also adds the `Microsoft.EntityFrameworkCore.InMemory` reference it needs — already pinned in
`api/Concertable.Shared/Directory.Packages.props`, and not a banned unit-tier symbol.

### Findings

- [x] **SEED-3 — MEDIUM — test-coverage** — `api/Concertable.Shared/tests/Concertable.Seed.Shared.UnitTests/UseSeedChainTests.cs:50`
  `UseSeedChain_ContextCreated_RunsNoOtherContextsSeeders` asserted only
  `DoesNotContain("other-context")`, which passes vacuously when no seeder runs at all — so it could not
  distinguish correct per-context scoping from a callback EF never invokes. Caught by sabotage, not by
  reading: reducing `UseSeedChain` to `=> builder` failed only the ordering test while this one stayed
  green, which is precisely the "gate looks like proof but does not bite" failure the branch is meant to
  design out.
  Fixed in the same commit: it now also asserts the seeded context's own seeder ran, and the same mutation
  fails both tests.
