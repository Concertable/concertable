# .NET 11 adoption roadmap

> **Goal:** adopt .NET 11 where it unlocks materially better domain modelling, without forcing
> independently deployed net10 services or published cross-service contracts onto a preview runtime.
> Each implementation item owns a plan and progress ledger in this folder.
>
> **Current decision:** the first adoption slice is the B2B runtime after the Application, Booking,
> and Concert module split lands. Native unions will model closed internal values, beginning with the
> combined journey projection and proven case-specific module states, triggers, and operation
> outcomes, but never workflow services, cross-module lifecycle ownership, or DI dispatch.

## How to continue this roadmap

The selected item is blocked behind the lifecycle ownership implementation. Continue that actionable
owner directly:

```text
/resume-plan @plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md
```

The B2B owner will open and surface the blocked .NET 11 ledger. Do not resume that blocked ledger
directly, and do not start another .NET 11 service slice until this one establishes the package, CI,
hosting, and toolchain conventions.

## Status

### Prerequisites — existing owners, do not duplicate

- [x] ✅ **ReUnion integration and Payment carrier cutover.** PR #453 and platform-sync PR #463 are
  merged; B2B now owns its remaining Reunion migration directly.
- [x] ✅ **B2B typed-result migration.** Landed in PR #552. It no longer owns this roadmap's return path.
- [ ] 🟠 **Application, Booking, and Concert ownership.** Owned by
  [`../launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md`](../launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md).
  It deletes the cross-stage workflow model this roadmap previously intended to convert.

### Selected

- [ ] 🟡 **B2B .NET 11 runtime and native value unions.** `dotnet-11/b2b-workflow-unions` Design and operational state:
  [`B2B_WORKFLOW_UNIONS_PLAN.md`](B2B_WORKFLOW_UNIONS_PLAN.md) and
  [`B2B_WORKFLOW_UNIONS_PROGRESS.md`](B2B_WORKFLOW_UNIONS_PROGRESS.md). Upgrade the B2B runtime and
  reverse build/test closure while keeping published cross-service contracts net10-compatible. Add
  the journey-stage union and the case-specific module state, trigger, and operation-outcome unions justified after the
  lifecycle split; never union concrete DI step implementations.

### Blocked follow-up

- [ ] 🔴 **.NET 11 GA and Azure Functions deployment readiness.** Open its own plan after the selected
  slice lands. Replace the exact preview/RC SDK pin with the released SDK, take the final language/API
  changes directly, verify the B2B Workers hosting matrix, run the full delivery gates, and remove the
  preview deployment restriction. This item is blocked on both the .NET 11 GA release and Azure
  Functions isolated-worker support for net11.

The repository-wide native conversion of Concertable-owned Dunet error unions remains a separate item
in the typed-result roadmap. This roadmap does not pull that broader cutover into the B2B workflow
slice.

## Dependency map

```text
ReUnion integration + B2B typed-result delivery
└── Application → Booking → Concert ownership delivery
    └── B2B .NET 11 platform-only checkpoint
        └── native closed-value unions
            └── merge-queue full E2E + platform sync

.NET 11 GA + Azure Functions net11 support
└── B2B GA/deployment-readiness follow-up
```

## Adoption rules

- Native unions stay internal to the owning service. They never appear in integration events,
  protobuf messages, persistence models, HTTP contracts, or published `*.Contracts` packages.
- A net11 runtime may consume net10 libraries. A net10 service must never be forced to consume a
  net11-only B2B contract package.
- Published B2B contracts remain net10 unless a later cross-service cutover explicitly multi-targets
  and verifies every consumer. Native workflow unions provide no reason to change those contracts.
- Preview source changes are absorbed at the small set of union declarations and exhaustive dispatch
  sites. Do not create compatibility interfaces, conditional shims, or parallel workflow models.
- Pin an exact SDK in `global.json`; never float a preview channel. Every SDK bump is a reviewed,
  independently green commit or PR.
- The root SDK pin changes the repository build toolchain even when only B2B targets net11. Therefore
  the full solution and all affected CI jobs are part of the gate.
- B2B Workers must not be deployed to Azure Functions until the hosted isolated-worker support matrix
  includes the selected net11 release. Local/Aspire execution does not prove hosted support.
- Full E2E is mandatory in the merge queue for runtime, TFM, CI, and workflow-dispatch changes. Local
  work runs targeted TFM/architecture checks and affected builds/tests; draft-PR CI owns the full
  build, carve, unit, and integration gate.

## Epic definition of done

- The B2B runtime and every direct reverse build/test consumer compile on a supported .NET 11 SDK.
- Customer, Search, Payment, and Auth remain independently buildable on net10 against published B2B
  contract packages.
- Application, Booking, and Concert retain independent state machines and module-local step resolvers;
  no native union contains or dispatches service implementations.
- Native unions model the combined journey projection and proven case-specific module states,
  triggers, and operation outcomes with exhaustive coverage; they never contain runtime services.
- The B2B preview slice and its generated platform-sync PR are merged with full merge-queue E2E green.
- The GA follow-up is merged, B2B Workers are supported by the target host, and the preview deployment
  restriction is gone.
