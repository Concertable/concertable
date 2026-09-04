# Test-tier naming, and the gates that should stop configuration defects before E2E

> **This is a research recommendation, not a code-review pass.** It deliberately carries no
> `## Review pass` descriptor, no candidate base/head identity and no `[ ]` findings, because there is
> nothing to review: the branch is clean at merged `main` (`e1475b473`) with an empty diff, and the brief
> was explicitly read-only — decide, then implement. It occupies `reviews/Chore-TestTierNaming.md`
> because that is where the brief asked for it and it is the canonical slug for `Chore/TestTierNaming`.
> If a real review pass lands on this branch later, append it below under a proper
> `## Review pass` heading per `review-lifecycle`; do not retrofit this section into one. See §6.8.

Read-only pass over merged `main` (`e1475b473`) in the `Chore/TestTierNaming` worktree. Nothing here is
implemented.

Everything below was verified against the tree. Where the brief was wrong, the correction is marked
**[CORRECTION]** and stated with its evidence. Where research found no canonical name, that is said
plainly rather than papered over with a coinage.

---

## 0. Corrections to the brief

The brief was written from a long session and it has three material errors. Two of them change the
recommendation.

### [CORRECTION 1] The unit tier is *not* blocked from asserting the E2E composition

The brief says: *"`AddE2EStack` extends `IDistributedApplicationTestingBuilder`, which comes from
`Aspire.Hosting.Testing`. So the E2E composition cannot currently be asserted from the unit tier."*

The ban is narrower than that, in two ways that matter.

1. **`Aspire.Hosting.Testing` is not banned at package level.** `api/TestConventions.targets`'s
   `_ConcertableHostPackage` list is `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.AspNetCore.TestHost`,
   `Respawn`, `Testcontainers*`, `Microsoft.Playwright*`, `Reqnroll*`. Aspire is absent.
2. **The symbol ban names the concrete type, not the interface.** `api/BannedSymbols.UnitTests.txt` bans
   `T:Aspire.Hosting.Testing.DistributedApplicationTestingBuilder` — the static factory. There is no
   entry for `IDistributedApplicationTestingBuilder`. `RS0030` fires on *use of a symbol*, so naming the
   interface in a signature is legal in the unit tier.

Both are already exercised. `api/tests/Concertable.E2E.Source.UnitTests` is a unit-tier project that
references `Concertable.E2E` (whose `IComposition.CreateBuilderAsync` returns
`IDistributedApplicationTestingBuilder`) and builds green today. And
`api/Concertable.Search/tests/E2ETests/Concertable.Search.E2ETests.Helpers.UnitTests/ContainerBackedPinningTests.cs`
(7 tests — the count in the brief is right) already **composes real Aspire resource graphs in the unit
tier**, via `DistributedApplication.CreateBuilder()` from plain `Aspire.Hosting`:

```csharp
var builder = DistributedApplication.CreateBuilder();
var paymentWeb = builder.AddPaymentWeb("test-image", digest, auth, paymentDb, asb).Resource;
var e2ePaymentWeb = DistributedApplicationBuilderExtensions
    .SubstituteE2EProject(builder, paymentWeb, new TestProjectMetadata("payment-e2e-web.csproj"));
Assert.Equal(PaymentConstants.ServiceName, Environment(e2ePaymentWeb)["ServiceBus__ServiceName"]);
```

That is precisely defect #2's assertion, running in the unit tier, in milliseconds. The capability
already exists; it is just not applied to the whole stack.

The one genuine blocker is narrow: to call `AddE2EStack` you need an
`IDistributedApplicationTestingBuilder` *instance*, and the only way to get one is the banned factory.

### [CORRECTION 2] Widening the `AddE2EStack` signatures is a pure signature change — proved

The brief asks whether widening is "the enabling change". It is, and it costs nothing, because the
interface derives from the plain one. Disassembled from
`~/.nuget/packages/aspire.hosting.testing/13.3.2/lib/net10.0/Aspire.Hosting.Testing.dll`:

```
.class interface public abstract auto ansi beforefieldinit
       Aspire.Hosting.Testing.IDistributedApplicationTestingBuilder
       implements [Aspire.Hosting]Aspire.Hosting.IDistributedApplicationBuilder,
                  [System.Runtime]System.IAsyncDisposable,
                  [System.Runtime]System.IDisposable
```

Every one of its 13 members is a **default interface method forwarding to the base**
(`callvirt ... IDistributedApplicationBuilder::get_Configuration()` and so on). `BuildAsync` is the only
genuinely abstract member it adds.

`AddE2EStack`'s body
(`api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests/DistributedApplicationBuilderExtensions.cs:16`)
uses only `Resources`, `Configuration`, `CreateResourceBuilder`, `AddResource` — all base members. So
changing `extension(IDistributedApplicationTestingBuilder builder)` to
`extension(IDistributedApplicationBuilder builder)` on `AddE2EStack`, `PinAuthService`, `PinPaymentWeb`,
`PinPaymentWorkers`, `PinStripeCli` and `AddEphemeralSql` is a **type-only edit with no body changes**.
`AddSearchService` already takes the plain interface. The only adjustment is `AddE2EStack`'s return type,
which exists so `AppFixture` can chain `BuildAsync` — and `AppFixture` doesn't actually chain it
(`api/.../AppFixture.cs:94` discards the return and calls `builder.BuildAsync()` separately), so the
return type can become `void` or stay narrow behind a thin overload.

