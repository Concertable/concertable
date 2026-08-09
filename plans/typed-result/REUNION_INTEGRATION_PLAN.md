# Reunion integration plan

> **Next steps live in @plans/typed-result/REUNION_INTEGRATION_PROGRESS.md → `## Next Steps`.**

## Objective

Replace Concertable's temporary owned Result/Option carriers with the real Reunion package family,
using reviewed carrier base `7bf5f66` through corrected merged release head `e33b40f`, without changing domain
error ownership, controller signatures, MVC behavior, published transport contracts, or service
package boundaries.

This is a publish-gated cross-package cutover. It is not a request to redesign Reunion, remove a
Result family, retain permanent compatibility shims, or convert MVC controllers to Minimal API
results. Temporary additive overloads and the owned carriers remain only while the published
package graph advances one compatible layer at a time.

## Non-negotiable boundaries

- Keep all five Reunion functional types: `Result`, `Result<T>`, `Result<T,TError>`,
  `UnitResult<TError>`, and `Option<T>`.
- `Reunion` remains dependency-free. Concertable's `IError`, `ErrorDefinition`, error cases, status
  policy, and ProblemDetails customization remain Concertable-owned.
- Only `Concertable.Shared.Api` directly references `Reunion.AspNetCore`. Domain and application
  projects never acquire that package or a new ASP.NET Core dependency.
- Keep conventional MVC `ActionResult`/`ActionResult<T>` controller signatures and observable
  responses. Do not import `Reunion.AspNetCore.HttpResults` or migrate endpoints to Minimal API
  `Results<T1,T2>` in this work.
- No permanent machine-specific `ProjectReference`, absolute feed path, local package version, or
  temporary NuGet configuration may merge.
- Existing B2B and Auth migrations contain authoritative unpushed work. Preserve those local owners
  and do not infer their state solely from GitHub.
- Preserve the existing `Refactor/typed-result_http-terminals` semantic-terminal work as local input
  to the final Shared contraction. It must not publish a competing Shared.Api package or generated sync.

## Current-state inventory (2026-08-09)

### Repository and carrier state

- Concertable `origin/main` was `dc0da9360370e188b27f8e8bda775beac7c65de5` when Phase 1 began;
  the verified checkpoint is reconciled with current `origin/main` `162b8412a1941c765f2c2b2c5c1db5b7f0549928`.
- `Concertable.Kernel.Functional` currently defines all five temporary carriers plus synchronous,
  task-aware, collection, and Option conversion extensions. Its behavior intentionally already
  resembles Reunion: non-null payloads, non-empty string failures, uninitialized default Results,
  `default(Option<T>) == None`, `Match`/`TryGet*`, equality, and lazy task-aware composition.
- `Concertable.Kernel.Errors` owns `IError`, `ErrorDefinition`, `ValidationErrorDefinition`,
  `ErrorKind`, stable code/message derivation, and validation invariants. These do not move to Reunion.
- `Concertable.Shared.Api.Results` owns MVC terminal mapping. It currently returns
  `ActionResult<T>`, `IActionResult`, `OkObjectResult`, `CreatedAtActionResult`, and
  `NoContentResult` and executes failures through `ApplicationErrorResult`.
- `ApplicationProblemDetails.WriteAsync` supplies the request `instance`, `traceId`, configured
  `IProblemDetailsService` customization/writers, and the JSON fallback. That execution path is part
  of the HTTP contract.
- All Concertable production projects currently target net10.0. Reunion's net10 asset is therefore
  the initial runtime asset; net11 native-union behavior remains a forward-compatibility gate.

### Local worktree state

The active typed-result worktrees were reconciled read-only after the docs merge. Their exact
operational state lives in the owning ledgers; the dependency-relevant snapshot is:

| Worktree branch | State at audit |
|---|---|
| `Refactor/B2BTypedResultMigration` | Clean at `ba5791268`; no PR/remote; 198 behind / 25 ahead. |
| `Feature/typed-result_auth-outcomes` | Clean at `98599413a`; no PR/remote; 286 behind / 27 ahead. |
| `Feature/typed-result_customer-outcomes` | Clean at `e7c44f5b3`; PR #425 at `e60219f7d`; 185 behind / 31 ahead. |
| `Feature/TypedResultMigrationPhase2` | Clean at `b6a671ef9`; PR #282 at `26ed63b896`; 548 behind / 29 ahead. |
| `Refactor/typed-result_http-terminals` | Clean at `fecd46c11`; verified code/test checkpoint `c593150e4`; 168 behind / 4 ahead. |

