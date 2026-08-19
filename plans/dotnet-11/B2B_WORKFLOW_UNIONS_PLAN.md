# B2B .NET 11 runtime and native value unions

> **Next steps live in @plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md → `## Next Steps`.**

## Outcome

Adopt .NET 11 for the B2B runtime and its reverse build/test closure after the Application, Booking,
and Concert ownership refactor has landed, then use native unions for the resulting closed internal
value models. Preserve net10-compatible published Contracts and independent Customer, Search,
Payment, and Auth builds.

The former plan to replace `IConcertWorkflow` and its step interfaces with unions over concrete DI
implementations is rejected. The approved lifecycle design deletes that cross-stage workflow and gives
each module contextual operations. Native unions model small closed internal values, not services,
lifetimes, keyed dependency resolution, or the published Deal payload.

`@plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md` owns the separate Deal decision: a closed record hierarchy,
native unions for heterogeneous internal operation values, and generated invariant module factories for
the terms, mapper, and updater families that already share one honest interface. Executors and steps do
not use that common-interface factory. This plan must establish
the native-union runtime/toolchain and whether a C# 15-compiled `Deal.Contracts` can retain net10 assets,
including which consumers require a compiler/runtime retarget. It does not perform that breaking
published-package cut-over or reclassify the Deal operations itself.

## Dependency gate

Do not create the implementation worktree until
`plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` records the module/state refactor and its complete
delivery lifecycle as terminal. The lifecycle owner will reconcile this plan against the landed
module APIs and surface its pointer.

The earlier B2B typed-result dependency is satisfied by merged PR #552. It no longer owns the return
path for this work.

## Target-framework boundary

### Move to net11

- B2B Web, Workers, AppHost, AppHost.Extensions, non-contract module runtime projects, B2B data-access
  infrastructure, and their direct unit/integration/architecture/E2E reverse consumers;
- the root AppHost and shared E2E composition projects where direct project references require it;
- build/CI tooling, Docker/runtime inputs, and hard-coded target-framework paths in that closure.

### Remain net10-compatible

- every published `Concertable.B2B.*.Contracts` and `Concertable.B2B.Seed.Contracts` project;
- `Concertable.B2B.Seed.Simulator` while net10 consumers reference it;
- Customer, Search, Payment, Auth, and their runtime/test closures;
- Shared packages that do not require a net11 API.

A net11 project may consume these net10 assemblies. Never retarget another service merely to silence a
compatibility error.

This is the boundary for this runtime PR, not a permanent prohibition. The downstream Deal plan may
change `Deal.Contracts` only through its explicit package/consumer cut-over after proving the supported
target and compiler matrix.

## Native union boundary

After the lifecycle refactor lands, inventory its actual closed value alternatives. The first required
candidate is the read-only combined journey shape:

```text
ApplicationStage | BookingStage | ConcertStage
```

It gives API/HATEOAS projections exhaustive handling of all current stages. It remains a derived read
value with no repository, transition method, or command authority. Module-local state, trigger, and
operation-outcome shapes with case-specific data are the second candidate and must use native unions
where the inventory proves the alternatives are genuinely closed. Persistence retains an explicitly
mapped module-owned discriminator rather than serialising the in-process union as a cross-boundary
contract.

Do not use native unions for:

- step implementations or any DI-resolved service;
- database columns or persistence DTOs in place of an explicit module-owned discriminator;
- a replacement workflow/process aggregate;
- integration events, protobuf, persistence, HTTP contracts, or published packages;
- recreating a cross-module capability registry or exhaustive switch over `DealType` in agnostic code;
- replacing a genuine same-interface terms, mapper, or updater family with a union merely because the
  Deal representation is closed.

The published Deal sum uses a closed hierarchy instead of a native union because it has genuine shared
members, ordinary reference identity, TPT/class-polymorphism alignment, and an established tagged
`$type` JSON contract. Heterogeneous Deal-operation values use native unions. The proven uniform
terms, mapper, and updater operations remain named calls selected through generated invariant module
factories; their selection switches and registrations are never repeated in application source.