### [CORRECTION 3] `IComposition`'s indirection is a real carve seam, not ceremony

The brief calls the reflection-by-string-name a symptom of a mis-shaped abstraction. Half right. The
reason it exists is concrete and load-bearing:

```xml
<!-- api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests.csproj:32 -->
<ProjectReference Include="...\Concertable.E2E.Source\Concertable.E2E.Source.csproj"
                  Condition="'$(UseSourceComposition)' == 'true'" />
```

`UseSourceComposition=false` is an exercised build mode —
`plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_STAGE4_SYSTEM_PROGRESS.md:89` records a carve build
against `0.1.0-local.1788306290376` with it set. Under that mode `Concertable.E2E.Source` is not in the
build at all, so a compile-time reference is impossible and `Type.GetType` is the only option. The
abstraction is the repository-split seam.

The brief's design critique still lands on the *shape*: because all seven members are `IProjectMetadata`,
an image-backed sibling has nothing to return, so the interface can only ever have one implementation —
and under `UseSourceComposition=false` it has **zero**, meaning `Compositions.Source()` throws at runtime
in exactly the mode the seam exists for. Worth fixing, but by naming and splitting, not deletion. §1.3.

---

## 1. Naming recommendation

### 1.1 Is "architecture test" an umbrella term, or exclusive to ArchUnitNET?