### Package boundaries

| Concertable project/category | Direct package | Reason |
|---|---|---|
| `api/Concertable.Shared/src/Concertable.Kernel/Concertable.Kernel.csproj` | `Reunion` | Owns the shared in-process carrier vocabulary and publishes APIs containing those types. |
| `api/Concertable.Shared/src/Concertable.Shared.Api/Concertable.Shared.Api.csproj` | `Reunion.AspNetCore` | Owns MVC/ProblemDetails boundary behavior; the package brings matching `Reunion` and `Reunion.Errors` dependencies. |
| Kernel and Shared.Api unit/architecture tests | Neither directly | Consume through their tested project references unless a clean package-consumer test deliberately verifies NuGet assets. |
| Service Web/API projects and controllers | Neither directly | Consume the Concertable-owned Shared.Api boundary; do not spread adapter-package ownership across services. |
| `api/Concertable.Payment/src/Concertable.Payment.Client/Concertable.Payment.Client.csproj` | Neither during expansion; `Reunion` after its own migration | This published package re-exposes Kernel Result/Option types and is therefore a second publication layer, not an ordinary final consumer. |
| Domain, application, contracts, other clients, infrastructure, messaging, worker, seed, and AppHost projects | Neither directly | Receive Reunion transitively where a published Concertable API exposes it. Never reference `Reunion.AspNetCore`. |

`Concertable.Kernel` already has a pre-existing `Microsoft.AspNetCore.App` framework reference for
unrelated legacy reasons. This migration must not use that fact to add `Reunion.AspNetCore` or new HTTP
policy to Kernel; removing the legacy framework reference is separate work.

## MVC compatibility decision

Concertable can keep its controller signatures while consuming the MVC adapter namespace, but direct
replacement is safe only where Reunion exactly matches existing behavior:

- `Option<T>.ToOkOrNotFound()` and `Option<T>.ToOkOrNoContent()` produce the required MVC
  `ActionResult<T>` and exact 200/404 or 200/204 cases.
- Reunion's generic-error overloads accept a caller-supplied `Func<TError, ProblemDetails>`, so the
  carrier is compatible with Concertable's error hierarchy without Reunion knowing `IError`.
- Direct generic-error use is not initially behavior-equivalent: Reunion returns an ordinary
  `ObjectResult`, while Concertable's `ApplicationErrorResult` executes through
  `IProblemDetailsService`, supplies request instance and trace ID, respects customizers/writers, and
  preserves its JSON fallback.
- Reunion `ToCreatedOrProblem` creates `CreatedResult` from a literal Location string. Concertable's
  `ToCreatedAtActionResult` uses MVC route generation through `CreatedAtActionResult`. Those have
  observably different Location behavior and are not interchangeable.

Therefore the first cutover keeps a thin Concertable Shared.Api terminal over Reunion carriers. It may
delegate exact Option success/absence cases to `Reunion.AspNetCore.Mvc`, but typed failures and
CreatedAtAction stay Concertable-owned until separately proven equivalent. No controller signature or
response contract changes are intended.

The semantic terminal vocabulary remains explicit: Result terminals name success plus ProblemDetails
(`ToOkOrProblem`, `ToCreatedOrProblem`, `ToCreatedAtOrProblem`, `ToAcceptedOrProblem`, and
`ToNoContentOrProblem`); Option terminals name Some plus ordinary absence (`ToOkOrNotFound` and
`ToOkOrNoContent`). Option does not encode unauthenticated/forbidden/conflict outcomes. Those remain
operation-owned typed Results. The existing local HTTP-terminal implementation is completed and
reviewed as a local checkpoint, then incorporated into the final consumer contraction in Phase 5.

## Old-to-new API mapping

