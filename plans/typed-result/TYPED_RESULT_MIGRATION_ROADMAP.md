# Typed Result migration roadmap

> **Roadmap** for adopting one Reunion-backed `Result` and `Option` vocabulary across every backend
> microservice while keeping Concertable's error unions and HTTP policy application-owned. This is
> the living cross-workstream dependency map, not an implementation plan. Each
> buildable item spins off its own `_PLAN.md` and `_PROGRESS.md`; see
> [`../agents/ROADMAP.md`](../agents/ROADMAP.md).
>
> **Goal:** replace the temporary Concertable-owned carriers with Reunion, remove third-party Result
> carriers and ambiguous application/module lookup contracts, and keep every service independently
> buildable, deployable, and package-clean.
>
> **Canonical conventions:** [`../../api/agents/CODE_CONVENTIONS.md`](../../api/agents/CODE_CONVENTIONS.md)
> “Typed operation Results.” Backend ownership and package rules live in
> [`../../api/ARCHITECTURE.md`](../../api/ARCHITECTURE.md).
> Those conventions remain the current code contract until the Reunion producer and generated
> platform-sync consumer cutover land; the selected cutover design lives in
> [`REUNION_INTEGRATION_PLAN.md`](REUNION_INTEGRATION_PLAN.md).

---

## How to continue this roadmap

Run with an optional preferred item after the roadmap path:

```text
$continue-roadmap @plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md [preferred item in natural language]
```

For example, this one-shot invocation selects Customer when it remains ready:

```text
$continue-roadmap @plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md Customer non-Payment outcomes and lookups
```

The skill always classifies every item below against branches, worktrees, PRs, and package gates.
Omit the preference to receive the full list of unblocked and unowned choices. When a supplied
preference is ready, the skill treats it as the choice and directly emits the handoff; when it is
blocked, in flight, or not found, the skill explains why and offers the ready alternatives instead.
The handoff tells a fresh context to write the item's plan and progress ledger; it does not create or
implement the plan itself.

For parallel work, reserve the ready items sequentially: run `$continue-roadmap`, choose one item, and
let its handoff create the worktree/plan before selecting the next item. Once the distinct worktrees
exist, their planning and implementation may run concurrently. Starting several selection contexts
before any creates its worktree leaves a race in which each context can see the same item as unowned.

Every new item uses a sibling worktree and a branch named `Feature/typed-result_<name>` from fresh
`origin/main`, with its plan and ledger under `plans/typed-result/`. Existing legacy-named owners keep
their current branch and worktree rather than fragmenting in-flight work.

## Status

### Foundation — shipped

- [x] ✅ **Owned Kernel Result/Option foundation and Shared.Api terminals.** PR #290 published
  `Result`, `Result<TValue>`, `UnitResult<TError>`, `Result<TValue,TError>`, `Option<T>`, and
  `ValidationErrors`; platform-sync PR #291 delivered them to every service.
- [x] ✅ **Kernel-derived error definitions and natural error-case conventions.** Kernel derives the
  stable code and default humanized message from each named case, with `[ErrorCode]` reserved for
  published-code compatibility. The current convention uses Dunet and one exhaustive `Definition`
  switch for payload-free, payload-carrying, validation, and composite cases. Its canonical form is
  in `api/agents/CODE_CONVENTIONS.md`.

### Service tracks — shipped

- [x] ✅ **Search contract audit and normalization.** PR #380 normalized Search's in-process collection
  contracts to `IReadOnlyList<T>` and added Search-owned architecture enforcement without changing
  transport, projection, failure, or empty-result behavior. Publication and platform-sync PR #388
  delivered `ConcertablePlatformVersion` `0.1.0-alpha.0.827` to every service.
- [x] ✅ **Payment owned-result migration.** PR #392 replaced Payment's published FluentResults client
  surface with Concertable-owned typed Result/Option contracts and preserved reviewed-gross money
  invariants. Platform-sync PR #420 migrated B2B and Customer consumers, merged as `372be1041`, and
  post-merge publication delivered platform `0.1.0-alpha.0.857`. B2B and Auth handoffs are dispatched.

