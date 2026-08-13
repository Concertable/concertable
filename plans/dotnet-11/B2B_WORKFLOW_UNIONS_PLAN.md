# B2B .NET 11 runtime and native workflow unions

> **Next steps live in @plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md → `## Next Steps`.**

## Outcome

Move the B2B runtime and its reverse build/test closure to .NET 11, then replace the Concert
workflow's interface-as-sum-type hierarchy with native unions whose cases are the concrete closed step
implementations. Preserve the single Apply and Accept operations, keyed `DealType` workflow selection,
cross-service package compatibility, and all current payment and lifecycle behaviour.

This is worth doing before a firm release date because the workflow model becomes simpler immediately,
the union surface is internal and therefore cheap to adjust as the preview evolves, and the project has
time to absorb SDK churn. It is not risk-free: preview .NET is unsupported, native unions remain an
active language design, and Azure Functions does not currently host net11 isolated workers. The plan
contains those risks rather than pretending the E2E suite eliminates them.

## Dependency and ownership gate

Do not create `Refactor/dotnet-11_b2b-workflow-unions` until
[`../typed-result/B2B_PROGRESS.md`](../typed-result/B2B_PROGRESS.md) records all of the following:

1. the ReUnion integration owner's Phase 4 generated platform-sync PR is merged;
2. B2B typed-result checkpoints 6-7 are complete on the authoritative B2B worktree;
3. the B2B source PR is merged; and
4. every resulting package publication/platform-sync gate is terminal and green.

That owner controls the overlapping Concert payment/cancel/finish code and will update this plan's
ledger and surface its resume prompt when the gate opens. Do not rebase, copy, or recreate its unpushed
work.

## Evidence snapshot — 2026-08-09

- Concertable currently has no `global.json`; the available local SDK is `10.0.302` and backend
  projects target `net10.0` individually.
- The current .NET 11 release is Preview 6. General availability is scheduled for 2026-11-10.
- Microsoft's support policy does not support preview releases; a go-live RC is supported only when
  explicitly designated. Revalidate this at execution and delivery time.
- C# native unions are still an active proposal. Preview 6 includes the language shape plus runtime
  support types, `System.Text.Json`, and ASP.NET integration, but source-level details may still move.
- Azure Functions' isolated-worker matrix currently tops out at .NET 10 and states that Preview/Go-live
  releases are not currently available in the hosted service. `Concertable.B2B.Workers` directly
  references B2B runtime infrastructure, so a net11-only workflow makes that worker net11 too.
- Published B2B Artist, Concert, Venue, User, Tenant, and Seed contracts are consumed by net10 Customer,
  Search, and Payment projects. Those packages cannot become net11-only as part of this work.
- The workflow currently models closed alternatives using `IApplies*`, `IAccepts*`, and step-interface
  inheritance. `ConcertWorkflowBuilder` constrains registration through `IConcertStep`, and the
  capability registry infers checkout support from marker interfaces.
- Existing E2E coverage is valuable regression evidence, not a proof of correctness. Add focused
  dispatch and missing-input tests rather than relying only on the roughly 30 B2B UI scenarios.

Revalidation sources:

- [.NET 11 releases](https://github.com/dotnet/core/blob/main/release-notes/11.0/README.md)
- [.NET support policy](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
- [C# unions proposal](https://github.com/dotnet/csharplang/blob/main/proposals/unions.md)
- [Azure Functions isolated-worker supported versions](https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide#supported-versions)

## Target-framework boundary

“B2B on .NET 11” means the deployable B2B runtime plus every project that must compile against that
runtime. It deliberately does not mean every project under the `Concertable.B2B` directory.

### Move to net11

- B2B Web, Workers, AppHost, AppHost.Extensions, non-contract module Domain/Application/Api/
  Infrastructure projects, B2B data-access/runtime infrastructure, and their unit/integration/
  architecture/E2E projects;
- `Concertable.AppHost`, because it references the B2B AppHost;
- shared/root E2E projects and any other direct reverse project-reference consumer discovered by the
  execution-time graph audit; and
- build/CI tooling that selects the SDK or assumes a B2B `net10.0` output directory.

### Remain net10-compatible

- every published `Concertable.B2B.*.Contracts` and `Concertable.B2B.Seed.Contracts` project;
- `Concertable.B2B.Seed.Simulator` while net10 Customer/Search AppHosts reference it directly;
- Customer, Search, Payment, Auth, and their AppHosts/tests; and
- Shared packages that do not require a net11 API.

A net11 project can reference these net10 assemblies. If the graph audit finds a net10 project that
must reference a net11 runtime project, either move that reverse consumer to net11 or preserve a clean
net10 boundary by extracting the dependency. Never retarget another service merely to silence a
compatibility error.

## Target workflow architecture

### Closed choices are unions over concrete cases

The final shape has no `IAcceptStep`, `IApplyStep`, `ISimpleAcceptStep`, `IPaidAcceptStep`, or interface
case hidden inside a union. The representative declarations are:

```csharp
internal union AcceptStep(
    CaptureEscrowAcceptStep,
    DepositEscrowAcceptStep,
    PaidAcceptStep);

internal union ApplyStep(
    SimpleApplyStep,
    PaidApplyStep);
```

Apply the same rule to other genuine closed variations after the execution-time inventory:

- `AcceptCheckoutStep(HoldCheckoutStep, VerifyCheckoutStep)`;
- `FinishStep(ReleaseEscrowFinishStep, PayoutFinishStep)`; and
- any additional family that has two or more concrete, closed implementations at the reconciled head.

Use the concrete class directly for a one-implementation operation such as `SetupCheckoutStep`,
`CreateConcertDraftStep`, or `RefundEscrowStep`. Do not wrap a single case in a union and do not retain a
marker interface merely to satisfy a generic constraint.

### Workflow composition is concrete and Infrastructure-owned

The concrete step cases live in Concert Infrastructure, so their unions and workflow composition also
belong there. Application must not reference Infrastructure to expose them. Preserve Application-owned
operation ports such as `IApplyExecutor` and `IAcceptExecutor`; they are real dependency-inversion
boundaries, not attempts to model a closed sum type.

Replace the internal `IConcertWorkflow`/capability inheritance graph with one concrete workflow
composition returned by the keyed Infrastructure factory. It contains:

- required `ApplyStep`, `AcceptStep`, book, finish, and cancel operations;
- explicit optional checkout operations using the already-integrated Reunion `Option<T>`; and
- `DealType` plus lifecycle transitions/capability metadata configured at keyed registration time.

The builder registers concrete steps explicitly and emits the concrete workflow configuration. Delete
`IConcertStep` and step-family generic constraints rather than weakening them to unconstrained
`where TStep : class`. Replace reflection over `IAppliesCheckout`/`IAcceptsCheckout` with explicit
immutable capability metadata produced by the builder. Agnostic executors and response mappers do not
branch on `DealType`.

### Dispatch remains exhaustive and validates operation-specific input

Keep the single Accept endpoint. “Accept an application” is one lifecycle operation; the selected
workflow decides which implementation performs it. The executor both dispatches the closed step and
validates the input required by the selected case:

```csharp
await (workflow.Accept switch
{
    CaptureEscrowAcceptStep step =>
        step.ExecuteAsync(app),

    DepositEscrowAcceptStep step =>
        step.ExecuteAsync(app),

    PaidAcceptStep step when paymentMethodId is not null =>
        step.ExecuteAsync(app, paymentMethodId),

    PaidAcceptStep =>
        throw new BadRequestException(
            "This deal requires a payment method at acceptance")
});
```

The union is exhaustive over step cases. The unguarded `PaidAcceptStep` arm is still required because
the guard partitions that case by request state; it is the correct place to reject a missing payment
method. Apply follows the same pattern and has its own operation-specific error. There is no nullable
payment parameter on a common step abstraction and no wildcard “workflow does not support operation”
arm for operations required by every configured workflow.

## Implementation phases

Each phase ends green and is committed immediately. Use one implementation branch and one PR; no
publication or external dependency requires a merge between the platform and union checkpoints.

### Phase 0 — refresh volatile evidence and freeze the graph

1. Create `Refactor/dotnet-11_b2b-workflow-unions` from fresh `origin/main` only after the dependency
   owner opens the gate.
2. Re-read the current .NET 11 release notes, support policy, union proposal/compiler diagnostics,
   Azure Functions matrix, and Reunion package target frameworks. Record any source-shape change in the
   progress ledger before editing code.
3. Generate the complete forward and reverse project-reference graph from the reconciled head. Mark
   each project net11, net10-contract boundary, or unaffected; include non-B2B consumers such as root
   AppHost/E2E projects.
4. Inventory SDK pins, `actions/setup-dotnet` inputs, Dockerfiles, deployment manifests, test scripts,
   and hard-coded `bin/.../net10.0` paths.
5. Confirm the B2B Workers hosted-deployment restriction still applies. This does not block local/CI
   implementation, but it blocks deploying that worker until a later GA/deployment-readiness effort
   verifies hosted support.

Gate: evidence and exact project matrix are recorded in the ledger; no runtime edit yet.

### Phase 1 — platform-only .NET 11 checkpoint

1. Add a repository-root `global.json` pinned to the exact selected .NET 11 SDK with prerelease policy
   explicit. At the current snapshot that means Preview 6; at execution time use the newest exact .NET
   11 release allowed by the revalidated decision rule, preferring GA, then a go-live RC, then preview.
2. Retarget only the B2B runtime/reverse-consumer matrix to `net11.0`. Keep published cross-service
   contracts and the shared seed simulator net10-compatible.
3. Update CI to install the exact pinned SDK. Replace B2B hard-coded framework output paths with a
   value derived from the project/MSBuild output so the GA bump does not require path archaeology.
4. Update Docker/runtime base images and deployment build inputs only where the B2B net11 closure
   requires them. Do not claim Azure Functions hosted deployability while its matrix says otherwise.
5. Restore and build before making any workflow or union source change. Resolve real package/tooling
   incompatibilities directly; do not multi-target runtime projects or add compatibility shims merely
   to make the preview compile.

Gate:

- `dotnet --info` proves the exact SDK selected through `global.json`;
- B2B solution Release build, full `api/Concertable.slnx` Release build, architecture tests, and all
  affected unit/integration suites are green;
- net10 Customer/Search/Payment/Auth standalone builds still consume net10-compatible B2B contracts;
- package inspection shows published B2B contract assets still include net10; and
- the diff contains no `union` declaration or workflow behaviour change.

Commit the verified platform-only checkpoint. If it cannot pass without breaking a service boundary or
requiring an unsupported hosted deployment, record the evidence and discard the disposable branch;
do not start Phase 2.

### Phase 2 — coherent native workflow-union cutover

1. Add the native concrete-case unions for every multi-implementation workflow-step family identified
   above; use concrete classes for single-implementation families. Enable preview language features
   only in the project that declares the unions rather than changing the repository-wide language
   version.
2. Move workflow composition and its factory/builder internals into Infrastructure. Replace the four
   interface-heavy workflow shapes with the concrete keyed configuration while preserving the keyed
   resolver pattern and lifecycle transition tables.
3. Replace apply, accept, checkout, finish, cancel, book, and application-cancel step-interface
   properties with union, concrete, or explicit optional values as appropriate.
4. Rewrite dispatchers/executors as exhaustive switches. Retain guarded paid arms plus unguarded
   missing-input failures. Preserve expected typed Results from the completed B2B migration and let
   infrastructure/cancellation/invariant failures remain exceptions.
5. Replace capability-marker reflection with explicit builder-produced metadata used by response
   mappers and checkout dispatch. No agnostic `DealType` switch and no service location.
6. Delete the obsolete `IApplies*`, `IAccepts*`, `IConcertStep`, and step-family interfaces, plus dead
   registrations/usings. Do not leave a hybrid interface/union abstraction at the phase boundary.

Gate: builds are green and repository scans find none of the deleted step/capability interface names or
an equivalent nullable all-purpose step API.

### Phase 3 — behaviour and exhaustiveness coverage

Add focused tests before relying on the broad suites:

- every `ApplyStep`, `AcceptStep`, `AcceptCheckoutStep`, and `FinishStep` case dispatches to its exact
  concrete implementation;
- paid Apply and Accept succeed with a payment method and reject a missing one with the correct
  operation-specific error;
- simple Apply/Accept paths do not require or consume a payment method;
- every configured `DealType` exposes the same apply/accept/checkout/finish/cancel capability set and
  lifecycle transitions as before;
- adding a new union case makes the relevant switch non-exhaustive at compile time;
- no controller or endpoint branches on deal type; and
- existing step tests continue to pin money movement, idempotency, transition, signature, contract,
  and background-task behaviour.

Run the B2B architecture, Concert unit, and Concert integration suites through the repository's normal
debug workflows. Fix failures at the owning layer; never weaken assertions, add retries, or skip a
scenario.

### Phase 4 — final verification, review, and delivery

1. Merge current `origin/main` while the tree is clean, then re-run the SDK/support/proposal snapshot.
   If a newer preview/RC/GA is available, update the exact pin in its own green commit before review.
2. Run the B2B Release build and focused affected unit and architecture tests locally.
3. Push the coherent checkpoint. Exact-head PR CI owns the full solution build, standalone carves,
   package compatibility, and complete unit/integration matrices proving net10 consumers remain
   independent.
4. Run a full code review of `origin/main...HEAD`, fix every high-confidence finding, and re-run affected
   gates.
5. Open one PR and require full merge-queue E2E. This change is not eligible for `skip-e2e` or
   `skip-e2e-ui` because it changes the SDK/runtime, CI paths, workflow composition, and payment-related
   dispatch.
6. Follow the source PR through merge, package publication, and the generated platform-sync PR. Fix any
   consumer compilation issue in the sync PR and do not close the plan until the sync is green/merged.

Do not run local E2E before the PR. If the queue fails, enter the matching E2E debug skill on a fresh,
healthy Docker stack and drive the genuine failure to green.

## Preview-change strategy

- Keep union declarations and pattern matches internal and concentrated in the Concert Infrastructure
  workflow folder. No public compatibility promise depends on preview syntax.
- Prefer direct adoption of the current compiler shape over wrappers. If a later preview changes syntax
  or runtime APIs, change the declarations and switches in one focused commit.
- Never encode native unions into stored or wire data. Serialization support is therefore incidental,
  not a migration dependency.
- Treat compiler exhaustiveness warnings as build failures in the affected projects so a new case
  cannot silently bypass dispatch.
- Final GA SDK pinning, Azure Functions host proof, and removal of the deployment restriction are
  outside this preview slice; they do not require repeating the workflow design.

## Definition of done

- Platform-only and workflow-cutover checkpoints are separately committed and independently green.
- The B2B runtime/reverse closure targets net11 while published B2B contracts and external services
  remain net10-compatible.
- No workflow step or capability interface is being used as a substitute for a closed union.
- Apply/Accept use concrete-case exhaustive switches, including guarded paid-input validation.
- The existing endpoint and domain behaviour are preserved with focused tests plus the full merge-queue
  E2E gate.
- The source PR, publication, and generated platform-sync PR are terminal and green.
