# UI E2E Test Plan (Reqnroll + Playwright + Aspire)

## Goal

A Reqnroll + Playwright UI E2E suite that drives the Concertable SPA through every concrete concert workflow end-to-end as multi-actor narratives (Venue Manager → Artist → SignalR → DB state → Stripe state). Two purposes:

1. Replace tedious manual click-through-as-3-roles testing during current development.
2. Permanent regression armour for the workflow surface as the SPA refactor lands.

All three E2E projects (Common + Api + Ui) live under a single `api/Tests/Concertable.E2ETests/` parent folder, mirroring the `Modules/<Domain>/` cohesion pattern: Common is private infra for Api+Ui, neither sibling makes sense without it. UI tests are a new Reqnroll project. API E2E tests stay xUnit-direct, renamed from `Concertable.Web.E2ETests` to `Concertable.E2ETests.Api`.

## Decisions locked in

- **Reqnroll for UI E2E** — multi-actor narratives (role transitions every few steps, SignalR events mid-flow, 4-strategy × N-outcome matrix) make Gherkin's structural reuse + readable subject prefixes worth the framework cost.
- **xUnit for API E2E** — assertions are single-subject (HTTP request → assert state). Gherkin adds no value here.
- **Class library `E2ETests.Common`** for shared fixtures (already extracted — Step 1 complete).
- **Reqnroll, not SpecFlow** — SpecFlow is unmaintained and lacks .NET 10 support; Reqnroll is the OSS fork by the original SpecFlow author, API-compatible.
- **C#, not TypeScript Playwright** — the C# fixture reuse (typed SeedDataResponse, in-process DB assertions, Aspire orchestration) wins over TS codegen DX. Use `npx playwright codegen --target csharp` for locator discovery only.
- **Authentication: real OIDC login per role, captured to `storageState` once per test run, hydrated per-scenario.** No ROPC backdoor. `Concertable.Auth` runs in Aspire E2E mode unmodified. One dedicated `Login.feature` covers the real redirect dance.
- **Build from scratch** — happy-path scenarios drive the *entire* workflow in the UI starting from no state, rather than starting from a pre-seeded `Pending*App`. Half the value of UI E2E is proving the full chain works.
- **Chromium only for v1.** Multi-browser matrix deferred.

## Scope

**In v1:** Happy-path scenarios per concrete workflow strategy + customer ticket purchase + concert finish/settlement. Per-scenario DB reset + reseed.

**Out of v1:** Visual regression / screenshots. Mobile viewports. Accessibility audits. Failure branches (decline / 3DS fail / withdraw) — Phase 4.

## Workflow Matrix

Four concrete strategies in `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Workflow/`. The matrix is smaller than feared — UI scenario count is bounded.

| # | Strategy | Contract | Application subtype | Booking subtype | Payment phase | UI checkout pages traversed |
|---|----------|----------|---------------------|-----------------|---------------|-----------------------------|
| 1 | `FlatFeeConcertWorkflow` | `FlatFeeContract` (Fee) | `StandardApplication` | `StandardBooking` | Accept-checkout (immediate) | Artist apply (no payment) → VM accept (Stripe + 3DS) |
| 2 | `DoorSplitConcertWorkflow` | `DoorSplitContract` (ArtistDoorPercent) | `StandardApplication` | `DeferredBooking` | None at apply/accept; deferred to settlement | Artist apply (no payment) → VM accept (no payment) → settle later |
| 3 | `VersusConcertWorkflow` | `VersusContract` (Guarantee + ArtistDoorPercent) | `StandardApplication` | `DeferredBooking` | None at apply/accept; deferred | Artist apply (no payment) → VM accept (no payment) → settle later |
| 4 | `VenueHireConcertWorkflow` | `VenueHireContract` (HireFee) | `PrepaidApplication` (stores PaymentMethodId) | `StandardBooking` | Apply-checkout (artist authorises at apply) | Artist apply (Stripe + 3DS) → VM accept (no payment, captures held auth) |

Plus three cross-cutting flows that aren't strategies but are UI surface:

| # | Flow | Notes |
|---|------|-------|
| 5 | Customer ticket purchase | UI version of existing `TicketPurchaseTests.cs` API tests |
| 6 | Concert finish / settlement | Triggered via `/e2e/finish/{concertId}` to keep tests deterministic |
| 7 | OIDC login | Single dedicated scenario covering the real redirect dance — proves `storageState` capture is sound |