| Existing Concertable API | Reunion target | Migration decision |
|---|---|---|
| `Concertable.Kernel.Functional.Result` | `Reunion.Result` | Replace carrier and factories; keep uninitialized/default tests. |
| `Result<TValue>` | `Reunion.Result<TValue>` | Replace one-for-one; string failures remain non-null/non-blank. |
| `Result<TValue,TError>` | `Reunion.Result<TValue,TError>` | Replace one-for-one; retain typed Concertable error unions. |
| `UnitResult<TError>` | `Reunion.UnitResult<TError>` | Replace one-for-one. |
| `Option<T>` | `Reunion.Option<T>` | Replace one-for-one; `default` remains `None`. |
| `Option.OrFailure(error)` | Reunion `OrFailure(error)` | Same name and eager semantics. |
| `Option.OrFailure(() => error)` | Reunion lazy `OrFailure` | Same name; assert factory is called exactly once only for `None`. |
| Task receiver `OrFailure` / async factory `OrFailureAsync` | Reunion task extensions | Same names and shapes; verify null tasks, cancellation, exception identity, and laziness. |
| `Map`/`Bind`, task variants, collection traversal | Reunion combinators | Replace imports and call sites mechanically only after parity tests; do not retain duplicate extensions. |
| Query syntax over Result/Option | Reunion `Select`/`SelectMany` | Supported; compile representative queries on net10 and later net11. |
| `ToOkActionResult` | Concertable terminal over Reunion `Result<T,TError>` | Carrier plumbing becomes redundant; keep Concertable ProblemDetails execution. |
| `ToNoContentActionResult` | Concertable terminal over Reunion `UnitResult<TError>` | Keep 204 and Concertable failure writer behavior. |
| `ToCreatedAtActionResult` | No direct Reunion replacement | Keep application-specific MVC route generation and body behavior. |
| Manual Option `Match` to 200/404 or 200/204 | `Reunion.AspNetCore.Mvc` Option methods | Replace when the exact response contract matches. |
| `ToActionResult` callback core | Reunion `Match` plus Concertable boundary wrapper | Remove only after all application-specific wrappers no longer depend on it. |

The final cleanup removes Concertable's duplicate functional implementation and task/collection
extensions only after repository-wide symbol inventory proves every caller is on Reunion. Application
error and HTTP policy code remains.

### Option-to-Result conversion inventory

- `origin/main` contains no production `OrFailure`, `OrFailureAsync`, or CFE `ToResult` call site.
  The only Reunion-shaped conversions are the owned definitions in `Option.cs` and
  `OptionTaskExtensions.cs` plus their Kernel unit tests.
- PR #425 adds no production Option-to-Result conversion; it carries the same shared definitions and
  tests inherited from its base. Its service work uses Option as an absence boundary without turning
  it into a typed failure.
- PR #282's unique Ticket implementation contains exactly two CFE `Maybe.ToResult` conversions in
  `api/Concertable.Customer/src/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure/Services/TicketService.cs`:
  missing Concert in `PurchaseAsync` becomes `PurchaseError.ConcertNotFound`, and missing Concert in
  `CheckoutAsync` becomes `CheckoutError.ConcertNotFound`. Recreate both with Reunion
  `Option.FromNullable(...).OrFailure(...)`; do not retain CFE or its throwing wrong-case accessors.
- B2B and Auth may contain additional unpushed conversions in their local worktrees. Their exact
  call-site inventory remains a mandatory Phase 1 check before any carrier edit; the heads and dirty
  state are recorded in the ledger.

Reunion's eager, lazy, Task-receiver, and async-factory names match Concertable's current API. The
behavioral contract also matches by design—Some returns success without invoking the error factory,
None creates one failure, and null results/tasks are rejected—but the parity suite remains the removal
gate for Concertable's duplicate extensions.

## Concertable error adapter

Expose a pure web-layer mapper in `Concertable.Shared.Api`, not Reunion:

```csharp
public static ProblemDetails ToProblemDetails(this IError error)
```

The mapper validates `error` and `error.Definition`, then preserves the existing policy exactly:

| `ErrorKind` | HTTP status |
|---|---:|
| `Invalid` | 400 |
| `NotFound` | 404 |
| `Conflict` | 409 |
| `Unauthenticated` | 401 |
| `Forbidden` | 403 |
| `PaymentRequired` | 402 |

It sets `Status`, the status reason phrase as `Title`, `ErrorDefinition.Message` as `Detail`, and the
stable definition code in the `code` extension. A `ValidationErrorDefinition` becomes
`ValidationProblemDetails` with the exact key-to-message-array map. It must not add request state.

The terminal result then passes that mapped instance to Concertable's existing
`ApplicationErrorResult`/`ApplicationProblemDetails.WriteAsync` path, which remains responsible for
`instance`, `traceId`, `IProblemDetailsService`, configured customization, content type, response
status, cancellation, and serialization fallback. Tests cover both the pure mapper and executed MVC
response. Reunion receives only the caller-supplied mapper delegate and never references `IError`.

## Compatibility risk register