### In flight — existing owners, do not offer or duplicate

- [ ] 🟠 **B2B typed-result migration.** Exclusive owner: `Refactor/B2BTypedResultMigration` at
  `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration`.
  **Authoritative work is active and unpushed in the recorded local worktree as of 2026-08-09.**
  GitHub remains an incomplete inventory because no branch PR or remote branch exists.
  Payment-independent checkpoints 1-5 are complete. Payment PR #392, platform-sync PR #420, and
  platform `0.1.0-alpha.0.857` discharged the old package gate; checkpoints 6-7 own Concert
  payment/cancel/finish workflows and final B2B FluentResults removal. Preserve the branch unchanged
  until the Reunion Phase 4 generated platform-sync PR merges, then reconcile it once against that
  integrated baseline. Do not recreate, supersede, or independently apply the Reunion carrier cutover.
- [ ] 🟠 **Customer Ticket purchase/checkout slice.** Exclusive owner:
  `Feature/TypedResultMigrationPhase2` at
  `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\TypedResultMigrationPhase2`.
  PR #282 contains one unique commit and is 776 commits behind current `origin/main` at the
  2026-08-09 reconciliation.
  Its Ticket/Concert semantics remain exclusive, but its old carrier and CFE composition should not
  be revived wholesale. Recreate the slice from the post-Reunion integration baseline, recovering
  behavior and tests deliberately rather than rebasing the historical branch. Do not close or mutate
  PR #282 until that replacement is ready and approved.

- [ ] 🟠 **Customer non-Payment outcomes and lookups.** Exclusive owner:
  `Feature/typed-result_customer-outcomes` at
  `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_customer-outcomes`.
  Non-draft PR #425 is open at `e60219f7d`. It contains 29 unique commits and is 117 commits behind
  current `origin/main` at the 2026-08-09 reconciliation. Its semantic work does not exist elsewhere; preserve it,
  update it once against the shared integration baseline, rerun its normal delivery gates, then land
  it and its generated platform sync. Do not duplicate Reunion package or carrier edits on this PR.
  - Scope: Customer Review, Preference, User, Venue, and Artist application/module contracts;
    operation-specific expected failures; nullable persistence lookups converted to `Option<T>` at
    application/module boundaries; collection contracts normalized to empty `IReadOnlyList<T>`.
  - Preserve: repository nullability, infrastructure/cancellation exceptions, existing integration
    event boundaries, and Customer’s standalone build closure.
  - Out of scope: Ticket, Concert, Payment clients/mocks, purchase/checkout flows, shared Kernel API
    changes, cross-service runtime references, and Customer-wide FluentResults pin/package cleanup.
    The existing Ticket owner or final repository cleanup removes the shared package entry after the
    last Customer consumer is gone.
  - Planning sources: this roadmap, [`../../api/AGENTS.md`](../../api/AGENTS.md),
    [`../../api/ARCHITECTURE.md`](../../api/ARCHITECTURE.md),
    [`../../api/Concertable.Customer/ARCHITECTURE.md`](../../api/Concertable.Customer/ARCHITECTURE.md),
    and `api/agents/CODE_CONVENTIONS.md`.