**Total v1 happy-path scenarios: 7** (4 workflows + customer + settlement + login).

## Project Structure

```
api/Tests/
└── Concertable.E2ETests/                              ✅ Steps 1–4 LANDED — parent classlib (was .Common)
    ├── Concertable.E2ETests.csproj                    (class lib, IsTestProject=false; <DefaultItemExcludes> excludes Api/Ui subfolders so the recursive compile glob doesn't grab inner project files)
    ├── AppFixture.cs                                  (Aspire boot, /e2e/reseed, JWT minting)
    ├── StripeCliFixture.cs                            (Docker stripe-cli + webhook secret)
    ├── SqlFixture.cs                                  (Respawn DB reset)
    ├── PollingService.cs / IPollingService.cs
    ├── TestTokenMinter.cs                             (used by Api project only)
    ├── MessageSinkLoggerProvider.cs
    ├── DistributedApplicationBuilderExtensions.cs     (port pinning + E2E env vars)
    ├── E2ETestCollection.cs                           (collection definition base)
    ├── GlobalUsings.cs
    │
    ├── Concertable.E2ETests.Api/                      ✅ Step 3 LANDED (rename of Concertable.Web.E2ETests)
    │   ├── Concertable.E2ETests.Api.csproj
    │   ├── AssemblyInfo.cs                            ([AssemblyTrait("Category","Api")])
    │   ├── CollectionRegistration.cs                  ([CollectionDefinition("E2E")] for this assembly)
    │   └── Payments/
    │       ├── ConcertDraftTests.cs
    │       ├── ConcertFinishedTests.cs
    │       └── TicketPurchaseTests.cs
    │
    └── Concertable.E2ETests.Ui/                       (NEW — Step 4)
        ├── Concertable.E2ETests.Ui.csproj             (Reqnroll.xUnit + Reqnroll.Microsoft.Extensions.DependencyInjection
        │                                               + Microsoft.Playwright + Microsoft.Playwright.Xunit)
        ├── reqnroll.json                              (Reqnroll config)
        ├── Hooks/
        │   ├── PlaywrightHooks.cs                     ([BeforeTestRun] launch browser; [BeforeScenario] new context; [AfterScenario] dispose)
        │   ├── AspireHooks.cs                         ([BeforeTestRun] boot AppFixture; [BeforeScenario] reset + reseed)
        │   └── LoginCaptureHooks.cs                   ([BeforeTestRun] real OIDC login per role → cache storageState)
        ├── Support/
        │   ├── UiFixture.cs                        (composes AppFixture + Browser/Playwright)
        │   ├── WorkflowState.cs                       (per-scenario typed state object — avoids ScenarioContext stringly-typed keys)
        │   ├── EnumTransformations.cs                 ([StepArgumentTransformation] for ContractType, BookingSubtype, etc)
        │   ├── PageObjects/
        │   │   ├── LoginPage.cs
        │   │   ├── VenueManager/
        │   │   │   ├── OpportunitiesPage.cs
        │   │   │   ├── ApplicationsPage.cs
        │   │   │   ├── AcceptApplicationPage.cs
        │   │   │   └── ApplicationCheckoutPage.cs
        │   │   ├── Artist/
        │   │   │   ├── OpportunityFindPage.cs
        │   │   │   ├── ApplyCheckoutPage.cs
        │   │   │   └── MyApplicationsPage.cs
        │   │   └── Customer/
        │   │       └── TicketCheckoutPage.cs
        │   └── Helpers/
        │       ├── StripeUiHelpers.cs                 (frame-locator helpers + Cards.Success/Cards.Decline/etc constants)
        │       ├── SignalRWaiter.cs                   (wait-for-event with PollingService)
        │       └── DbAssertions.cs                    (read EF entities post-flow)
        ├── Steps/
        │   ├── VenueManagerSteps.cs                   ([Binding] — thin, delegates to page objects)
        │   ├── ArtistSteps.cs
        │   ├── CustomerSteps.cs
        │   ├── SystemSteps.cs                         (Given/When for system events: SignalR, /e2e/finish, etc)
        │   └── AssertionSteps.cs                      (Then DbAssertions / StripePaymentIntents calls)
        └── Features/
            ├── Login.feature                          (Phase 1 smoke)
            ├── FlatFeeWorkflow.feature                (Phase 2)
            ├── DoorSplitWorkflow.feature              (Phase 3)
            ├── VersusWorkflow.feature                 (Phase 3)
            ├── VenueHireWorkflow.feature              (Phase 3)
            ├── CustomerTicketPurchase.feature         (Phase 3)
            └── ConcertSettlement.feature              (Phase 3)
```