| Risk | Required evidence before cutover |
|---|---|
| Default/uninitialized Results | Every Result family rejects operational `Match`, `TryGet*`, and combinator use as currently expected; `default(Option<T>)` remains `None`. |
| Null and string validation | Success/error payloads reject null; string errors, including generic `TError == string`, reject null, empty, and whitespace at every factory/case boundary. |
| Construction and implicit conversions | Rewrite raw implicit assumptions to named factories/cases. Only Reunion named `Success`/`Failure`/`Some`/`None` cases may convert. Guard `Result<T,T>` ambiguity. |
| Wrong-case access | No throwing `.Value`/`.Error` compatibility accessors are introduced; callers use `Match`, `TryGetValue`, or `TryGetError`. |
| Async surface | Compare all receiver and callback overloads, branch laziness, invocation count, null-task rejection, exception identity, cancellation, and `ConfigureAwait`-independent behavior. |
| LINQ/query syntax | Compile and execute representative Option, string-error Result, and typed-error Result queries, including fail-fast projection. |
| Equality/hash/default text | Preserve case-sensitive structural equality/hash behavior and explicitly approve any `ToString` text differences before replacement. |
| Serialization | Keep Result/Option out of DTO/event/protobuf/HTTP payload shapes; add architecture scans and formatter tests because Reunion provides no application wire contract. |
| MVC negotiation | Execute results through the real MVC pipeline and verify `application/problem+json`, configured customizers/writers, fallback JSON, and Accept handling. |
| Created Location | Keep `CreatedAtActionResult`; verify route values, generated Location header, status 201, and response body. Do not substitute literal-location `CreatedResult`. |
| Status mapping | Contract-test every `ErrorKind`, validation shape, title, detail, code, instance, trace ID, and custom extensions. Unknown kinds remain rejected, not silently mapped. |
| TFM difference | Concertable initially tests Reunion net10 conventional cases. Add net11 compile/runtime coverage only when Concertable multi-targets or upgrades; Reunion package consumers still verify both assets upstream. |

## PR and branch migration graph

```text
#248 foundation ──#261 sync
       └──#284 error API ──#290 owned carriers ──#291 sync
                              ├──#392 Payment ──#420 sync ──B2B/Auth/Customer owners
                              ├──#380 Search ──#388 sync
                              └──#404 transport + #407 conventions

local package battle test ──Reunion publish
                └──additive Shared expansion ──publish/sync
                     └──Payment + Payment.Client migration ──publish/sync
                          └──final consumer contraction + HTTP terminals ──publish/sync
                               ├── preserve/reconcile B2B local-only owner
                               ├── preserve/reconcile Auth local-only owner
                               ├── update PR #425 once
                               └── recreate PR #282 semantics on the integrated baseline
```

State is from GitHub plus local repository evidence on 2026-08-09. `closed + merged date` is reported
as merged; #336 is the only listed closed-unmerged PR.