- [ ] 🟠 **Auth expected-outcome migration.** Exclusive owner:
  `Feature/typed-result_auth-outcomes` at
  `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_auth-outcomes`.
  **Authoritative work is active and unpushed in the recorded local worktree as of 2026-08-09.**
  GitHub remains an incomplete inventory because no branch PR or remote branch exists.
  The Payment platform gate is discharged. Preserve the branch unchanged until the Reunion Phase 4
  generated platform-sync PR merges, then reconcile it once against that integrated baseline and
  continue verification, preflight, and delivery. Do not recreate, supersede, or independently apply
  the Reunion carrier cutover to Auth.
  - Scope: audit `IAuthService` null/bool/enum/void outcomes; model ordinary absence with `Option<T>`
    and caller-actionable refusal with operation-specific Result contracts where that distinction is
    useful; map owned in-process results to Duende/Razor/protocol behavior at the Auth edge.
  - Preserve: Auth’s credential-only responsibility, privacy-preserving indistinguishability of invalid
    credentials/accounts, framework-required wire/protocol shapes, capability queries whose complete
    contract is genuinely boolean, and infrastructure/cancellation exceptions.
  - Current evidence: Auth has no third-party Result dependency. Its review surface includes nullable
    login/logout returns, `RegisterResult`, password-change/reset and verification booleans, and silent
    email/reset no-ops used to avoid account disclosure.
  - Out of scope: roles, tenant/customer business concepts, downstream user projections, Payment/B2B/
    Customer runtime code, and shared Kernel API changes.
  - Planning sources: this roadmap, `api/AGENTS.md`, `api/ARCHITECTURE.md`,
    [`../../api/Concertable.Auth/ARCHITECTURE.md`](../../api/Concertable.Auth/ARCHITECTURE.md),
    `api/agents/CODE_CONVENTIONS.md`, Auth’s Pages/Services, and the coverage inventory established by
    the plan.

### Selected cross-cutting track — one owner, never duplicated on service PRs

- [ ] 🟡 **Reunion package integration and carrier cutover.** The design and operational state are in
  [`REUNION_INTEGRATION_PLAN.md`](REUNION_INTEGRATION_PLAN.md) and
  [`REUNION_INTEGRATION_PROGRESS.md`](REUNION_INTEGRATION_PROGRESS.md). Use Reunion commit `7bf5f66`
  for the initial package battle test. The Reunion package family is published; migrate
  Payment/Payment.Client first, then perform the repository-wide consumer contraction against its
  published package. B2B, Auth, Customer, and any other semantic owner consume that integrated
  baseline; they do not repeat package or carrier substitutions independently.

- [x] 🟢 **HTTP terminal ownership resolved upstream.** `Reunion.AspNetCore` already publishes the MVC
  and Minimal API Result/Option terminals, generic success mappers, ProblemDetails execution, and
  structured validation mapping. Retire `Refactor/typed-result_http-terminals` without publication;
  each service HTTP edge consumes the Reunion adapter directly during its carrier migration.

### Ready — may be planned and implemented in parallel

No other unowned service track is ready while the Reunion integration and existing B2B, Customer,
and Auth owners remain in flight.

### Blocked follow-ups

- [ ] 🔴 **Shared Kernel, Messaging, and background-path audit.** Plan only after the service tracks
  establish concrete remaining call sites. Service plans consume the published Kernel API as-is; a
  genuinely missing shared operation becomes its own additive Kernel publish/sync item rather than
  three service-local variants.
- [ ] 🔴 **Repository cleanup and architecture enforcement.** Blocked until Payment, B2B, Customer,
  Search, and Auth are complete. Remove the remaining third-party Result dependencies and compatibility
  surfaces, then enforce: no third-party Result/Option in public signatures, no nullable non-persistence
  single-item application/module/client lookups, no `Option`-wrapped collections, no Result on wire
  DTOs, and no controller-local typed-error status switches.