## Reqnroll Conventions (NON-NEGOTIABLE — failure mode prevention)

These rules exist because BDD codebases consistently rot in predictable ways. Locking them in from day one.

### 1. Step definitions are thin one-liners

```csharp
// ✅ GOOD — step delegates to page object
[When(@"the venue manager accepts the application with a valid card")]
public async Task VmAcceptsWithValidCard()
{
    var page = await _pages.OpenApplicationCheckoutAsync(_state.ApplicationId);
    await page.PayWithCardAsync(Cards.Success);
}

// ❌ BAD — selector logic inside step
[When(@"the venue manager accepts the application with a valid card")]
public async Task VmAcceptsWithValidCard()
{
    var page = await _ctx.NewPageAsync();
    await page.GotoAsync($"/venue/application/checkout/{_state.ApplicationId}");
    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    var iframe = page.FrameLocator("iframe[name^='__privateStripeFrame']");
    // ... 30 more lines
}
```

If a step body is over ~5 lines, push the work into a page object or helper.

### 2. Scenario state via typed `WorkflowState`, never `ScenarioContext.Set("...")`

```csharp
// ✅ GOOD — typed properties, F12 navigates, rename works
public class WorkflowState
{
    public int OpportunityId { get; set; }
    public int ApplicationId { get; set; }
    public int? ConcertId { get; set; }
}

[Given(@"a venue manager has posted a (.*) opportunity")]
public async Task PostsOpportunity(ContractType contract)
{
    var opp = await _vmPages.PostOpportunityAsync(contract);
    _state.OpportunityId = opp.Id;
}

// ❌ BAD — magic string, runtime fails on typo
_ctx.Set(opp.Id, "opportunityId");
var oppId = _ctx.Get<int>("opportuntyId"); // typo — TestExecutionException at runtime
```

`WorkflowState` is registered scenario-scoped via `Reqnroll.Microsoft.Extensions.DependencyInjection` and constructor-injected into step binding classes.

### 3. Enum step parameters use `[StepArgumentTransformation]`

```csharp
// ✅ GOOD — Gherkin reads naturally, binding is typed
[StepArgumentTransformation]
public ContractType TransformContractType(string value) => value switch
{
    "flat fee"   => ContractType.FlatFee,
    "door split" => ContractType.DoorSplit,
    "versus"     => ContractType.Versus,
    "venue hire" => ContractType.VenueHire,
    _ => throw new ArgumentException($"Unknown contract type: {value}")
};

[Given(@"a venue manager has posted a (.*) opportunity")]
public async Task PostsOpportunity(ContractType contract) { /* typed */ }
```

A unit test in `Concertable.E2ETests.Ui` asserts that every `ContractType` enum value has a transformation row, catching new enum values that didn't get a transformation update.

### 4. Page objects encapsulate all selector + Playwright + Stripe iframe logic

Steps don't know what a `data-testid` is. Pages don't know what a `[When]` is. Steps call methods like `await page.PayWithCardAsync(Cards.Success)` and the page handles iframe walking, fill, submit, wait-for-redirect.

**Selector standard:**

- **`data-testid` everywhere we touch DOM in the SPA** — every element a page object interacts with gets a stable `data-testid="<feature>-<element>"` (kebab-case, feature-prefixed: `login-email`, `vm-opportunity-create`, `ticket-checkout-pay`). Add the attribute in the same PR as the page object that needs it.
- **Page objects use `page.GetByTestId(...)`**, not raw CSS — survives DOM/Tailwind churn, intent reads clearly.
- **Selector strings live as `private const` fields at the top of the page object** — never inline literals in methods, never `public` (consumers go through `SignInAsync`, not raw selectors).
- **Razor Pages on the Auth host (`/Account/Login`, `/Account/ForgotPassword`, etc) follow the same `data-testid` rule** — they're outside the SPA but still part of the UI surface page objects drive.
- Stripe iframe content is the unavoidable exception — selectors there target Stripe's stable element names (`[name='cardnumber']` etc), encapsulated in `StripeUiHelpers`.