| PR | State; base ← head | Result-related scope / duplication | Required action; dependencies; conflict risk |
|---|---|---|---|
| #248 Add typed result core foundation | Merged; `main` ← `Feature/TypedResultMigration` | Initial foundation; historical content evolved through later Kernel PRs. | No action; ancestor of #261/#290; low. |
| #261 Use the published shared exception handler | Merged; `main` ← platform sync `.710` | Published Shared exception boundary adoption. | No action; depends #248 publication; low. |
| #282 Migrate Customer Ticket to typed results | Open; `main` ← `Feature/TypedResultMigrationPhase2` | One unique Ticket/Concert/checkout commit; old CFE/carrier assumptions; not elsewhere. | Recreate semantics/tests after shared integration, then supersede old PR with approval; depends Payment sync and Reunion consumer cutover; very high (776 behind). |
| #284 Define typed result error API | Merged; `main` ← `Feature/TypedResultKernelApi` | Concertable error model prerequisite. | No action; preserve under adapter; low. |
| #290 Add owned Result and Option foundation | Merged; `main` ← `Refactor/OwnedResultFoundation` | Current temporary carriers and tests. | Replace once in Shared producer after package publish; foundational/high public-API risk. |
| #291 Platform sync `.740` | Merged; `main` ← platform sync `.740` | Delivered #290 types. | No action; historical dependency; low. |
| #296 Own deferred commission pricing in Payment | Merged; `main` ← commission branch | Payment prerequisite used by later migration. | No action; dependency of #392; low. |
| #312 Preserve validation errors through ProblemDetails writer | Merged; `main` ← `Fix/ProblemDetailsValidationWriter` | Validation writer/fallback contract. | No action; must be retained by adapter; medium regression risk. |
| #335 Codify typed error union conventions | Merged; docs branch | Error-union conventions. | Update conventions only in the carrier cutover PR; low. |
| #336 Make Payment owned-result branch canonical | Closed unmerged; docs branch | Obsolete ownership proposal. | Superseded by #362/#370 and later roadmap state; do not revive; low. |
| #340 Simplify typed error representation | Merged; docs branch | Error representation decision. | No action; remains Concertable-owned; low. |
| #343 Prefer natural typed error names | Merged; docs branch | Error case naming. | No action; low. |
| #344 Derive published error codes from case names | Merged; `main` ← `Refactor/DerivedErrorDefinitions` | Current stable code/message derivation. | No action; adapter must preserve; medium. |
| #362 Enable parallel typed-result workstreams | Merged; docs branch | Current service ownership model. | Updated by this plan; no branch revival; low. |
| #370 Standardize owned results and guidance delivery | Merged; docs branch | Canonical current carrier conventions. | Update once after Reunion cutover; medium documentation conflict risk. |
| #380 Normalize Search collection operation contracts | Merged; Search branch | Search empty-list/result contract work. | No action; consumer migration handles namespace/package change; medium. |
| #388 Platform sync `.827` | Merged; platform sync | Delivered Search changes. | No action; low. |
| #392 Own typed operation results in Payment | Merged; Payment branch | Payment typed outcomes. | No action; generated consumer migration updates Reunion types; high public-API consumer risk. |
| #404 Establish typed-error transport foundations | Merged; Shared branch | `IError` transport metadata and mapping foundation. | Keep in Concertable; adapter builds on it; medium. |
| #407 Codify typed error mapping | Merged; docs branch | Mapping convention. | Update only if names/imports change; low. |
| #420 Platform sync `.853` | Merged; platform sync | Migrated Payment consumers and unblocked B2B/Auth/Customer. | No action; dependency of active semantic owners; low. |
| #425 Model Customer non-Payment outcomes | Open; `main` ← `Feature/typed-result_customer-outcomes` | 29 unique reviewed commits; not elsewhere. | Preserve and update once against integrated main; do not duplicate package cutover; high (117 behind). |
| #426 Close Payment owned-result migration | Merged; docs closeout | Lifecycle closeout. | No action; low. |
| #427 Finish Payment closeout review fixes | Merged; same docs closeout branch | Review fixes for #426. | No action; low. |
| B2B local-only work | Active, unpushed; recorded owner `Refactor/B2BTypedResultMigration` | Authoritative semantic migration exists locally at `ba5791268`. | Preserve owner and reconcile in Phase 6 after the final contraction; high conflict risk. |
| Auth local-only work | Active, unpushed; recorded owner `Feature/typed-result_auth-outcomes` | Authoritative semantic migration exists locally at `98599413a`. | Preserve owner and reconcile in Phase 6 after the final contraction; high conflict risk. |
| HTTP-terminal local work | Active, unpushed; verified code/test checkpoint `c593150e4` | Semantic Shared.Api terminal rename and Option absence terminals overlap the Reunion contraction surface. | Do not publish independently; incorporate once in Phase 5 after Payment.Client is published on Reunion. |

## Safest integration strategy

Use strategy D: a publish-gated centralized integration. The Phase 1 rehearsal proved the original
two-hop graph incomplete: `Concertable.Payment.Client` publicly re-exposes Kernel Result/Option
types, while B2B and Customer compile against its published package rather than its source project.
Delivery therefore crosses three Concertable code merges after the Reunion publication:

1. Land this docs-only design first so every active branch shares the same owner and dependency map.
2. In the reserved `Feature/typed-result_reunion-integration` worktree, pack merged PR head
   `e33b40f`, restore its three-package dependency graph, and prove source/API/HTTP parity. Do not
   distribute those edits across service PRs.
3. In parallel, finish and review the existing semantic HTTP-terminal work as a local-only checkpoint.
   Do not push or publish its Shared.Api package independently.
4. Publish matching `Reunion` and `Reunion.AspNetCore` versions only after the battle-test gate passes.
5. Merge an additive Shared expansion: reference Reunion from Kernel and Reunion.AspNetCore from
   Shared.Api, add Reunion-backed terminal overloads, and retain every owned carrier and old terminal
   signature. Publish and drive its generated platform sync green.
6. Merge the Payment layer against that published expansion: migrate Payment source and the public
   `Concertable.Payment.Client` API to Reunion. Publish the repacked client and drive its generated
   platform sync green. Do not remove the old Shared identities yet.