- [ ] 🔴 **Released .NET native-union error cutover.** Blocked until Concertable upgrades from net10.0
  to the released .NET/C# toolchain that supports the required union semantics. Reunion already
  supplies conventional union-compatible net10 assets and native net11 carrier support; this item is
  about Concertable-owned domain error unions, not replacing Reunion's Result family.
  - Replace each Dunet error declaration with the idiomatic released native declaration; retain the
    exhaustive `Definition` switch and preserve case names, payloads, definitions, published
    codes/messages/kinds, transport mappers, and contract tests. Keep `Definition` as a computed
    instance member when the released union syntax permits it; use an extension only if the final
    language design requires one.
  - Remove Dunet attributes, generated inheritance, and package references only after every error
    union compiles natively. The open-string wire reverse map remains a dictionary because no closed
    union can make a future transport string exhaustive.
  - Measure the released representation before changing generic constraints: Dunet cases are
    reference records today, while converting a native value union to `IError` or `object` may box.
    Keep operation and transport paths concretely typed as `TError` and use `IError` primarily as a
    definition constraint.
  - Decide separately whether the released native semantics improve on Kernel's owned Result/Option
    carriers. Replacing those carriers is not a prerequisite for migrating error unions.

## Parallel dependency map

```text
Reunion 7bf5f66 battle test ──Reunion publication──Payment.Client migration
                                                      └──publish── generated platform-sync consumer migration
                                                                  ├── B2B active owner reconciliation
                                                                  ├── Auth active owner reconciliation
                                                                  ├── Customer non-Payment PR #425 reconciliation
                                                                  └── Customer Ticket PR #282 semantic recreation

All service tracks complete
└── Shared/background audit
    └── Repository cleanup and enforcement

Released .NET native unions
└── Concertable-owned error-union cutover
```

B2B and Auth have authoritative unpushed local work that is now inventoried. Preserve both worktrees
until the integrated platform baseline lands; remote state alone remains insufficient. Service diffs
remain service-owned; the cross-cutting Reunion substitution happens once through Payment.Client and
the generated platform-sync consumer path.

## Shared migration rules

- Reunion `Result` and `Option` are in-process contracts only. HTTP, protobuf, events, persistence,
  and other wire boundaries retain owned transport contracts and map at the service edge.
- Every project directly owns the Reunion package whose API its source compiles against. Core and
  typed-error use takes direct `Reunion`/`Reunion.Errors` references; each service API/Web project
  mapping carriers takes `Reunion.AspNetCore`. Shared.Api neither distributes the adapter nor defines
  duplicate Result/Option HTTP terminals. Domain error unions and published semantics remain
  application-owned.
- Expected caller-actionable refusals use typed Results. Infrastructure failures, cancellation, and
  programmer/invariant defects remain exceptions. Do not catch them in Result combinators.
- Repository single-item lookups may remain nullable as a persistence concern. Application, module,
  service, and published client boundaries convert ordinary absence to `Option<T>`; collections return
  empty `IReadOnlyList<T>` values.
- Result/Option changes do not justify cross-service runtime references. Every service must build from
  its own published package closure and pass its standalone carve.
- A service plan may not expand the public Kernel API opportunistically. Record a concrete missing
  operation as a separate shared roadmap item, publish it additively, and wait for platform sync before
  consuming it.
- Every published Kernel, Payment, or Contracts change owns the complete merge → publish → platform
  sync → consumer migration sequence. Never leave a red platform-sync PR behind.
- Reunion package/carrier changes land once in their owning integration branches. Existing service
  migration PRs merge the integrated baseline and resolve semantic conflicts; they do not carry
  duplicate package pins, copied carriers, or HTTP adapter replacements.
- The final cleanup is the only removal point for compatibility surfaces still needed by another
  branch. Parallel service branches remove dependencies only from their own completed closures.

## Epic definition of done

- Every backend service uses Reunion's complete Result/Option family consistently at in-process
  boundaries, without removing or collapsing any Result family.
- Every service’s unit/integration gates, Release solution build, architecture tests, and standalone
  carve pass on the published package closure.
- Payment, B2B, and Customer package cutovers and platform-sync PRs are terminal and green.
- Repository-wide inventories find no FluentResults/CSharpFunctionalExtensions production use,
  third-party Result/Option public signatures, nullable non-persistence single-item lookup contracts,
  Option-wrapped collections, or Result-bearing wire DTOs.
- Architecture enforcement prevents those contracts from returning.
- The released native-union error cutover is complete and Dunet is removed.