Reference impl: `LoginPage.cs` + `Concertable.Auth/Pages/Account/Login.cshtml` (`login-email` / `login-password` / `login-submit`).

### 5. Background blocks for shared setup

Each `.feature` file uses `Background:` for the "fresh DB reseed + role auth state hydrated" setup that every scenario needs. Reduces scenario noise.

## Authentication Strategy

Drive the real OIDC login flow once per role at test-run start, capture each role's `IBrowserContext.StorageStateAsync()` to an in-memory dict, hydrate per-scenario contexts from that captured state.

Lifecycle:
- `[BeforeTestRun]` in `LoginCaptureHooks` — for each role (`VenueManager`, `ArtistManager`, `Customer`), spin up a one-shot Playwright context, navigate to `/login`, complete the redirect dance with seeded credentials (`SeedData.Customer.Email` + `SeedData.TestPassword` etc), call `context.StorageStateAsync()` → string, cache by role.
- `[BeforeScenario]` in `PlaywrightHooks` — read scenario tags (`@VenueManager`, `@Artist`, `@Customer`), create a fresh `IBrowserContext` with `BrowserNewContextOptions { StorageState = cachedStateForRole }`. Hydrated and ready to navigate.
- `Login.feature` — explicitly does NOT use the cached state; drives the real redirect end-to-end. Single scenario, runs once. If it fails, the cache mechanism is broken.

## Aspire Configuration

The SPA is already registered via `builder.AddNpmApp("frontend", "../../app", "dev")` in `Concertable.AppHost/Program.cs`. No new resources needed. Common's `DistributedApplicationBuilderExtensions.AddE2E` adds port pinning (Step 3 below) so SPA's `.env.development` URLs (`https://localhost:7086` for API, `https://localhost:7083` for Auth) match the test stack.

UI fixtures read the SPA URL via `DistributedApplication.GetEndpoint("frontend", "https")`.

## Per-Scenario Lifecycle

```
[BeforeTestRun once]
  AppFixture.InitializeAsync()       (Aspire boot, Stripe CLI, /health)
  LoginCaptureHooks.PopulateAuthCache()  (real OIDC login per role)

[BeforeScenario every scenario]
  AppFixture.ResetAsync()            (Respawn + /e2e/reseed → typed SeedData)
  WorkflowState                      (fresh per-scenario object via Reqnroll DI)
  IBrowserContext                    (hydrated from cached storageState by tag)

[Scenario steps execute]

[AfterScenario every scenario]
  IBrowserContext.DisposeAsync()
```

## Phased Implementation

### ✅ Step 1 — Extract `Concertable.E2ETests.Common` + green API baseline (LANDED)

What was originally one step grew (deliberately) to also fix the pre-existing red API suite — UI E2E needs a clean baseline to build on. **Final state: 12/12 API E2E tests green** (was 2/12 on master).

**Common class lib:**
- New class lib at `api/Tests/Concertable.E2ETests.Common/`
- Moved 8 infra files + collection definition + `appsettings.E2E.json` content include
- Slimmed `Concertable.Web.E2ETests.csproj` to ref Common
- Updated 3 test files' `using` directives
- Added `CollectionRegistration.cs` to consumer for xUnit collection lookup in test assembly
- Added Common to solution

