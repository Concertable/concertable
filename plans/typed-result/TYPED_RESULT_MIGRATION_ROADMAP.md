# Typed Result migration roadmap

> **Roadmap** for adopting Concertable-owned `Result` and `Option` contracts across every backend
> microservice. This is the living cross-workstream dependency map, not an implementation plan. Each
> buildable item spins off its own `_PLAN.md` and `_PROGRESS.md`; see
> [`../agents/ROADMAP.md`](../agents/ROADMAP.md).
>
> **Goal:** remove third-party Result carriers and ambiguous application/module lookup contracts while
> keeping every service independently buildable, deployable, and package-clean.
>
> **Canonical conventions:** [`../../api/agents/CODE_CONVENTIONS.md`](../../api/agents/CODE_CONVENTIONS.md)
> “Typed operation Results.” Backend ownership and package rules live in
> [`../../api/ARCHITECTURE.md`](../../api/ARCHITECTURE.md).

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

### In flight — existing owners, do not offer or duplicate

- [ ] 🟠 **Payment owned-result migration.** Exclusive owner:
  `Feature/PaymentOwnedResultExpansion` at
  `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\PaymentOwnedResultExpansion`.
  The reviewed-`Money` decision is implemented and Payment's local owner gates are green. Delivery is
  waiting on incremental review, a current-main sync and committed-tree re-verification before
  push/PR, package publication, and the generated platform-sync PR.
  `Feature/CommissionBindingDeferredPricing` / PR #296 is frozen donor history, not a second owner. No
  other workstream may recreate Payment contracts or bridge the unpublished package with local source
  references.
- [ ] 🟠 **B2B typed-result migration.** Exclusive owner: `Refactor/B2BTypedResultMigration` at
  `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration`.
  Payment-independent Deal, Tenant, Venue, Artist, User, and Concert-core checkpoints are complete.
  Concert payment/cancel/finish workflows and final FluentResults removal remain blocked on the
  published Payment package and green platform sync. Resume the existing plan; do not spin off a new
  B2B plan.
- [ ] 🟠 **Customer Ticket purchase/checkout slice.** Exclusive owner:
  `Feature/TypedResultMigrationPhase2` at
  `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\TypedResultMigrationPhase2`.
  Its local Ticket/Concert owned-result core and validation boundary coverage are complete. The final
  Payment-client swap remains blocked on Payment publication and platform sync. This owner includes
  Ticket, Concert, Customer Payment clients/mocks, purchase, checkout, and their integration/API
  coverage; parallel Customer work must not touch those surfaces.

### Ready — may be planned and implemented in parallel

- [ ] 🟡 **Customer non-Payment outcomes and lookups.** Ready now and independent of B2B and Payment.
  Spin off `CUSTOMER_OUTCOMES_PLAN.md` / `CUSTOMER_OUTCOMES_PROGRESS.md` on
  `Feature/typed-result_customer-outcomes`.
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

- [ ] 🟡 **Auth expected-outcome migration.** Ready now and independent of every other service track.
  Spin off `AUTH_OUTCOMES_PLAN.md` / `AUTH_OUTCOMES_PROGRESS.md` on
  `Feature/typed-result_auth-outcomes`.
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
- [ ] 🔴 **Released .NET native-union cutover.** Blocked until the released .NET/C# toolchain supports
  the required union semantics and Concertable is ready to upgrade.
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
Published Kernel foundation
├── Payment migration ──publish/sync──┬── B2B Payment workflows
│                                    └── Customer Ticket Payment cutover
├── B2B Payment-independent migration (already in flight)
├── Customer non-Payment outcomes (ready)
├── Search contract normalization (ready)
└── Auth expected outcomes (ready)

All service tracks complete
└── Shared/background audit
    └── Repository cleanup and enforcement

Released .NET native unions
└── Native-union cutover
```

Customer non-Payment, Search, and Auth have no dependency on B2B or the unpublished Payment package
and may run concurrently in isolated worktrees. Their diffs must remain service-owned so one branch
cannot silently become the integration branch for another.

## Shared migration rules

- `Result` and `Option` are in-process contracts only. HTTP, protobuf, events, persistence, and other
  wire boundaries retain owned transport contracts and map at the service edge.
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
- The final cleanup is the only removal point for compatibility surfaces still needed by another
  branch. Parallel service branches remove dependencies only from their own completed closures.

## Epic definition of done

- Every backend service uses the owned Kernel vocabulary consistently at in-process boundaries.
- Every service’s unit/integration gates, Release solution build, architecture tests, and standalone
  carve pass on the published package closure.
- Payment, B2B, and Customer package cutovers and platform-sync PRs are terminal and green.
- Repository-wide inventories find no FluentResults/CSharpFunctionalExtensions production use,
  third-party Result/Option public signatures, nullable non-persistence single-item lookup contracts,
  Option-wrapped collections, or Result-bearing wire DTOs.
- Architecture enforcement prevents those contracts from returning.
- The released native-union cutover is complete and Dunet is removed.
