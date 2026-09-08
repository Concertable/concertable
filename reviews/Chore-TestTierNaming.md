# Test-tier naming, and the gates that should stop configuration defects before E2E

> **This is a research recommendation, not a code-review pass.** It deliberately carries no
> `## Review pass` descriptor, no candidate base/head identity and no `[ ]` findings, because there is
> nothing to review: the branch is clean at merged `main` (`e1475b473`) with an empty diff, and the brief
> was explicitly read-only — decide, then implement. It occupies `reviews/Chore-TestTierNaming.md`
> because that is where the brief asked for it and it is the canonical slug for `Chore/TestTierNaming`.
> If a real review pass lands on this branch later, append it below under a proper
> `## Review pass` heading per `review-lifecycle`; do not retrofit this section into one. See §6.9.

Research pass over merged `main` (`e1475b473`) in the `Chore/TestTierNaming` worktree.

**Status: partly implemented.** §1.1/§1.2's tier split and §3's G1 gate are landed on this branch
(`fee52b4cf` and its successor). §4.2's `ValidateOnStart` conversion, and G1's extension to B2B and
Customer, are not — both are blocked on the defect in §5.4a, which the gate found the moment it existed.
Sections still describing something as a proposal say so; §5.4a and §5.4b record what landing it revealed.

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

### 5.4a What the G1 gate found the moment it existed — three hosts cannot start

G1 is implemented and landed for Auth, Search and Payment. Pointing it at B2B produced a defect in two
seconds that no existing tier could see, and it is the same root cause as §5.3 one layer along.

Aspire keys service-discovery configuration by an endpoint's **`UriScheme`, not its name**. So
`WithHttpEndpoint(targetPort: 8080, name: "https")` in the standalone B2B and Customer AppHosts produces
`services:payment-web:http:0` and `services:payment-web:http:1` — and never
`services:payment-web:https:0`, which is exactly the key `AddPaymentClient` requires and throws on
(`Concertable.Payment.Client/Extensions/ServiceCollectionExtensions.cs:13`). Resolved app-model keys for
`b2b-web`, verbatim from the gate:

```
--services:auth:https:0=...
--services:payment-web:http:0=...
--services:payment-web:http:1=...        <- no https:0 anywhere
```

`AddPaymentClient` is called by **b2b-web**, **b2b-workers** and **customer-web**. All three fail at startup
under `dotnet run` on either standalone AppHost. The only reason anything works is that the E2E harness sets
the key by hand in three places — `Concertable.B2B.E2ETests/DistributedApplicationBuilderExtensions.cs:66`
and `:89`, and the Customer sibling at `:68`. So the single path that exercises these hosts supplies what
the app model does not, which is the whole pathology in one line.

This **resolves §6.2 and §6.3 with evidence**: the standalone AppHosts are broken in run mode, and the
`https`-named plaintext endpoint is not deliberate — it buys nothing, because the name never reaches
service discovery.