**API test fixes (separate from extraction, found during checkpoint):**
- URL pattern: 4 sites `accept/{id}` → `{id}/accept` (after BE rename in commit `ff7555ec`)
- Body: FlatFee/DoorSplit/Versus accept tests now send `new { PaymentMethodId = AppFixture.TestPaymentMethodId }`; VenueHire sends null body
- TicketPurchase: added `PaymentMethodId` to all 4 test bodies
- 9 files: `pm_test` → `pm_card_visa` (Stripe's permanent test PM ID — convention adoption across seeders + integration + unit + builders + assertions)
- Removed `/e2e/payment-intent/{applicationId}` HTTP backdoor — replaced with reusable `DbAssertions.GetLatestSettlementPaymentIntentIdByApplicationIdAsync(connection, appId)` extension method that filters `WHERE PaymentIntentId LIKE 'pi[_]%'` (skips dev-seeded GUID rows)

**Reusable infrastructure improvements (apply beyond this fix):**
- `AppFixture.Sql` promoted to public auto-property (matches `ApiFixture.ReadDbContext` style)
- `SqlFixture.Connection` exposed for raw DB access in any future test
- `CompletionDispatcher` gained `ILogger<CompletionDispatcher>` + `LogError(ex, ...)` in catch block — surfaces full exception detail when `BadRequestException` / similar gets swallowed by result-conversion. Same pattern should apply to any future catch-all that converts exceptions to `Result.Fail(ex.Message)`.
- 3 tests migrated from raw `EnsureSuccessStatusCode` to `HttpResponseAssertions.ShouldBe(...)` for visible response body on assertion failures

### ✅ Step 2 — Port pinning + Auth env override (LANDED)
- `ApiBaseUrl` `http://localhost:7001` → `https://localhost:7086` (matches `.env.development`)
- Pinned Auth resource to `https://localhost:7083` via `EnvironmentCallbackAnnotation`
- URLs sourced from `IConfiguration` (`Endpoints:Api` + `Endpoints:Auth` in `appsettings.E2E.json`) — no hardcoded constants. ASPNETCORE_ENVIRONMENT=E2E means these never bleed into prod (prod uses Azure App Settings → IConfiguration).
- StripeCliFixture forwards via `--skip-verify` (HTTPS localhost dev cert)
- 12/12 green

### ✅ Step 3 — Rename `Concertable.Web.E2ETests` → `Concertable.E2ETests.Api` + nest both projects under `Tests/Concertable.E2ETests/` (LANDED)
- Renamed folder, csproj, namespaces, asm name; updated sln + 4 IVTs (Concert.Api, Concert.Application, Contract.Application, Payment.Application)
- Added `[AssemblyTrait("Category","Api")]` (alongside existing `Category=E2E`)
- Both Common + Api moved into nested parent `Tests/Concertable.E2ETests/` — mirrors `Modules/<Domain>/` cohesion pattern (Common is private infra, Api+Ui never used independently). Adjusted relative paths in csprojs (`..\..\` → `..\..\..\` for cross-tree refs).
- 12/12 still green

### Step 4 — Create `Concertable.E2ETests.Ui` Reqnroll project + `UiFixture` composition

**Composition pattern** (decided pre-Step 4):

```csharp
public class UiFixture : IAsyncLifetime
{
    public AppFixture App { get; }
    public IBrowser Browser { get; private set; } = null!;
    private IPlaywright playwright = null!;

    public UiFixture(IMessageSink messageSink)
    {
        App = new AppFixture(messageSink);  // composes existing fixture
    }

    public async Task InitializeAsync()
    {
        await App.InitializeAsync();        // boots Aspire, Stripe CLI, SqlFixture
        playwright = await Playwright.CreateAsync();
        Browser = await playwright.Chromium.LaunchAsync();
        // Reqnroll [BeforeTestRun] hook calls into this for OIDC login capture per role
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        playwright.Dispose();
        await App.DisposeAsync();
    }
}
```

`UiFixture` **composes** `AppFixture` rather than inheriting — keeps API-only fixture concerns separate from UI-only Playwright concerns. Tests reach API stuff via `fixture.App.Sql.Connection` etc; UI stuff via `fixture.Browser`. Clean cross-cutting access, no diamond inheritance, no leaky abstractions.

**Project setup:**
- New test project at `api/Tests/Concertable.E2ETests.Ui/`; refs Common
- Packages: `Reqnroll.xUnit`, `Reqnroll.Microsoft.Extensions.DependencyInjection`, `Microsoft.Playwright`, `Microsoft.Playwright.Xunit`
- `reqnroll.json` config
- Skeleton folders: `Hooks/`, `Support/PageObjects/`, `Support/Helpers/`, `Steps/`, `Features/`
- `UiFixture` (composition above) + `WorkflowState` + `EnumTransformations`
- Hooks: `AspireHooks` (BeforeTestRun init / AfterTestRun dispose), `PlaywrightHooks` (BeforeScenario context creation from cached storageState by `@VenueManager`/`@Artist`/`@Customer` tag), `LoginCaptureHooks` (BeforeTestRun real OIDC login per role → cache)
- One `Login.feature` smoke scenario — drives real OIDC login, asserts post-login URL
- DoD: `dotnet test --filter "Category=Ui"` runs Login scenario green.
- **Test Explorer trait hygiene:** Reqnroll's xUnit codegen hardcodes `[global::Xunit.TraitAttribute("FeatureTitle", ...)]` + `[Description, ...]` on every generated scenario, which duplicates the VS Test Explorer tree (one branch per trait). Suppressed via an MSBuild target `StripReqnrollFeatureTitleAndDescriptionTraits` in `Concertable.E2ETests.Ui.csproj` that runs `AfterTargets="GenerateFeatureFileCodeBehindFiles" BeforeTargets="CoreCompile"` and regex-strips both attributes from `*.feature.cs`. Result: only `[AssemblyTrait("Category", "Ui")]` survives, matching `Category=Api` symmetry. Headed Chromium is the local default (`Headless = CI=="true"`); CI will set `CI=true` automatically.

### Step 5 — FlatFee happy path
- Page objects for VM and Artist pages touched by FlatFee
- `FlatFeeWorkflow.feature` — happy-path scenario covering full chain
- Step bindings (thin) in `VenueManagerSteps`, `ArtistSteps`, `AssertionSteps`
- DoD: scenario passes deterministically; trace viewer artefact saved on failure

### Step 6 — Remaining happy-path workflows (one PR each, in order)
- DoorSplit (no Stripe at apply/accept — simplest after FlatFee)
- Versus (DoorSplit + extra contract field)
- VenueHire (apply-checkout — exercises new SPA work in `ApplicationCheckoutPage`)
- Customer ticket purchase
- Concert settlement (uses `/e2e/finish`)

### Phase 4 — Failure branches (deferred)
For each workflow with a payment step:
- Card declined (`Cards.Decline`)
- 3DS challenge failed (`Cards.Insufficient3ds`)
- User abandons checkout
For DoorSplit/Versus:
- VM rejects application

## Stripe-Specific Browser Concerns

- Stripe Elements iframes are cross-origin — Playwright handles via `page.FrameLocator("iframe[name^='__privateStripeFrame']")`. Encapsulated in `StripeUiHelpers`.
- 3DS challenge is a nested iframe — helper walks the iframe tree.
- Test cards: `Cards.Success = "4242424242424242"`, `Cards.Requires3ds = "4000002500003155"`, `Cards.Decline = "4000000000000002"`, `Cards.Insufficient3ds = "4000008400001629"`.
- Webhook timing: after `confirmPayment()` resolves on FE, BE may not have processed `payment_intent.succeeded` yet. Use `PollingService.UntilAsync(...)` to wait for BE state — wrapped in step `Then the payment succeeds` so the wait is part of the documented behaviour.

## CI Considerations (post-v1)

- UI tests run on push to `master` + nightly, NOT on every PR. API E2E + Unit + Integration on every PR.
- Trace viewer + video output on failure → CI artefacts.
- Browser install (`pwsh playwright.ps1 install chromium`) cached across CI runs.

## Open Questions (resolved)

- ~~ROPC vs UI login~~ — UI login per role, captured to storageState. Locked.
- ~~Build from scratch vs pre-seeded~~ — build from scratch. Locked.
- ~~Single project vs separate~~ — separate (`E2ETests.Common` + `.Api` + `.Ui`). Locked.
- ~~Reqnroll vs xUnit-direct~~ — Reqnroll for Ui, xUnit for Api. Locked.
- ~~Reqnroll vs SpecFlow~~ — Reqnroll. Locked.
- ~~TS vs C# Playwright~~ — C#. Locked.
- ~~Chromium-only v1~~ — yes. Locked.

## What this plan does not cover

- Step naming conventions (settled in Step 6 when first feature lands).
- Scenario tagging conventions (`@VenueManager` / `@Artist` / `@Customer` for auth hydration; rest decided as needed).

## Next Step

Step 4: Reqnroll project scaffolding at `Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/` + `UiFixture` composition + `Login.feature` smoke. Steps 1–3 all landed.