7. Merge the final consumer contraction: migrate B2B, Auth, Customer, Ticket, Search, and remaining
   callers against the newly published Payment.Client, incorporate the reviewed semantic HTTP
   terminals, then delete the owned carriers, duplicate extensions, and old terminal overloads.
   Publish and drive the final platform sync green.
8. Reconcile each semantic owner against that integrated `main`: update PR #425 once; sync and update
   the active local B2B/Auth worktrees; recreate #282's Ticket semantics rather than rebasing its
   obsolete carrier implementation.
9. Run the final shared/background inventory and remove leftover third-party surfaces only when all
   semantic owners are terminal.

Updating every PR independently (A) duplicates package pins, carrier renames, and adapter fixes and
guarantees divergent conflict resolutions. A single ordinary mega-integration branch (plain B) cannot
cross either NuGet publication boundary safely. Landing every semantic migration first (C) delays the
common baseline and increases later churn. Strategy D's local battle test → Reunion publish → additive
Shared expansion → Payment.Client migration → final consumer contraction sequence keeps every
published graph buildable while centralizing each mechanical layer exactly once.

## Local package battle-test workflow

Run from PowerShell. The Reunion repository must have SDK
`11.0.100-preview.6.26359.118` plus the .NET 10 SDK/runtime required by its pinned `global.json`.

```powershell
$reunionRepo = 'C:\Users\tommy\source\repos\Reunion'
$concertableWorktree = 'C:\Users\tommy\source\repos\Concertable.worktrees\Feature\typed-result_reunion-integration'
$reunionWorktree = 'C:\Users\tommy\source\repos\Reunion.worktrees\concertable-e33b40f'
$feed = Join-Path $env:LOCALAPPDATA 'NuGet\Reunion-Concertable'
$version = '0.1.0-local.concertable.2'

New-Item -ItemType Directory -Force -Path $feed | Out-Null
git -C $reunionRepo fetch origin --quiet
git -C $reunionRepo worktree add --detach $reunionWorktree e33b40f

dotnet restore "$reunionWorktree\Reunion.slnx"
dotnet pack "$reunionWorktree\src\Reunion\Reunion.csproj" `
  -c Release --no-restore -p:Version=$version -p:PackageVersion=$version -o $feed
dotnet pack "$reunionWorktree\src\Reunion.Errors\Reunion.Errors.csproj" `
  -c Release --no-restore -p:Version=$version -p:PackageVersion=$version -o $feed
dotnet pack "$reunionWorktree\src\Reunion.AspNetCore\Reunion.AspNetCore.csproj" `
  -c Release --no-restore -p:Version=$version -p:PackageVersion=$version -o $feed

Get-ChildItem $feed -Filter "Reunion*$version.nupkg"
tar -xOf "$feed\Reunion.AspNetCore.$version.nupkg" Reunion.AspNetCore.nuspec
```

On the reserved Concertable integration branch only, add both exact versions to
`api/Concertable.Shared/Directory.Packages.props`, add `Reunion` only to `Concertable.Kernel.csproj`,
and add `Reunion.AspNetCore` only to `Concertable.Shared.Api.csproj`. Then restore without creating or
committing a machine-specific NuGet configuration:

```powershell
dotnet restore "$concertableWorktree\api\Concertable.slnx" `
  --force --no-cache `
  -p:RestoreAdditionalProjectSources=$feed `
  -v normal

dotnet list "$concertableWorktree\api\Concertable.Shared\src\Concertable.Kernel\Concertable.Kernel.csproj" `
  package --include-transitive
dotnet list "$concertableWorktree\api\Concertable.Shared\src\Concertable.Shared.Api\Concertable.Shared.Api.csproj" `
  package --include-transitive
dotnet nuget why "$concertableWorktree\api\Concertable.Shared\src\Concertable.Kernel\Concertable.Kernel.csproj" Reunion
dotnet nuget why "$concertableWorktree\api\Concertable.Shared\src\Concertable.Shared.Api\Concertable.Shared.Api.csproj" Reunion.AspNetCore
```

The restore log must show the local feed supplying all three exact packages; the AspNetCore `.nuspec`
must show exact dependencies on Reunion and Reunion.Errors. Before any production
PR, replace the local version with the published version and restore without
`RestoreAdditionalProjectSources`. A temporary committed battle-test pin may live locally on the
reserved integration branch. It is not pushed; after Reunion publication, Phase 3 replaces it with
the exact production version in a later commit before the first producer push/PR. The merge gate is:

```powershell
rg -n 'local\.concertable|Reunion\.worktrees|Reunion-Concertable|RestoreAdditionalProjectSources' `
  "$concertableWorktree\api"
git -C $concertableWorktree diff --check
```

The first command must return no committed-source matches after the real-version replacement. Remove
the detached Reunion worktree after testing with the exact verified path:

```powershell
git -C $reunionRepo worktree remove $reunionWorktree
```

## Required automated test plan

### Carrier parity

- All five Result families: every success/failure case, factory, `Match`, `TryGet*`, `Map`, `Bind`,
  error mapping, tap/recovery behavior, equality/hash/text, and collection traverse/sequence behavior.
- `Option<T>` Some/None/default, nullable conversions, equality, mapping/binding, fallback, and
  task receivers.
- `OrFailure` eager, lazy, Task receiver, and async factory paths: correct case, exact payload,
  success-side laziness, failure-side single invocation, null delegate/task/payload rejection,
  cancellation, and original exception identity.
- Invalid construction: null payloads/errors, empty and whitespace string errors including generic
  string error types, default named cases, and uninitialized Results.
- Representative LINQ queries for Option, string Result, and typed Result, including fail-fast and
  same-type value/error discrimination.

### Concertable error and MVC contract

- Every `ErrorKind` status, reason-phrase title, definition message detail, and stable `code`
  extension.
- Validation keys and message arrays through both `ValidationProblemDetails.Errors` and fallback
  serialization.
- Executed ProblemDetails includes request `instance`, `traceId`, configured custom extensions,
  correct `application/problem+json`, and preserves `IProblemDetailsService` customizers/writers.
- Success controller responses: 200 body, 404 NotFound, 204 NoContent, and existing formatter/content
  negotiation behavior under representative Accept headers.
- Created controller response: 201, exact body, MVC-generated Location from action and route values,
  and typed failure behavior. Assert Reunion's literal-location helper is not used for this path.
- Async services/controllers preserve cancellation and infrastructure exceptions and do not convert
  them into expected failures.

### Repository and package gates

- Run affected Kernel and Shared.Api unit/architecture suites first, then each changed service's unit
  and integration suites through the repository integration-debug workflow.
- Build `api/Concertable.slnx` Release with zero errors and build each standalone Payment, B2B,
  Customer, and Search `.slnx` against its published package closure.
- Run architecture scans proving no Result/Option wire DTOs, no `Reunion.AspNetCore` dependency in
  domain/application projects, no remaining duplicate Concertable carriers/extensions after cleanup,
  and no Minimal API result migration.
- Verify clean package consumers restore `Reunion` transitively from `Concertable.Kernel` and the
  matching `Reunion.AspNetCore`/`Reunion` pair from `Concertable.Shared.Api`; capture restore
  provenance and package graph.
- Concertable currently targets net10 only. Upstream Reunion gates both net10 and net11 packages. When
  Concertable adopts net11, rerun the same behavioral suite against native union cases plus exhaustive
  switching, native case conversions, positional patterns, `Result<T,T>`, and Option None/default.
- Let each code PR's merge queue run the selected E2E tier; do not duplicate E2E locally unless the
  queue fails and the repository debug workflow is invoked.

## Representative manual battle tests

Use real B2B/Customer flows after the automated gate:

1. GET an existing entity: 200, expected body and formatter.
2. GET a missing entity: exact 404 behavior without an unexpected problem body.
3. POST a valid request: 201, expected body, and route-generated Location that resolves.
4. POST a validation or conflict failure: exact 400/409 title, detail, code, validation errors,
   instance, trace ID, and configured custom extensions.
5. DELETE success: 204 with no body.
6. DELETE a domain failure: exact mapped status and ProblemDetails contract.

## Phases and verification gates

### Phase 1 — Reconcile owners and battle-test corrected merged release head `e33b40f` ✅

- Confirm the recorded B2B/Auth/Customer/Ticket/HTTP owner inventory still matches the live worktrees
  before the first code edit; update the ledger if any head or dirty path changed.
- Create the isolated integration worktree and local feed; pack and inspect all three matching packages.
- Add the local Reunion packages and additive Shared.Api overloads on the reserved integration branch;
  retain the old carriers so the complete published-package closure remains buildable, then run
  carrier plus Shared.Api parity tests.

Gate: local package provenance proven, Kernel 241/241 and Shared.Api 53/53 green, Release solution
build green, no machine-local configuration staged, and the published-package topology recorded as
Shared expansion → Payment.Client migration → final consumer contraction.

### Phase 2 — Publish the Reunion package family ✅

- Publish matching `Reunion`, `Reunion.Errors`, and `Reunion.AspNetCore` version
  `0.1.0-alpha.1` packages built from corrected merged head `e33b40f`; do not publish the superseded
  four-package `e52129d` tree or the removed `Reunion.Errors.Extensions` package under the immutable
  release version.
- Verify package contents, dependency version, both TFMs, clean package consumers, and feed
  availability from a clean cache.

Gate: all three packages are immutable and restorable from the production feed at the exact same
version, with their dependency groups resolving only that version.

### Phase 3 — Additive Concertable Shared expansion

- Replace the local package version with the exact published Reunion version; reference Reunion only
  from Kernel and Reunion.AspNetCore only from Shared.Api.
- Add Reunion-backed Shared.Api terminal overloads while retaining the nine owned functional source
  files and every old public terminal signature. This is a temporary binary/source-compatible expand
  step, not a permanent compatibility layer.
- Keep Concertable errors, MVC execution, controller signatures, and existing call sites unchanged.

Gate: Shared unit/architecture/package tests and the full Release solution build green; code review
clean; source PR merged; the generated platform sync green and merged; expanded Concertable Shared
packages available. Do not contract the public surface on an unpublished expansion.

### Phase 4 — Payment and Payment.Client migration

- Migrate Payment source and the published `Concertable.Payment.Client` public API from
  `Concertable.Kernel.Functional` to Reunion against the published Shared expansion.
- Preserve Payment contracts, exceptions, HTTP behavior, and client call semantics; add direct
  Reunion ownership to Payment.Client only if its public API requires the compile asset explicitly.
- Own the generated platform sync and prove B2B/Customer consumers compile against the republished
  client before the next contraction.

Gate: Payment build/unit/integration/package-consumer verification and merge-queue E2E green; source
PR merged; generated platform sync green and merged; the Reunion-based Payment.Client package is
available. Never remove the old Shared identities before this gate.

### Phase 5 — Final consumer migration and Shared contraction

- Migrate every remaining service import/call site against published Shared and Payment.Client
  packages, making package restore provenance explicit.
- Incorporate the reviewed semantic HTTP-terminal checkpoint; keep Option absence terminals limited
  to NotFound and NoContent, with caller-actionable failures represented by typed Results.
- Remove the nine owned carrier/extension files and old Shared.Api overloads only after the repository
  inventory proves no production caller or published package still exposes their assembly identity.
- Preserve controller signatures, CreatedAtAction, HTTP behavior, domain errors, exceptions, and
  service carve boundaries.

Gate: all standalone builds, unit/integration suites, package consumers, architecture tests, and
merge-queue E2E green; source PR and generated platform sync merged. Never leave a sync red.

### Phase 6 — Reconcile active semantic migration owners

- Update PR #425 exactly once against integrated main and preserve its 29 unique commits.
- Merge integrated main into the authoritative B2B/Auth owners after their other-workstation state is
  synced; resolve only semantic conflicts there.
- Recreate #282's Ticket/Concert behavior and tests on the integrated baseline, then request approval
  before superseding the old PR.

Gate: every owner has one authoritative branch/PR, no duplicate carrier/package changes, normal
service verification green, and each generated platform sync terminal.

### Phase 7 — Repository cleanup and closeout

- Inventory and remove remaining FluentResults/CFE production use and duplicate Concertable carrier
  code only after the last semantic owner no longer needs it.
- Enforce dependency and wire-boundary rules and update current code conventions to Reunion.
- Complete manual battle tests, final repository/package provenance checks, and lifecycle closeout.

Gate: definition of done below, reviews resolved, PRs/merges/publications/syncs terminal, then delete
this plan and ledger together through the documented docs closeout.

## Definition of done

- Concertable consumes published, matching Reunion packages; no local feed/version/path remains.
- All five Reunion types remain available and all production Result/Option call sites use them.
- Only Kernel directly owns `Reunion`; only Shared.Api directly owns `Reunion.AspNetCore`; no domain or
  application ASP.NET dependency was introduced.
- `IError` and every error union remain Concertable-owned.
- Controller signatures and observable MVC status/body/header/ProblemDetails behavior are preserved.
- Existing semantic migration work is preserved under one owner per scope; #282 is superseded only
  after its replacement is ready and approved.
- Every local, CI, publication, package-consumer, generated sync, and manual battle-test gate is
  terminal and recorded in the progress ledger.