Recorded as HIGH in `api/Concertable.AppHost.Shared/TECH_DEBT.md`, with the two real fix options and an
explicit warning not to switch the declaration to `WithHttpsEndpoint` (that would make the key appear and
every gRPC call over it fail at runtime — RT3's Auth TLS failure, moved along). B2B's and Customer's
`AppModelStartupContractTests` are deliberately **not** added yet: the gate is correct and the topology is
not, and asserting the broken state as correct is the mistake §5.3 already documents.

### 5.4b Two more things the gate surfaced

- **`AddStripeCli` puts a blocking side effect in the app model.** `AppHostExtensions.cs:135` attaches
  `paymentWeb.WithEnvironment(async ctx => ... await webhookSecret.Task.WaitAsync(TimeSpan.FromSeconds(60)))`.
  Resolving Payment's app-model configuration therefore blocks 60s awaiting a log line from a Stripe CLI
  process that is not running, then throws `TimeoutException`. The `composition-testing` skill's own rule is
  that "composition registration must remain side-effect-free"; this breaks it. Payment's gate pins
  `--Stripe:SecretKey=` empty so the resource is skipped deterministically, which is a workaround in the
  test, not a fix in the AppHost.
- **The secret residual is exactly 7 keys.** Everything the AppHosts forward only when already configured
  (`AddSecrets`, `WithOptionalEnvironment`): the three `ServiceAuth:*ClientSecret` values,
  `ServiceAuth:ClientSecret`, `Stripe:SecretKey`, `Stripe:WebhookSecret`, `ExternalServices:UseRealStripe`.
  That is the honest remainder §1.3 predicted `CompositionTestArguments` would shrink to — 7 secrets rather
  than 25 mixed secrets and topology keys. It lives in `AppModelConfiguration.Secrets`, and the comment there
  states the rule: a topology key added to that list hides the defect the tier exists to catch.

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

1. ~~Rename now, or gate now and rename later?~~ **Done — both, in that order.** The tier split landed
   first, then G1 on top of it.

2. ~~Does `dotnet run` on `Concertable.B2B.AppHost` work right now?~~ **Answered: no.** §5.4a has the
   evidence — b2b-web, b2b-workers and customer-web all throw at startup on a missing
   `services:payment-web:https:0`.

3. **THE ONE DECISION I NEED.** §5.4a proves the `https`-named plaintext endpoint is not deliberate and
   buys nothing, so it has to go. Which replacement do you want?
   **(a)** Give `Concertable.Payment.Client` a scheme-agnostic address key it owns — `Services:PaymentApiUrl`,
   matching the `Services:B2BApiUrl` / `Services:CustomerApiUrl` convention already in the AppHosts. Each
   AppHost then supplies whichever endpoint actually exists. Correct and convention-consistent, but it is a
   published-contract change: accept-new-with-fallback, publish, bump, migrate, delete the old key.
   **(b)** Run Payment from source in the standalone AppHosts, as E2E now does for Auth. One small edit,
   green immediately, but it gives back part of the RT3 image cut-over you just landed.
   I lean (a) — (b) re-opens RT3 — but (a) is three PRs and (b) is one, and that trade is yours.

4. ~~One Startup project per service, or split app-model from host-startup?~~ **Went with one per service.**
   G1 turned out to be exactly the intersection of "what the graph supplies" and "what the host requires",
   so splitting them would have put the only gate that matters across two projects.

5. **Should `AddStripeCli`'s blocking environment callback be fixed?** §5.4b: it makes resolving Payment's
   app-model configuration block 60s on a live process, against the `composition-testing` skill's own
   side-effect-free rule. Payment's gate pins the Stripe key empty to dodge it, which is a test-side
   workaround. Fixing it properly means the webhook secret arriving by some route other than an `await`
   inside `WithEnvironment`.

6. **How far does `ValidateOnStart` adoption go in one pass?** §4.2 is the highest-value change and it
   touches 20 escape hatches across 8 production host files, i.e. real production behaviour in every
   service. Options-pattern-per-host is a clean multi-PR cut-over; a single pass would be a large, wide
   diff. Which shape do you want?

7. **Where should the "does this image actually answer TLS" check live?** §3 proposes folding it into
   `publish-images.yml` at build time and recording the answer beside the digest, so PR-time gates stay
   static. That means a new recorded artifact (image → schemes/ports) and somewhere to keep it. If you'd
   rather not carry that, #4 stays uncatchable before a boot.

8. **`Concertable.E2E.Source` is excluded from composition coverage as a "source provider", but under
   `UseSourceComposition=false` there is no `IE2EHostProvider` implementation at all** — the harness
   compiles and `E2EHosts.Source()` throws at runtime. Fine while the carved mode only runs the helper
   unit tests (what the Stage-4 progress note records). Intended end state, or is a package/image-backed
   provider still planned? The answer decides whether the seam in §1.3 should keep `CreateBuilderAsync`
   and the seven `IProjectMetadata` members on one interface.

9. **Where should a research document like this actually live?** `review-lifecycle` owns
   `reviews/<branch-slug>.md` and defines it as a code-review work order with a frozen candidate
   identity and `[ ]` findings. This file is neither, and it collides with that path. If a review pass
   later lands on `Chore/TestTierNaming` it must append rather than overwrite. `plans/` or a
   `research/` sibling may be the correct home for this shape of document.

---

**Review status:** `complete`
**Reviewed up to commit:** `3f4f04a3bfa3669f106e971acf8df243dd8f8517`  `(2026-09-08)`
**Security-reviewed up to commit:** `3f4f04a3bfa3669f106e971acf8df243dd8f8517`  `(2026-09-08)`
**Judgment:** `approved`

## Review pass — 2026-09-06 — full

**Candidate base:** `b0be763edaf36026b8a28a8acc28475900737e4c`
**Candidate head:** `e3595f5c95f75cfa10a7511bf2db653679ca363b`
**Candidate branch:** `Chore/TestTierNaming`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:018ca73ecb663f9558da073ccb4323af81e9f7c10fdbf0c1125ce6522cb10dcc` `(78 paths)`
**Candidate bundle:** `C:/Users/TOMMYS~1/AppData/Local/Temp/claude/C--Users-TommySeery-source-repos-Concertable--worktrees-Chore-TestTierNaming/d223007b-4e38-4258-b5cc-0552d4273713/scratchpad/review-bundle`
**Candidate bundle identity:** `sha256:6117967e037c2a4f5a0596ef7618ca220262213bebd703771fa633366cff44b5`
**Work-order path:** `reviews/Chore-TestTierNaming.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

The candidate base is #633's head rather than `main`: this PR is stacked on
`Refactor/launch_deal-lifecycle-modules-phase2` and GitHub retargets it to `main` when #633 merges.
The implementation reviewed here was written by a session whose brief was read-only, so none of it was
taken on trust; the verification log separates what was executed from what was only read.

### Findings

- [x] **F1 — HIGH — ci** — `.github/workflows/test.yml:918,963`
  `Concertable.Auth` was the one service whose architecture project held nothing but startup content, so
  the branch renamed it to `Concertable.Auth.StartupTests` and Auth now has no `*.ArchitectureTests`
  project at all. An Auth-only PR therefore scopes `architecture_projects` to `[]`, `architecture-tests`
  skips on its own `if:`, and a skipped `needs:` cascades — `integration-tests` and `e2e-api-tests` would
  both silently skip, so an Auth-only change would merge having run neither its integration suite nor the
  merge queue's E2E gate. Every service had an architecture project before this branch, so the hole is
  new. Fixed by gating both jobs on no need having **failed**
  (`!cancelled() && !contains(needs.*.result, 'failure')`) rather than on all needs having run: ordering
  still holds, a failed cheap gate still stops the expensive one, and an inapplicable tier no longer
  suppresses a later one.

- [x] **F2 — HIGH — ci** — `eng/repository-split/inventory.json`
  The committed generated inventory was never regenerated for the six new projects or the Auth rename, so
  `python eng/repository-split/inventory.py --check` exited 1 and the `split-inventory` job would have
  failed the PR outright. This is RT3 defect #6 verbatim — the one the case study records as *"already
  gated correctly"* — reintroduced by the change that cites it. Regenerated.

- [x] **F3 — MEDIUM — ci** — `eng/repository-split/inventory.py:81,90`
  `classify()` knew four tiers and `*.StartupTests` fell through to `"runtime"`; `is_runtime_closure()`
  excludes anything under `/tests/`, so the new projects sat outside **both** cross-repository
  `ProjectReference` rules. The `split-inventory` job's own comment claims it catches *"no test-tier
  project declares a cross-repository ProjectReference"* repo-wide, and that claim was false for the very
  tier this branch introduces. `classify()` now returns `startup-test` and `TEST_KINDS` carries it, which
  moves the AppHost suite's cross-area edge back out of `apphost-support` and brings all six projects
  within `blockingTestEdges`' reach. No live violation exists today (`blockingTestEdges: []`) — the gate
  was simply blind to them.

- [x] **F4 — LOW — docs** — `docs/INDEX.md:158`
  The enforcement table credited *"`ExecutableHostInventory` + the `architecture-tests` CI matrix"* with
  proving every executable host has real coverage. The split moved the host graphs into `startup-tests`
  and left only the inventory behind, so the row named a matrix that no longer runs what it claimed.
  Corrected, and the row the new tier actually buys was added beside it.

- [wontfix] **F5 — MEDIUM — correctness** — `api/Concertable.*/tests/*.StartupTests/AppModelStartupContractTests.cs`
  Every case ends `app.Services.GetService<IStartupValidator>()?.Validate()`. That service exists only
  once something has called `ValidateOnStart()`, and the only three calls in `api/` are in
  `Concertable.Payment.Infrastructure` — so in Auth and Search the line asserts nothing, and the gate's
  real bite is the eager `?? throw` in each host's `Configure` lambda firing during `builder.Build()`.
  The null-conditional is deliberate forward-compatibility, but it also means deleting a
  `ValidateOnStart()` weakens the gate without turning anything red, which is the exact shape this tier
  exists to end. Tightening it to `GetRequiredService` needs research §4.2's options-pattern conversion
  across 20 escape hatches in 8 production host files, explicitly not in this change.
  Transferred: `api/Concertable.AppHost.Shared/TECH_DEBT.md`, LOW, with its resolution condition.

- [wontfix] **F6 — MEDIUM — duplication** — `api/Concertable.{Auth,Payment,Search}/tests/*.StartupTests/AppModelConfiguration.cs`
  Three byte-identical 49-line copies differing only in the namespace declaration — verified by hash. This
  is the file that decides what every gate is fed, so its `Secrets` allowlist can drift per service, and a
  topology key wrongly added to one copy blinds only that service while the other two still read correct.
  The branch logged the *smaller* duplication (`AssertImageEndpoint` and siblings) as debt and left this
  one silent. Deduplicating means moving it into `Concertable.Testing.Architecture`, a published package
  that would take a new `Aspire.Hosting` dependency — the same publish-then-consume two-step the existing
  entry describes, so it belongs beside it rather than in this PR.
  Transferred: `api/Concertable.AppHost.Shared/TECH_DEBT.md`, LOW, with its resolution condition.

- [wontfix] **F7 — LOW — tests** — `api/Concertable.B2B/tests/Concertable.B2B.StartupTests/ResourceGraphTests.cs:150`
  `AssertImageEndpoint` asserts `Assert.Equal(8080, endpoint.TargetPort)` against a literal where
  `AuthConstants.ContainerPort` is the source of truth for the Auth call site, and its
  `Assert.Equal(endpointName, endpoint.Name)` is redundant after the `Assert.Single(..., e => e.Name ==
  endpointName)` predicate above it. Left as-is deliberately: the helper serves both Auth and
  `payment-web`, and the AppHost itself writes `8080` as a literal for `payment-web` (there is no
  `PaymentConstants.ContainerPort`), so deriving only the Auth half would leave one shared helper
  inconsistent with itself. Resolves with F6, when the helper gets one home and can take the port per
  resource.

- [wontfix] **F8 — LOW — docs** — external repository `Concertable/agent-standards`
  Two skills go stale the moment this merges. `dotnet:unit-testing` documents the tier table with four
  rows and states *"`EndsWith` is tested before `Contains`"*, so a `.StartupTests` project reads as a
  build failure to any agent consulting it. `dotnet:composition-testing` names
  `AppHostCompositionTests.Inventory_AllExecutableProjectsDeclareCoverageOrExclusion`, which this branch
  renames to `InventoryTests.AllExecutableProjects_DeclareCoverageOrExclusion` — the skill was already
  wrong about the class name (research §5.1) and this makes it wrong about the method too. Both live in a
  separate repository and cannot be fixed from this PR; per the owner's decision they are recorded here
  rather than landed as a companion PR, and they must not be updated before this merges, or the skills
  would describe a tier that does not exist yet.

- [wontfix] **F9 — HIGH — live defect, not introduced here** — `api/Concertable.B2B/src/Concertable.B2B.AppHost/AppHost.cs:31`, `api/Concertable.Customer/src/Concertable.Customer.AppHost/AppHost.cs:34`
  Research §5.4a's claim is **confirmed by execution**, not merely statically. A temporary B2B
  `AppModelStartupContractTests` fed from the standalone B2B app model failed both cases with
  `System.InvalidOperationException : Payment service address (services:payment-web:https:0) is not
  configured.` — b2b-web in 2 s, b2b-workers in 7 s. `WithHttpEndpoint(targetPort: 8080, name: "https")`
  sets `UriScheme` to `http` whatever the name says, so Aspire emits `services:payment-web:http:0` and
  `:http:1` and never the key `AddPaymentClient` requires. The gate is correct and the topology is not,
  which is why B2B's and Customer's contract tests are deliberately absent rather than written against the
  broken state — asserting a defect as correct is precisely the `AssertImageEndpoint` mistake research
  §5.3 documents.
  It cannot be repaired in this PR: `AddPaymentClient` ships inside the published
  `Concertable.Payment.Client` package, which B2B and Customer consume by `PackageReference`, so changing
  the key it reads needs publish-then-bump — two PRs minimum, three to retire the old key. The one-PR
  alternative (running Payment from source in the standalone AppHosts, as `PinPaymentWeb` already does
  for E2E) would require `Concertable.B2B.AppHost` to take a project reference on
  `Concertable.Payment.Web`, crossing the carve boundary the repository split exists to protect.
  Carried by: `api/Concertable.AppHost.Shared/TECH_DEBT.md`, HIGH, whose resolution condition is that
  B2B's and Customer's `AppModelStartupContractTests` exist and pass without the E2E harness's three
  manual `services__payment-web__https__0` overrides.

### Verification log — executed, not read

- **Full solution build** — `dotnet build api/Concertable.slnx -c Release`: 0 errors.
- **All eleven suites green** — 31 tests across six `*.StartupTests` (Auth 3, B2B 10, Customer 4,
  Payment 6, Search 6, AppHost 2) and 40 across five `*.ArchitectureTests` (B2B 22, Customer 1,
  Payment 9, Search 7, AppHost 1).
- **The tier gate classifies correctly** — `dotnet build -getProperty:ConcertableTestTier` returns
  `Startup` for the sampled `.StartupTests` projects, `Architecture` for `B2B.ArchitectureTests`, and
  `Unit` for `Concertable.Search.E2ETests.Helpers.UnitTests`, so the new `EndsWith` did not disturb the
  `EndsWith`-before-`Contains` ordering the targets file depends on.
- **G1 bites** — deleting `auth.WithEnvironment("ServiceAuth__AuthClientId", ...)` from the Auth AppHost
  made `WebHost_StartsOnTheConfigurationTheAppModelSupplies` fail in 65 ms with
  `System.InvalidOperationException : ServiceAuth:AuthClientId is required.` That is RT3 defect #8
  exactly, which originally surfaced as all 32 UI scenarios dying on a `:7086/health` timeout. Reverted.
- **`AssertImageEndpoint` bites** — changing `payment-web`'s first endpoint to `WithHttpsEndpoint` made
  B2B's `ResourceGraphTests` fail with `Expected: "http" / Actual: "https"`. The helper's `scheme`
  parameter is required (the `= "http"` default that asserted defect #4's condition as correct is gone)
  and all nine call sites across the four suites pass the truthful value. Reverted.
- **§5.4a reproduced** — see F9.
- **The `split-inventory` gate** — `inventory.py --check` exited 1 before F2 and exits 0 after.
- **Nothing was dropped by the split** — all 27 test methods across the six pre-split suites were located
  in their new homes (Auth 2, B2B 9, Customer 5, Payment 8, Search 4, AppHost 3), plus #633's
  `Web_MessageTopology_HandlesDurableCommandsWithoutSelfSubscriptions` carried into `WebHostTests`, and
  all seven architecture classes #633 touches still exist and still assert what they did.
- **Security review** — no HIGH or MEDIUM findings, scoped to this branch's 78-path delta against #633.

### Read but not executed

- Whether `dotnet run` on the standalone B2B/Customer AppHosts fails end to end. F9 proves those hosts
  throw on the configuration their app models supply, which is the same defect one layer in, but no
  AppHost was actually started.
- Research §5.6's count of duplicated image/digest constants. The claim of twelve is wrong — there are
  **20** across the four standalone AppHosts (B2B 6, Customer 8, Payment 2, Search 4), all agreeing today.
  The finding stands; the number does not.
- Research §5.5's two divergent `PinHttpsEndpoint` implementations — both confirmed present, at
  `Concertable.Testing.E2E/DistributedApplicationBuilderExtensions.cs:251` and
  `Concertable.Search.E2ETests.Helpers/DistributedApplicationBuilderExtensions.cs:102`. Their behaviour
  was not differentially exercised.
- `.agents/hooks/docs_reachability.py` reports 10 errors, none in this branch's paths — all six new
  project directories carry both `AGENTS.md` and `CLAUDE.md`. Pre-existing on the base, so not carried
  here.

## Review pass — 2026-09-06 — incremental

**Candidate base:** `d65161935d239b8130ac7cc1cd4082ca10ac2d12`
**Candidate head:** `a50f2e89c0f92d4579c3ba3fc71287b0c27b8a74`
**Candidate branch:** `Chore/TestTierNaming`
**Candidate scope:** `.github/workflows/test.yml`
**Candidate path-set:** `sha256:faff1af3d8ff408964a57b2e475f69a6b7c7b71c9978cccc8f471798caac2c88` `(1 path)`
**Candidate bundle:** `(single-path delta; reviewed directly from the frozen commit)`
**Candidate bundle identity:** `(not materialized — one file, one hunk)`
**Work-order path:** `reviews/Chore-TestTierNaming.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Opened by the first pass's own remote-validation step: the PR was created, a monitor was bound to its
checks, and it reported nothing for an hour. The cause was not this branch's `startup-tests` job.

### Findings

- [x] **F10 — HIGH — ci** — `.github/workflows/test.yml:7`
  On `pull_request` the `branches` filter matches the **base** branch, so `branches: [main]` meant the CI
  workflow never fired at all on a PR stacked on another PR. Confirmed against the live repository rather
  than inferred: `gh pr checks` returns *"no checks reported"* for **all four** PRs currently based on
  `Refactor/launch_deal-lifecycle-modules-phase2` — #942, #946, #947 and #948. No build, no carve, no
  unit, no architecture, no integration, no `split-inventory`. Four PRs would merge into #633 wholly
  unvalidated and only be checked when #633 itself reached the queue, on top of a 1052-file refactor —
  the find-it-late-at-maximum-cost pattern the tier in this same PR exists to end, one level up.
  This branch did not cause it, but it is the branch that exposed it and cannot be validated remotely
  until it is fixed, so it is repaired here rather than deferred. The filter is dropped from
  `pull_request` only; `push` and `merge_group` keep theirs, since only `main` is push-validated and the
  merge queue only ever targets `main`. E2E is unaffected, being gated on
  `github.event_name == 'merge_group'`, so a stacked PR gets the intended PR tier — build, carve, unit,
  architecture, startup, integration — and not the 25-minute suite.

## Review pass — 2026-09-08 — incremental

**Candidate base:** `a50f2e89c0f92d4579c3ba3fc71287b0c27b8a74`
**Candidate head:** `288330fa31623d8382379f5d0e711ad172bdf731`
**Candidate branch:** `Chore/TestTierNaming`
**Candidate scope:** branch-authored paths with a net change since the watermark
**Candidate path-set:** `sha256:bf2c4592eb8be42a…` `(11 paths)`
**Candidate bundle:** `…/47c488ba-e13e-4022-8eee-3ff69fbf32e6/scratchpad/rev-bundle`
**Work-order path:** `reviews/Chore-TestTierNaming.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`
**Remediation head:** `3f4f04a3bfa3669f106e971acf8df243dd8f8517` — all five findings fixed in this pass.

Opened because the merge gate refused the merge on a stale watermark. The delta is the seeding
identity-rewrite unit coverage plus two main merges; `.github/workflows/test.yml` has **no net change**,
a CI ordering commit and its revert cancelling exactly.

Layers: native/general, conventions, changed-behaviour test impact, and the security layer (the PR's own
sensitive surface is Auth/Payment test projects). Of the 46 sensitive paths the gate sees since the
watermark, **44 are main-inherited** and reviewed on their own PRs; the 2 PR-owned ones
(`Concertable.Auth.StartupTests/ResourceGraphTests.cs`, `WebHostTests.cs`) arrived via merge `288330fa3`
carrying #943's SPA-client assertions, which tighten posture (`RestrictToEnabledClients`, stale redirect
URIs and CORS origins asserted absent). Security layer: **no qualifying vulnerability.** The rewriter's
emitted table name clears two barriers — the capture class `\[?[\w]+\]?` admits no interior `]` or `;`,
and every emitted name must be an allowlisted `IModel` key — and `SeedingScope.Activate()` has exactly
three callers, all startup-gated outside Production.

### Findings

- [x] **F11 — MEDIUM — correctness** — `SeedingIdentityRewriterTests.cs`
  `Assert.Single(this.identityTables.Keys, key => key == "[Animals]")` was unfalsifiable for the property
  its name claims: `Dictionary.Keys` is distinct by construction, so no predicate can ever match twice, and
  the zero case was already covered by the indexer on the preceding line. Dropping `e.BaseType is null`
  from `BuildTableMap` surfaces as a duplicate-key throw in the test constructor — a fixture crash, not
  this assertion failing. Now asserts the whole key set.

- [x] **F12 — MEDIUM — correctness** — `SeedingIdentityInterceptor.cs`
  The extraction moved the `Contains("INSERT")` short-circuit behind `tableCache.GetOrAdd`, so an active
  seeding scope's first intercepted command of any kind built the model map instead of its first INSERT.
  Restored, so the extraction is behaviour-preserving apart from the deliberate throw.

- [x] **F13 — MEDIUM — test-coverage** — `SeedingIdentityRewriterTests.cs`
  Three mutations survived the original nine tests, each boot-fatal in production: `ToHashSet`→`ToList`
  (a legal two-statements-one-table command becomes the throw), `Off()` dropping the schema (leaves
  `IDENTITY_INSERT` on for the rest of the connection — the *primary* path, since every module sets
  `HasDefaultSchema`), and the per-match column list reverted to a whole-command scan (makes the throw fire
  on `ConcertDevSeeder`'s real shape, where `Applications` takes a generated identity while the
  `BookingEntity` its navigation drags into the same save supplies an explicit one). Four tests added;
  each mutation now fails exactly one. Also covers EF's `MERGE` with `OUTPUT` plus a declared table
  variable, and the throw across an INSERT/MERGE pair rather than only INSERT pairs — what EF actually
  emits for a TPH batch.

- [x] **F14 — LOW — docs** — `api/Concertable.Shared/TECH_DEBT.md`
  Item 1 of the seeding entry still read "has *zero* coverage today" for a class this same change gave
  thirteen tests, which `docs-and-debt` forbids ("once the debt is addressed, delete the entire entry").
  Item 1 deleted, **Resolves when** narrowed to items 2-3; items 2-4 stay genuinely open.

- [x] **F15 — LOW — hygiene** — `Concertable.Seed.Shared/AssemblyInfo.cs`
  Gained a UTF-8 BOM from a `utf-8-sig` write over a file that had none, rewriting an untouched line.
  Stripped; that file's diff is now the one added `InternalsVisibleTo`.

### Verification

Executed, not asserted. 13/13 unit tests pass in ~0.9s; full `api/Concertable.slnx` build clean, 0 errors
(6 pre-existing CS8632 in Reqnroll-generated E2E temp files, present on the base). Each of the three F13
mutations was applied and shown to fail exactly one test, and each was reverted and re-verified green.

One process note worth keeping: the F13 `Off()` mutation **silently failed to apply** on its first run —
the replacement pattern did not match and nothing checked that it had, so the run reported all-pass and
looked like evidence the gate did not bite. Re-run with an assertion that the mutation landed, it fails one
test. A sabotage harness without a did-it-apply check is the `AssertImageEndpoint` mistake in the
verification step rather than the code.