Do not manufacture unions for types that are already honestly represented by an enum or ordinary
value object. The selected .NET 11 direction requires native unions for the closed journey projection
and any proven case-specific module states, triggers, or operation outcomes; it does not require every
state or result to become a union.

## Evidence to refresh at execution

- exact .NET 11 SDK/release and support status;
- final C# union syntax, exhaustiveness diagnostics, runtime support, and serializer behaviour;
- final C# closed-hierarchy metadata/consumer behavior, including whether a net10-targeted contract
  compiled by the .NET 11 SDK preserves exhaustiveness for supported consumers;
- Azure Functions isolated-worker support for the selected net11 release;
- Reunion target frameworks and whether any internal result union still warrants migration;
- the complete B2B forward/reverse project-reference graph after module extraction;
- SDK pins, CI setup, Dockerfiles, deployment inputs, and hard-coded output paths.

## Phases

### Phase 0 — refresh the landed graph

1. Create `Refactor/dotnet-11_b2b-runtime` from fresh `origin/main` after the lifecycle gate opens.
2. Re-read official release/support/Functions/union guidance.
3. Generate the exact target-framework and reverse-consumer matrix.
4. Inventory every SDK, CI, container, deployment, and output-path assumption.
5. Record the concrete journey-stage union and inventory case-specific module state, trigger, and
   operation-outcome shapes, with their before/after types and consumers.

Gate: the ledger contains current official evidence, the exact project matrix, the journey-stage
union design, and every accepted or rejected operation-outcome candidate. No runtime edit yet.

### Phase 1 — platform-only runtime checkpoint

1. Pin the exact supported .NET 11 SDK in `global.json` with prerelease policy only if still required.
2. Retarget only the B2B runtime/reverse-consumer matrix.
3. Update CI, Docker/runtime inputs, and derived output paths.
4. Keep all published B2B Contracts and other services net10-compatible.
5. Prove the platform checkpoint before any optional language-model change.

Gate: B2B and full API builds are green, external services remain independently buildable, and package
inspection proves published B2B contracts still carry net10 assets.

### Phase 2 — native value-union checkpoint

1. Add the internal journey-stage native union at the owning read boundary.
2. Use exhaustive matching and focused tests for every case.
3. Add only the case-specific module state, trigger, and operation-outcome unions accepted by Phase 0.
4. Keep module-owned state machines and aggregate ownership unchanged. Do not preserve a provisional
   Deal-keyed executor/step resolver merely because it exists in the landed layout, and do not convert
   the downstream Deal plan's same-interface terms, mapper, or updater families into unions.
5. Delete superseded value abstractions in the same checkpoint; do not retain parallel models.

Gate: the union remains internal, contains values rather than services, and no module/runtime dependency
direction changes.

### Phase 3 — verification and delivery

1. Reconcile current `origin/main` and repeat the volatile SDK/support/Functions evidence.
2. Run focused local builds and tests under repository validation policy.
3. Push the coherent checkpoint; exact-head CI owns the full solution, service carves, packages, and
   complete unit/integration matrices.
4. Review the complete diff and require full merge-queue E2E for the runtime/TFM change.
5. Follow source merge, publication, generated platform sync, and deployment-readiness gates to
   terminal green.

## Definition of done

- B2B runtime and every required reverse consumer compile on a supported .NET 11 SDK.
- Published B2B Contracts remain net10-compatible and other services build independently.
- No union contains or dispatches a DI service implementation.
- Application, Booking, and Concert retain independent state machines and contextual operations.
- Native unions model the internal combined journey projection and every proven case-specific module
  state, trigger, or operation outcome with exhaustive coverage; no union contains runtime services.
- Azure Functions deployment support is proven before claiming the Workers deployable.
- The native-union/runtime and compiler/target matrix needed by the downstream Deal operation and
  representation cut-over is recorded and its progress ledger is updated when that gate opens.
- Source PR, package publication, platform sync, and required E2E are terminal green.