**Neither. The words aren't reserved, but in the .NET ecosystem the term has a settled narrow meaning:
assertions over *code structure*.** Dependency direction, layering, cycles, naming conventions,
sealedness, attribute presence. That is the definition in
[NetArchTest](https://github.com/BenMorris/NetArchTest),
[Code Maze](https://code-maze.com/csharp-architecture-tests-with-netarchtest-rules/),
[Milan Jovanović](https://milanjovanovic.tech/blog/shift-left-with-architecture-testing-in-dotnet) and
[Anton Dev Tips](https://antondevtips.com/blog/why-do-you-need-to-write-architecture-tests-in-dotnet). It
names a *technique*, not a container.

Reading it broadly ("does the system conform to its intended architecture") is defensible English, but it
buys nothing here: under that reading *every* static assertion qualifies, the label stops predicting what
a suite contains, and the tier stops being a decision. Concertable's suites are already the evidence —
`*ArchitectureTests` holds three unrelated things, and the label leaks in both directions.

**Verified inventory of the six suites:**

| Project | File | What it actually asserts | Kind |
|---|---|---|---|
| `B2B.ArchitectureTests` | `ModuleBoundaryTests` | **ArchUnitNET over compiled IL**: layer reference graph, cross-module isolation | Architecture (canonical) |
| `B2B.ArchitectureTests` | `ControllerBoundaryTests` | **ArchUnitNET**: endpoints declare authorization; route segments match controller names | Architecture (canonical) |
| `B2B.ArchitectureTests` | `ReunionArchitectureTests` | no dependency on `Option`; legacy result identities absent | Architecture (canonical) |
| `B2B.ArchitectureTests` | `B2BHostGraphTests` | `ValidateComposition` DI graph; `JwtBearerOptions.RequireHttpsMetadata`; Aspire endpoint names/schemes/target ports; container runtime args; `HttpsCertificateAnnotation`; SPA-origin ↔ CORS ↔ redirect-URI consistency | **Startup + app model** |
| `Customer.ArchitectureTests` | `CustomerArchitectureTests` | same mix, plus `Web_ReferencesNoModuleInfrastructureAssembly` | Mixed |
| `Payment.ArchitectureTests` | `PaymentArchitectureTests` | same mix | Mixed |
| `Payment.ArchitectureTests` | `PaymentContractReferenceTests` | assembly-reference rules | Architecture |
| `Payment.ArchitectureTests` | `PaymentPublishedPackageReferenceTests` | package-reference rules | Architecture (packaging) |
| `Search.ArchitectureTests` | `SearchArchitectureTests` | same mix | Mixed |
| `Auth.ArchitectureTests` | `AuthArchitectureTests` | DI graph + `AppHost.CreateBuilder([]).Build()` only | **Startup** |
| `AppHost.ArchitectureTests` | `AppHostArchitectureTests` | umbrella AppHost builds via the testing builder; executable-host coverage inventory | **Startup + inventory** |

And leaking the other way — five architecture-flavoured *classes* living in `*.UnitTests` projects:

- `Concertable.Shared.Api.UnitTests/TypedResultArchitectureTests.cs` (file-system scan of every
  `Program.cs` and `*HostExtensions.cs` under `api/*/src/`)
- `Concertable.Shared.Api.UnitTests/RepositoryArchitectureTests.cs`
- `Concertable.Search.UnitTests/Architecture/ContractArchitectureTests.cs`
- `Concertable.B2B.Deal.UnitTests/Strategies/DealStrategyArchitectureTests.cs`
- `Concertable.B2B.Concert.UnitTests/DisplayNameConventionTests.cs`

**Recommendation:** `*.ArchitectureTests` keeps **code-structure rules only** — ArchUnitNET, reflection
over assemblies, assembly/package reference rules, file-system convention scans. That is the ecosystem
meaning, it needs no tier-gate edit, and it makes the four `*ArchitectureTests` classes now sitting in
unit projects *move into it* rather than be renamed.

### 1.2 What is the thing you actually described called?

Your own framing is the right one: *"before the end-to-end test runs, can the app actually load up with
any of these problems?"*

**There is no single settled industry name for that tier.** I searched for one and it is not there:

- Aspire's own [testing overview](https://aspire.dev/testing/overview/) calls everything that drives an
  AppHost "closed-box integration testing" and **never names resource-graph inspection as a category at
  all**. Its [advanced scenarios](https://aspire.dev/testing/advanced-scenarios/) page describes
  inspecting `appHost.Resources` without starting resources, and gives that no name either.
- Aspire's [glossary](https://aspire.dev/get-started/glossary/) has **no entry for "composition"**. Its
  vocabulary is *AppHost*, *resource*, *reference*, *WithReference*, *WaitFor*, *connection string*,
  *environment variable*. The [resource-model page](https://aspire.dev/architecture/resource-model/) adds
  *app model* and *resource graph* (an explicit developer-authored DAG), and uses "resource composition"
  once, in passing prose, for the fluent wiring.
- "Smoke test" and "contract test" are both taken and mean other things — a smoke test *runs* the
  deployed system; a contract test (Pact) is consumer-driven API compatibility.

So I will not hand you a coined tier name dressed up as an industry term. What *does* exist is a
vendor-blessed name for the mechanism, and it happens to be the exact sentence you said:

**Startup validation.** .NET's own term. `ValidateOnStart()` registers `IStartupValidator`, whose
`Validate()` "calls the `IValidateOptions<T>` validators" and throws `OptionsValidationException` listing
**all** failures. Verified in
`~/.nuget/packages/microsoft.extensions.options/10.0.0/lib/net10.0/Microsoft.Extensions.Options.xml:508`.
Critically, `IStartupValidator` is resolvable from the provider and callable **without starting the
host**.

Its sibling is **service provider validation** (`ValidateOnBuild` / `ValidateScopes`), already in this
repo as `UseStrictServiceProviderValidation` and `StrictDistributedApplication`.

**Recommendation: the tier is `*.StartupTests`, and it owns one question — "would this host refuse to
boot, given the configuration the app model actually supplies it?"** Its name is built out of vendor
vocabulary rather than invented, and it reads back as the thing it does. Two candidates I rejected:

- `*.CompositionTests` — the word is triple-booked here (§1.3) and the skill already documents projects
  by that name that do not exist (§5.1). Adopting it would make an existing docs-vs-reality defect look
  intentional.
- `*.ConfigurationContractTests` — already rejected, and rightly: "configuration contract" is not a term
  anyone else uses, and the suite asserts more than configuration (DI graph, endpoint schemes, wait
  edges).

**Namespace and class names**, with the no-stuttering constraint applied:

```
api/Concertable.B2B/tests/Concertable.B2B.StartupTests/
    namespace Concertable.B2B.StartupTests;
      WebHostTests            (was B2BHostGraphTests's Web_* facts)
      WorkerHostTests         (was Functions_*)
      SeedSimulatorHostTests  (was SeedSimulator_*)
      ResourceGraphTests      (was AppHost_*, LocalSpaSurfaces_*, AppHost_WebSpaOrigins_*)
```

`ResourceGraphTests` rather than `HostGraphTests` or `AppModelTests`: "resource graph" is Aspire's own
term for the DAG, so a reader can look it up. `AppModelTests` is equally vendor-grounded but less
self-describing to someone who hasn't read the glossary.

This also fixes constraint 4 wholesale. In `namespace Concertable.B2B.StartupTests`, none of
`WebHostTests`, `WorkerHostTests`, `ResourceGraphTests` repeats the namespace — whereas
`B2BHostGraphTests`, `CustomerArchitectureTests`, `PaymentArchitectureTests`, `SearchArchitectureTests`,
`AuthArchitectureTests` and `AppHostArchitectureTests` all do today.

**The cost, stated honestly.** A new suffix requires editing `api/TestConventions.targets` (one
`PropertyGroup` line plus the error message) — cheap, but note the `EndsWith`-before-`Contains` ordering
already documented there, and that `.StartupTests` must be tested by `EndsWith` so a hypothetical
`Concertable.X.E2ETests.Helpers.StartupTests` resolves correctly. It also needs
`[assembly: AssemblyTrait("Category", "Startup")]` and a `startup-tests` CI job.

**Against that, the cheap path is real and worth knowing:** `architecture-tests` **already runs on pull
requests** (`if: needs.changes.outputs.run_tests == 'true'`, no `merge_group` restriction) and is
**already a `needs:` of `e2e-api-tests`** (`.github/workflows/test.yml:910`). E2E is merge-queue-only. So
every gate in §3 placed in today's `*.ArchitectureTests` projects runs on the PR and blocks E2E with
**zero** workflow or tier-gate changes. If you want "you cannot push code that breaks the composed
configuration" landed this week, put the gates in the existing projects and do the rename as a follow-up.
The rename is a naming improvement; it is not what buys the gate.

### 1.3 The three senses of `Composition`

Verified occurrence counts across `api/**/*.cs`:

| Sense | Identifiers (count) | Verdict |
|---|---|---|
| **1. DI composition root** | `CompositionTestArguments` (24), `CompositionValidationOptions` (12), `ValidateComposition` (11), `CompositionValidationExtensions` (1), `CompositionValidationExclusion` (MSBuild, 8 declarations), `ReadHostCompositionSources` (2) | **Keeps the word** |
| **2. What backs each E2E host** | `IComposition` (8), `SourceComposition` (5), `Compositions` (4), `SourceCompositionTests` (2), `UseSourceComposition` (MSBuild) | **Loses it → `Provider`** |
| **3. Domain / plain English** | `ConcertWorkflowCompositionTests` (1), `TypedErrorCompositionTests` (1) | One keeps it, one doesn't |

**Sense 1 keeps the word.** "Composition Root" is a settled term of art from Mark Seemann's *Dependency
Injection in .NET* — the single place an application composes its object graph. That is a real literature
anchor, and `ValidateComposition` accurately describes validating that graph (it does more than
`ValidateOnBuild`: framework activation roots, keyed services, closed generic consumers, hosted services).
Keep `ValidateComposition`, `CompositionValidationOptions`, `CompositionValidationExtensions`,
`CompositionValidationExclusion`.

`ReadHostCompositionSources` (`TypedResultArchitectureTests.cs:322`) is **sense 1** and correctly named:
it reads `Program.cs` plus every sibling `*HostExtensions.cs` — literally the source files that make up
the host's composition root.

**`CompositionTestArguments` is the exception, and it is not a naming problem.** It is a hand-maintained
list of 25 configuration values standing in for what a deployed host receives —
`--ConnectionStrings:asb=...`, `--ServiceAuth:AuthClientId=composition-auth`,
`--ServiceBus:ServiceName=composition`, and so on. Every one of defects #1, #2 and #8 was a *missing*
value of exactly that kind, and this file **already contained** the value the composed graph failed to
supply. So the fixture is not the fix for the tier's blind spot; it is the blind spot. It proves "a host
works given correct config" — the same thing the integration tier already proves, and the same gap the
brief identifies. Under §4.2 the gate should feed each host the configuration the **app model** actually
supplies, not a curated list, and `CompositionTestArguments` shrinks to the genuinely environmental
leftovers (`--Functions:Worker:*`). Rename it then, to say what survives; renaming it now would be motion
without meaning.

**Sense 2 loses the word, and the correct suffix is the one it used to have.** Evidence, four ways:

1. Aspire's glossary has no "composition"; the word is borrowed, not inherited.
2. `dotnet-standards:csharp-naming`'s suffix table defines `Provider` as *"Supplies a value or a
   pluggable strategy, often one of several"*, precedent `IServiceProvider` / `IFileProvider` /
   `TimeProvider`. That is exactly what this type is: it supplies the projects and the AppHost entry point
   that back one E2E run, and it exists *because* there is meant to be more than one of it
   (`UseSourceComposition`).
3. The code used to call it that. Branch history had `FleetProjectProviders.Source()`; the rename to
   `Composition` is what created the collision.
4. **The repo's own prose never stopped calling it a Provider.**
   `api/tests/Concertable.E2E.Source/Concertable.E2E.Source.csproj:13` reads
   `<CompositionValidationExclusion>Monorepo-only source **provider** for the system E2E harness.`

Recommended shape — and it splits the two jobs the interface currently fuses:

```csharp
// Concertable.E2E — the seam, referenced unconditionally
public interface IE2EHostProvider
{
    Task<IDistributedApplicationTestingBuilder> CreateBuilderAsync(Surface surface, CancellationToken ct = default);
    IProjectMetadata Auth { get; }
    // ... the six others
}

public static class E2EHosts
{
    public static IE2EHostProvider Source() => /* as today */;
}

// Concertable.E2E.Source — namespace Concertable.E2E.Source
public sealed class HostProvider : IE2EHostProvider { ... }   // not SourceHostProvider: Source.Source stutters
```

`E2EHosts.Source()` rather than `Providers.Source()`: the static entry point is a lookup of *which host
set*, and `E2EHosts` says that without the meaningless `Providers` plural.

**Does any of this justify a new namespace?** No. `Concertable.E2E` / `Concertable.E2E.Source` already
separate the seam from its monorepo implementation, and that split is exactly the carve boundary. Adding
a namespace would be motion. The one namespace change worth making is the tier rename in §1.2, which is a
project rename that carries its namespace with it.

**Sense 3, split verdict** — and one **[CORRECTION]** to the brief, which called both "plain English,
probably fine":

- `ConcertWorkflowCompositionTests` is **sense 1, not plain English.** It imports
  `Microsoft.Extensions.DependencyInjection` and asserts which workflow type resolves per `DealType`. The
  name is accurate. Leave it. (It does raise a separate tier question — it resolves from a service
  provider inside a `*.UnitTests` project — but that is the `keyed-strategies` pattern and out of scope.)
- `TypedErrorCompositionTests` has nothing to do with composition in any sense. It asserts that
  `PurchaseError.NotFound(42).Definition` has a stable code, message and `ErrorKind`. That is
  `TypedErrorDefinitionTests`. Rename it.

---

## 2. Tier map

Five tiers, each owning one question. The first four are all *static or in-process*; only E2E starts the
world.

| Tier | Owns the question | Boots what | Runs where | Cost |
|---|---|---|---|---|
| **Unit** | Does this deterministic logic compute the right answer? | nothing | PR | seconds |
| **Architecture** | Does the *code structure* conform — layers, dependency direction, naming, references? | nothing | PR | seconds |
| **Startup** | Would any host refuse to boot, given the configuration the app model actually supplies it? | nothing (graphs only) | PR, and blocks E2E | seconds |
| **Integration** | Does this host's behaviour satisfy its contract, given correct configuration? | one host + a DB container | PR | minutes |
| **E2E** | Does the product work end to end across real services? | the whole stack | merge queue | ~25 min |

**What E2E is *for*, once startup defects are caught earlier.** Right now E2E is doing two jobs and only
one of them is its own. It is the product's end-to-end proof *and* the system's first configuration check
— and because the second job fails several layers from its cause (a missing `asb` connection string
surfacing as "Timed out waiting for PayoutAccounts to be provisioned"), it is a terrible configuration
check that happens to be the only one.

Once the Startup tier exists, **E2E's remit is behaviour that only emerges from real services talking to
each other**: a ticket purchase moving money through Stripe and landing a settlement row; a contract
transitioning through its lifecycle across B2B and Payment; an integration event published by one service
being consumed by another through a real bus. Its failure message should name a product step. If an E2E
run fails and the cause turns out to be a missing environment key, a port that was never published, or a
wait pointing at a resource that never starts, that is **a missing Startup test**, and the fix is to add
it — not to note it in the E2E suite and move on. That rule is what makes the tier boundary hold.

The nine defects are the calibration: **eight of nine were not product failures, and eight of nine were
catchable without starting a process.**

---

## 3. Gate table

Cost is wall-clock for the gate itself. "Catches" is measured against the nine defects.

| # | Gate | Asserts | Catches | Cost | Lives in | Runs |
|---|---|---|---|---|---|---|
| G1 | **Composed-graph startup contract** — for every resource in every AppHost's app model, plus every substituted E2E resource, build the owning host's DI graph with *the configuration the graph supplies that resource* and resolve `IStartupValidator.Validate()` | every host's required config is actually supplied | **#1, #2, #8** | ~2s/host | Startup | PR |
| G2 | **Pinned endpoint is proxyless and on its contract port** — every endpoint `PinHttpsEndpoint` touches has `Port == contract` and `IsProxied == false` | DCP will publish the port the tests dial | **#3** | <1s | Startup | PR |
| G3 | **No wait targets an explicit-start resource that has a replacement** — after `AddE2EStack`, no `WaitAnnotation` points at a resource carrying `ExplicitStartupAnnotation` when `{name}-e2e` exists | the run cannot hang forever | **#7** | <1s | Startup | PR |
| G4 | **Endpoint name agrees with `UriScheme`** — an endpoint named `https` has `UriScheme == "https"`, and vice versa | the URL `GetEndpoint("https")` builds is actually TLS | **#4 (declaration half)** | <1s | Startup | PR |
| G5 | **A resource declaring an `https` endpoint can serve TLS** — either it is a `ProjectResource`, or it carries `HttpsCertificateAnnotation` **and** its image is recorded as binding HTTPS | the remaining half of #4 | **#4, #5** | <1s | Startup | PR |
| G6 | **No two resources claim the same host `Port`** across one AppHost's graph | port collisions | — (latent) | <1s | Startup | PR |
| G7 | **No environment a live host needs is attached to an `ExplicitStartupAnnotation` resource** | substitution didn't strand config | reinforces #1/#2/#8 | <1s | Startup | PR |
| G8 | **Pinned image digests agree across AppHosts** — the 12 `*Image`/`*Digest` constants in the four standalone `AppHost.cs` files reduce to one source | drift between AppHosts | — (latent) | <1s | Startup or a script | PR |
| G9 | **Pinned images are pullable** — `docker manifest inspect` for every pinned image/digest in every AppHost | **#9** | ~2s | `scripts/e2e.ps1` (exists, partial) | local + CI E2E preflight |
| G10 | **Generated split inventory is current** | **#6** | ~1s | `eng/repository-split/inventory.py --check` | PR — **already exists and worked** |
| G11 | **`timeout-minutes` on every CI job** | nothing; bounds the cost of any future hang | — | free | `.github/workflows/test.yml` | all |

### Notes on the candidates from the prior session

- **Accepted as written:** proxyless-and-on-contract-port (G2), no-wait-on-explicit-start (G3),
  no-two-resources-share-a-port (G6), every-substituted-project-resolves-its-keys (G1).
- **Split in two (G4/G5).** "`UriScheme == "https"` is consistent with the endpoint's name **and** with
  what the owning resource can serve" is two assertions with very different costs. The name↔scheme half
  (G4) is free and catches the declaration. The can-it-actually-serve half (G5) cannot be fully proved
  statically for a foreign image — the only ground truth is booting it. G5 above is the *cheap
  approximation*: it catches "declared `https`, is a container, has no certificate annotation", which is
  defect #5's shape exactly, and catches #4's shape too as long as the image's binding behaviour is
  recorded somewhere the gate can read.
- **Rejected: "one booted container answering TLS on any endpoint declared `https`".** It is the only
  thing that *proves* #4, and I'd still not put it on the PR: it needs a container runtime and a registry
  credential on every PR, which is the cost profile that made E2E queue-only in the first place. Better
  placement — fold it into the **image publish** workflow (`.github/workflows/publish-images.yml`), where
  the image is already built and a runtime is already present: after pushing, boot the image and assert
  which schemes it answers on which ports, and record the answer next to the digest. Then G5 becomes a
  free static lookup against a recorded fact, and #4 is caught at the moment the image is produced rather
  than at the moment something consumes it.
- **G11 is not a gate and I am not proposing it as one.** It catches nothing. It is here because defect
  #7 cost two hours and there are currently **zero `timeout-minutes` in the entire workflow** (verified:
  `grep -c timeout-minutes .github/workflows/test.yml` → 0), so GitHub's 360-minute default applies. G3
  is the fix for #7; G11 just means the *next* unknown hang costs 30 minutes instead of six hours. It is
  a cost bound, never a substitute for diagnosing anything.

### G9 is only partially in place

`Assert-PinnedImagesPullable` (`scripts/e2e.ps1:104`) checks **one** image — B2B's Auth — by regexing
`AppHost.cs` for `AuthImage`/`AuthDigest`. It does not check Payment web, Payment workers, the B2B seeding
simulator, or the Customer/Search/Payment AppHosts' copies. And it runs only in the local script, not in
CI. Widening it is a few lines; G8 makes it a one-liner.

---

## 4. Enabling changes, ordered by leverage

### 4.1 Widen six signatures to `IDistributedApplicationBuilder` — highest leverage, near-zero cost

Proved in [CORRECTION 2] to be a type-only edit. This single change makes `AddE2EStack` — the entire E2E
composition, the thing that produced five of the nine defects — assertable from a test that starts
nothing, using the pattern `ContainerBackedPinningTests` already uses. G1 through G7 all become possible.

Do this first, regardless of any naming decision.

### 4.2 Declare required configuration through the options pattern with `ValidateOnStart()`

This is the change worth more than any individual test, and the brief is right to single it out. The
current idiom is a hand-rolled lazy check inside a `Configure` lambda:

```csharp
// api/Concertable.Auth/src/Concertable.Auth/AuthHostExtensions.cs:90
opts.ClientId = builder.Configuration["ServiceAuth:AuthClientId"]
    ?? (builder.Environment.IsIntegration() ? null!
        : throw new InvalidOperationException("ServiceAuth:AuthClientId is required."));
```

Three properties of that shape are what cost the eight hours:

1. **It fails on the first key touched**, not on all of them. Defect #1 (`Connection string 'asb' is
   required`) and defect #2 (`ServiceBus:ServiceName is required`) were the *same* substitution bug
   surfacing one key at a time, across two separate 25-minute queue runs.
2. **It is invisible to tooling.** Nothing can enumerate "what does this host require?", so every
   assertion about supplied configuration has to be hand-written and hand-remembered — which is what
   `CompositionTestArguments` is, and why it did not help.
3. **It runs lazily**, so the failure surfaces wherever the first resolution happens to be, not at boot.

`ValidateOnStart()` fixes all three, and **the repo already has the idiom** —
`api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Extensions/ServiceCollectionExtensions.cs:47-59`
does it three times with `IValidateOptions<T>` validators. So this extends an in-repo precedent rather
than importing a pattern.

The payoff that makes it more than tidying: with requirements declared this way, **G1 needs no per-key
assertions at all**. It builds each host's graph, resolves `IStartupValidator`, calls `Validate()`, and
gets every missing key at once. A newly-added required key is covered the day it is added, by nobody
remembering anything.

**One finding the brief does not mention, which sharpens *why* integration tests passed throughout.** The
brief says the integration tier "supplies its own configuration". It is stronger than that: production
host code contains **20 configuration escape hatches across 8 host files** that make missing configuration
*legal* in the Integration environment —

```
Auth/AuthHostExtensions.cs:88,91,109        B2B.Web/B2BWebHostExtensions.cs:124,127,141,144
B2B.Workers/ServiceCollectionExtensions.cs:68,71                 B2B.Seed.Simulator/HostExtensions.cs:27
Customer.Web/CustomerWebHostExtensions.cs:73,76,89,92            Payment.Web/HostExtensions.cs:69,72
Payment.Workers/HostExtensions.cs:40,43                          Search.Workers/HostExtensions.cs:28,31
```

(plus 4 behavioural `if (!IsIntegration())` gates). The integration tier is not merely *unlikely* to catch
a missing-config defect — it is **structurally incapable** of it, by explicit design, in production code.
That is also a test-induced seam in production behaviour, worth flagging on its own terms.
`ValidateOnStart` plus a per-environment `IValidateOptions<T>` keeps the integration tier's convenience
without the production `null!`.

### 4.3 Fix the two assertions currently protecting live defects

§5.3 and §5.4. Both are small, and both need doing *before* G4/G5, because both currently pin the
defective state as correct.

### 4.4 De-duplicate the architecture/startup test helpers

`AssertImageEndpoint`, `AssertContainerRuntimeArgs` and `AssertUsesDeveloperCertificate` are
**copy-pasted verbatim into four suites** (B2B, Customer, Payment, Search) — including the defective
default in §5.3, four times. They belong in `Concertable.Testing.Architecture` (or its Startup successor)
once. Same for the two divergent `PinHttpsEndpoint` implementations (§5.5).

### 4.5 Then, if you want it: the tier rename

`api/TestConventions.targets` gains `.StartupTests` via `EndsWith`, tested before `.E2ETests`' `Contains`.
Six projects rename, their classes shed the stutter, `AssemblyTrait("Category", "Startup")` lands, and CI
gains a `startup-tests` job wired as a `needs:` of `e2e-api-tests` exactly where `architecture-tests` sits
today.

Deliberately last. It improves how the suites read and it makes the tier a real decision, but **it buys no
gate.** Everything in §3 can run on the PR today inside the existing `*.ArchitectureTests` projects.

---

## 5. Docs-vs-reality defects found

### 5.1 The `composition-testing` skill documents a project layout that does not exist

The `dotnet:composition-testing` skill describes:

- *"A `*.CompositionTests` project proves what `ValidateOnBuild` cannot"* — **there are zero
  `*.CompositionTests` projects in this repo.** Verified by enumerating every test `.csproj` under `api/`.
- *"`AppHostCompositionTests.Inventory_AllExecutableProjectsDeclareCoverageOrExclusion`"* — the test
  exists but is `AppHostArchitectureTests.Inventory_AllExecutableProjectsDeclareCoverageOrExclusion`
  (`api/tests/Concertable.AppHost.ArchitectureTests/AppHostArchitectureTests.cs:32`). Wrong class name.
- *"Each service owns and carries its own composition project"* — each service carries its own
  `*.ArchitectureTests` project, which holds this alongside genuine architecture tests.
- *"A fourth test tier beside unit, integration and E2E"* — the fourth tier
  `api/TestConventions.targets` recognises is `Architecture`. A `.CompositionTests` project would **fail
  the build**, because a test project whose name states no recognised tier is a hard error.

So the skill documents an aspiration as though it were the implementation. This matters more than a stale
doc usually would: an agent reading it will try to create `*.CompositionTests`, hit the tier gate, and
have to reverse out. Either the skill is corrected to describe `*.ArchitectureTests` as the current home,
or to describe `*.StartupTests` once §4.5 lands. It should not keep describing `*.CompositionTests`.

### 5.2 No architecture suite declares the documented assembly trait

`dotnet:unit-testing` states each test project carries `[assembly: AssemblyTrait("Category", "<Tier>")]`.
Verified across the repo: 22 `Unit`, 18 `Integration`, 2 `Api`, 2 `Ui`, 1 `Mobile` — and **zero
`Architecture`**. None of the six architecture projects has an `AssemblyInfo.cs` at all. Test Explorer
therefore cannot group them, which is presumably why they are easy to forget.

### 5.3 [Question 5 — confirmed] `AssertImageEndpoint`'s default asserts defect #4's condition as correct

Confirmed, and worse than the brief states, because the source it guards is also still wrong.

```csharp
// api/Concertable.B2B/tests/.../B2BHostGraphTests.cs:117 and CustomerArchitectureTests.cs:60
AssertImageEndpoint(validBuilder, PaymentConstants.WebResource, "https");   // scheme defaults to "http"
AssertImageEndpoint(validBuilder, PaymentConstants.WebResource, "http");
```

with `private static void AssertImageEndpoint(..., string scheme = "http")` asserting
`Assert.Equal(scheme, endpoint.UriScheme)`. So it asserts that the endpoint **named `https` has
`UriScheme == "http"`**.

The source it is pinning:

```csharp
// api/Concertable.B2B/src/Concertable.B2B.AppHost/AppHost.cs:31-32 (identical at Customer:34-35)
.WithHttpEndpoint(targetPort: 8080, name: "https")
.WithHttpEndpoint(targetPort: 8080, name: "http")
```

Two endpoints, same target port, one of them named `https` and created by `WithHttpEndpoint`. And that
name is load-bearing: `AddPaymentWeb` builds `Auth__Authority` from `auth.GetEndpoint("https")`, and
`AddStripeCli` forwards webhooks to `paymentWeb.GetEndpoint("https")` in run mode — so a consumer asking
for the `https` endpoint gets a plaintext URL.

**Recommended fix, in order:**

1. **Do not just pass `scheme: "https"`.** The test would then fail, correctly, because the resource
   genuinely serves plaintext. The assertion is not the defect; it is the defect's alibi.
2. Fix the source so name and scheme agree. Either the Payment image serves HTTPS on 8080 and the
   declaration becomes `WithHttpsEndpoint(targetPort: 8080, name: "https")`, or it does not and the
   endpoint is named `http` with every consumer's `GetEndpoint("https")` following it.
3. *Then* delete the `scheme` parameter's default entirely, so the caller must state the scheme and G4
   makes name↔scheme agreement structural rather than per-call.

### 5.4 Defect #4 is still live in all four standalone AppHosts

This is the finding I did not expect and it is not in the brief.

`2aba5fc2c`'s own message says it: *"The AppHosts keep the pinned image, so the RT3 cut-over is
unchanged."* The fix ran Auth from source **in the E2E stack only** — `PinAuthService` substitutes the
container for a `ProjectResource` and leaves the original carrying `ExplicitStartupAnnotation`, so it
never starts.

But every standalone AppHost still declares, in run mode:

```csharp
// B2B:29, Customer:31, Payment:20, Search:23 — all four, identically
builder.AddAuth(AuthImage, AuthDigest, authDb, asb)
       .WithContainerRuntimeArgs("--user", "root")
       .WithHttpsEndpoint(targetPort: AuthConstants.ContainerPort, name: "https");
```

against an image the commit message states *"cannot serve the https endpoint the E2E contract requires"*,
with `WithHttpsDeveloperCertificate()` which — per that same commit — *"never reaches that container"*.
`AGENTS.md` says standalone AppHosts are canonical, so `dotnet run` on any of the four should hit defect
#4 or #5 today.

And the architecture suites **assert this arrangement as correct**:
`AssertImageEndpoint(validBuilder, AuthConstants.Resource, "https", scheme: "https")` plus
`AssertUsesDeveloperCertificate(...)`, in all four suites.

I have not run a standalone AppHost to confirm the failure — that would mean booting the stack, which this
pass is not doing. So: **the declaration is provably the same one that failed in E2E, and the assertions
provably pin it. Whether `dotnet run` fails today is unverified.** It should be checked before G5 is
written, because G5's rule depends on the answer.

### 5.5 Two divergent `PinHttpsEndpoint` implementations

- `Concertable.Testing.E2E.DistributedApplicationBuilderExtensions.PinHttpsEndpoint` (line 251): adds an
  `https` endpoint *only if absent*, then **mutates** every existing one — `Port = port`,
  `IsProxied = false`. Preserves `TargetPort`.
- `Concertable.Search.E2ETests.Helpers.DistributedApplicationBuilderExtensions.PinHttpsEndpoint`
  (line 102): **removes** every `https` endpoint and re-adds via
  `WithHttpsEndpoint(port: port, isProxied: false)`. **Drops `TargetPort`.**

Same name, same nominal job, materially different results for a container-backed resource — and defect #3
was a `Port`/`IsProxied` bug in this exact area. One of these is redundant.

### 5.6 Twelve duplicated image/digest constants with no agreement gate

`AuthImage`/`AuthDigest` appear in all four standalone AppHosts; `PaymentWeb*`/`PaymentWorkers*` in two;
`B2BSeedingSimulator*` in two. They agree today (verified). Nothing makes them agree tomorrow, and
`Assert-PinnedImagesPullable` reads only B2B's copy — so a stale digest in Customer's AppHost would pass
the local preflight and fail in the queue. G8.

### 5.7 The E2E port contract is duplicated with no gate

`api/Concertable.B2B/src/Concertable.B2B.Web/appsettings.E2E.json` and
`api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests/appsettings.E2E.json` carry byte-identical
`Endpoints` blocks (7086/7087/7088/7083 plus SPA ports). Two copies of the contract the tests dial by
literal URL, and nothing asserts they match — or that they match what the AppHost publishes. B2B (708x)
and Customer (709x) are correctly disjoint today.

### 5.8 `*ArchitectureTests` classes in `*.UnitTests` projects

Listed in §1.1. Four architecture-technique classes and one convention class live in unit projects. Two of
them (`TypedResultArchitectureTests`, `RepositoryArchitectureTests`) do file-system scans across
`api/*/src/`, which is not a unit test under any reading of the tier gate's own error message ("a test
needing a host, HTTP or a database is an integration test" — these need the repository on disk).

---

## 6. What I could not settle — questions for you

1. **Rename now, or gate now and rename later?** §1.2 recommends `*.StartupTests`, but
   `architecture-tests` already runs on PRs and already blocks `e2e-api-tests`, so every gate can land
   today with no tier-gate or workflow edit. Rename as a precondition, or as a follow-up once green?

2. **Does `dotnet run` on `Concertable.B2B.AppHost` actually work right now?** §5.4 shows the same
   HTTP-only-image-declared-as-https arrangement that failed in E2E, still in all four standalone
   AppHosts, with the architecture suites asserting it. I did not boot one. If it *does* work, my model of
   defect #4 is incomplete and G5's rule needs rewriting. If it does not, that is a live bug on `main` in
   the canonical dev entry point.

3. **Is the Payment image's `https`-named-but-plaintext endpoint deliberate?** §5.3's fix depends on it.
   If the image genuinely serves only HTTP on 8080, the honest fix renames the endpoint and updates
   `Auth__Authority` and the Stripe CLI forward URL — which changes what consumers dial. If the image
   should serve HTTPS, the fix is in the image. I can't tell which from the tree.

4. **One Startup project per service, or split app-model from host-startup?** §1.2 proposes one project
   with `ResourceGraphTests` + `WebHostTests` + `WorkerHostTests`. The alternative is two projects
   (`*.AppModelTests` and `*.StartupTests`) on the grounds that "what the graph supplies" and "what the
   host requires" are different questions. I think that over-splits — the gate that matters (G1) is
   precisely their *intersection*, and it needs both in one place — but it is a judgement call.

5. **How far does `ValidateOnStart` adoption go in one pass?** §4.2 is the highest-value change and it
   touches 20 escape hatches across 8 production host files, i.e. real production behaviour in every
   service. Options-pattern-per-host is a clean multi-PR cut-over; a single pass would be a large, wide
   diff. Which shape do you want?

6. **Where should the "does this image actually answer TLS" check live?** §3 proposes folding it into
   `publish-images.yml` at build time and recording the answer beside the digest, so PR-time gates stay
   static. That means a new recorded artifact (image → schemes/ports) and somewhere to keep it. If you'd
   rather not carry that, #4 stays uncatchable before a boot.

7. **`Concertable.E2E.Source` is excluded from composition coverage as a "source provider", but under
   `UseSourceComposition=false` there is no `IE2EHostProvider` implementation at all** — the harness
   compiles and `E2EHosts.Source()` throws at runtime. Fine while the carved mode only runs the helper
   unit tests (what the Stage-4 progress note records). Intended end state, or is a package/image-backed
   provider still planned? The answer decides whether the seam in §1.3 should keep `CreateBuilderAsync`
   and the seven `IProjectMetadata` members on one interface.

8. **Where should a research document like this actually live?** `review-lifecycle` owns
   `reviews/<branch-slug>.md` and defines it as a code-review work order with a frozen candidate
   identity and `[ ]` findings. This file is neither, and it collides with that path. If a review pass
   later lands on `Chore/TestTierNaming` it must append rather than overwrite. `plans/` or a
   `research/` sibling may be the correct home for this shape of document.
