# .NET 11 adoption roadmap

> **Goal:** adopt .NET 11 where it unlocks materially better domain modelling, without forcing
> independently deployed net10 services or published cross-service contracts onto a preview runtime.
> Each implementation item owns a plan and progress ledger in this folder.
>
> **Current decision:** the first adoption slice is the B2B runtime plus its Concert workflow model.
> ReUnion remains the Result/Option carrier; native C# unions model closed workflow choices.

## How to continue this roadmap

The selected item is blocked behind B2B, which is itself blocked behind the ReUnion integration.
Continue the actionable resolver at the head of that chain:

```text
/resume-plan @plans/typed-result/REUNION_INTEGRATION_PROGRESS.md
```

The ReUnion owner will open and surface B2B; B2B will then open and surface the blocked .NET 11 ledger.
Do not resume either blocked ledger directly, and do not start another .NET 11 service slice until this
one establishes the package, CI, hosting, and toolchain conventions.

## Status

### Prerequisites — existing owners, do not duplicate

- [ ] 🟠 **ReUnion integration and carrier cutover.** Owned by
  [`../typed-result/REUNION_INTEGRATION_PROGRESS.md`](../typed-result/REUNION_INTEGRATION_PROGRESS.md).
  It must publish and synchronize the Reunion-backed Result/Option baseline before the existing B2B
  migration resumes.
- [ ] 🟠 **B2B typed-result migration.** Owned by
  [`../typed-result/B2B_PROGRESS.md`](../typed-result/B2B_PROGRESS.md). Its remaining Concert
  payment/cancel/finish work overlaps the workflow surface in this roadmap, so it must finish and land
  before the .NET 11 worktree is created.

### Selected

- [ ] 🟡 **B2B .NET 11 runtime and native workflow unions.** Design and operational state:
  [`B2B_WORKFLOW_UNIONS_PLAN.md`](B2B_WORKFLOW_UNIONS_PLAN.md) and
  [`B2B_WORKFLOW_UNIONS_PROGRESS.md`](B2B_WORKFLOW_UNIONS_PROGRESS.md). Upgrade the B2B runtime and
  reverse build/test closure while keeping published cross-service contracts net10-compatible, then
  replace the Concert workflow's interface-as-sum-type design with closed unions over concrete steps.

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
ReUnion integration + generated platform sync
└── existing B2B typed-result checkpoints 6-7 + delivery gates
    └── B2B .NET 11 platform-only checkpoint
        └── native Concert workflow unions
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
- Full E2E is mandatory in the merge queue for runtime, TFM, CI, and workflow-dispatch changes. The
  local gate remains build, unit, architecture, and integration tests.

## Epic definition of done

- The B2B runtime and every direct reverse build/test consumer compile on a supported .NET 11 SDK.
- Customer, Search, Payment, and Auth remain independently buildable on net10 against published B2B
  contract packages.
- Concert workflow variation is expressed by native unions over concrete closed implementations, not
  interface inheritance, nullable all-purpose method parameters, reflection marker interfaces, or
  `DealType` branching in agnostic executors.
- The B2B preview slice and its generated platform-sync PR are merged with full merge-queue E2E green.
- The GA follow-up is merged, B2B Workers are supported by the target host, and the preview deployment
  restriction is gone.
