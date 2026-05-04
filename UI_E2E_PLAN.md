# UI E2E Test Plan (Reqnroll + Playwright + Aspire)

## Goal

A Reqnroll + Playwright UI E2E suite that drives the Concertable SPA through every concrete concert workflow end-to-end as multi-actor narratives (Venue Manager → Artist → SignalR → DB state → Stripe state). Two purposes:

1. Replace tedious manual click-through-as-3-roles testing during current development.
2. Permanent regression armour for the workflow surface as the SPA refactor lands.

## Status: scaffolding complete, escrow landed, FlatFee workflow next

**Scaffolding PR (#35) merged** + **Escrow PR (#36) merged** + escrow follow-up commits on master. Reqnroll + Playwright + Aspire stack wired up end-to-end. `Login.feature` smoke proves the real OIDC redirect dance works. Money flow now goes through escrow (auth at accept → capture/transfer at settle) — UI E2E built against this final shape, no rework expected.

**Escrow's impact on UI E2E (now baked in, not deferred):**
- `EscrowStatus` enum + `IPaymentSucceededProcessor` keyed by transaction type now own the money flow
- For FlatFee/VenueHire (immediate-payment workflows): VM/Artist authorises Stripe payment at accept time; funds sit in platform balance under escrow; Concert.SettleAsync triggers transfer to recipient at concert finish
- Mid-test assertions: at accept time, `PaymentIntent.transfer_data.destination` is platform (not artist/venue); at settle, a separate Stripe `Transfer` lands
- Settlement scenarios for FlatFee/VenueHire are now meaningful (real transfer to recipient), not just status flips
- Money assertion helpers (`Money.AssertArtistReceivedAsync` etc) abstract phase — Gherkin reads `Then the artist receives £500` regardless of when in the timeline that's true

**What's in place:**
- `Concertable.E2ETests` — shared classlib (AppFixture, StripeCliFixture, SqlFixture, PollingService, port pinning, `/e2e/reseed`)
- `Concertable.E2ETests.Api` (xUnit-direct) — green; assertions updated for escrow shape (PR #36 follow-up)
- `Concertable.E2ETests.Ui` (Reqnroll + Playwright) — `UiFixture`, `TestRunHooks`, `ScenarioDependencies` (Reqnroll DI registers `UiFixture` as a singleton sourced from `TestRunHooks.Fixture`), `HomePage` + `LoginPage` page objects, MSBuild target strips Reqnroll's auto-emitted `FeatureTitle`/`Description` traits so only `Category=Ui` shows in Test Explorer
- `data-testid` selector standard codified (Reqnroll Conventions §4)
- Headed Chromium is the local default (`Headless = CI=="true"`); CI runners set `CI=true` automatically

## Decisions locked in

- **Reqnroll for UI E2E** — multi-actor narratives (role transitions every few steps, SignalR events mid-flow, 4-strategy × N-outcome matrix) make Gherkin's structural reuse + readable subject prefixes worth the framework cost.
- **xUnit for API E2E** — assertions are single-subject (HTTP request → assert state). Gherkin adds no value here.
- **Reqnroll, not SpecFlow** — SpecFlow is unmaintained and lacks .NET 10 support; Reqnroll is the OSS fork by the original SpecFlow author, API-compatible.
- **C#, not TypeScript Playwright** — the C# fixture reuse (typed SeedDataResponse, in-process DB assertions, Aspire orchestration) wins over TS codegen DX. Use `npx playwright codegen --target csharp` for locator discovery only.
- **Authentication: real OIDC login per role, captured to `storageState` once per test run, hydrated per-scenario.** No ROPC backdoor. `Concertable.Auth` runs in Aspire E2E mode unmodified. Dedicated `Login.feature` covers the real redirect dance (already green).
- **Build from scratch** — happy-path scenarios drive the *entire* workflow in the UI starting from no state, rather than starting from a pre-seeded `Pending*App`. Half the value of UI E2E is proving the full chain works.
- **Chromium only for v1.** Multi-browser matrix deferred.
- **Outside-in / scenario-first build order** — write `.feature` first; let Reqnroll's missing-binding errors drive what to build next. Boilerplate emerges, sized to need. Auth hooks are the only precondition that must land before any scenario can run.

## Scope

**In v1:** Happy-path scenarios per concrete workflow strategy + customer ticket purchase + concert finish/settlement. Per-scenario DB reset + reseed.

**Out of v1:** Visual regression / screenshots. Mobile viewports. Accessibility audits. Failure branches (decline / 3DS fail / withdraw) — Phase 4.

## Workflow Matrix (post-escrow)

Four concrete strategies in `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Workflow/`. Money flow now uniformly goes through escrow — auth + capture timing varies per strategy, but final transfer always happens at settlement.

| # | Strategy | Contract | App subtype | Booking subtype | Auth point | Capture/Transfer point | UI checkout pages |
|---|----------|----------|---------|-----------------|------------|------------------------|-------------------|
| 1 | `FlatFeeConcertWorkflow` | `FlatFeeContract` (Fee) | `StandardApplication` | `StandardBooking` | VM at accept (Stripe + 3DS) | Settle: capture → transfer to artist | Artist apply (no payment) → VM accept (Stripe) |
| 2 | `DoorSplitConcertWorkflow` | `DoorSplitContract` (ArtistDoorPercent) | `StandardApplication` | `DeferredBooking` | Customer at ticket purchase | Settle: transfer artist's share to artist | Artist apply (no payment) → VM accept (no payment) → ticket sales |
| 3 | `VersusConcertWorkflow` | `VersusContract` (Guarantee + ArtistDoorPercent) | `StandardApplication` | `DeferredBooking` | Customer at ticket purchase | Settle: transfer guarantee + door share to artist | Artist apply (no payment) → VM accept (no payment) → ticket sales |
| 4 | `VenueHireConcertWorkflow` | `VenueHireContract` (HireFee) | `PrepaidApplication` (stores PaymentMethodId) | `StandardBooking` | Artist at apply (Stripe + 3DS) | Settle: capture → transfer to venue | Artist apply (Stripe) → VM accept (no payment) |

Plus three cross-cutting flows that aren't strategies but are UI surface:

| # | Flow | Notes |
|---|------|-------|
| 5 | Customer ticket purchase | UI version of existing `TicketPurchaseTests.cs` API tests |
| 6 | Concert finish / settlement | Triggered via `/e2e/finish/{concertId}` to keep tests deterministic; **post-escrow this is where most workflows actually pay out** |
| 7 | OIDC login | ✅ Done — single dedicated scenario covering the real redirect dance |

**Total v1 happy-path scenarios: 7** (4 workflows + customer + settlement + login). Login already green; 6 to go.

## Project Structure

```
api/Tests/Concertable.E2ETests/                        (parent classlib)
├── Concertable.E2ETests.csproj                        (<DefaultItemExcludes> excludes Api/Ui subfolders)
├── AppFixture.cs / StripeCliFixture.cs / SqlFixture.cs / PollingService.cs
├── DistributedApplicationBuilderExtensions.cs / TestTokenMinter.cs
├── E2ETestCollection.cs / MessageSinkLoggerProvider.cs / GlobalUsings.cs
│
├── Concertable.E2ETests.Api/                          (xUnit-direct, green)
│   ├── AssemblyInfo.cs                                ([AssemblyTrait("Category","Api")])
│   ├── CollectionRegistration.cs
│   └── Payments/
│       ├── ConcertDraftTests.cs
│       ├── ConcertFinishedTests.cs
│       └── TicketPurchaseTests.cs
│
└── Concertable.E2ETests.Ui/                           (Reqnroll + Playwright)
    ├── reqnroll.json
    ├── AssemblyInfo.cs                                ([AssemblyTrait("Category","Ui")])
    ├── Hooks/
    │   ├── TestRunHooks.cs                            ✅ ([BeforeTestRun] boots UiFixture; [AfterTestRun] disposes)
    │   ├── ScenarioDependencies.cs                    ✅ (registers UiFixture singleton via Reqnroll DI)
    │   ├── PlaywrightHooks.cs                         (TODO Step 5 — [BeforeScenario] hydrate IBrowserContext from cached storageState by @role tag; [AfterScenario] dispose)
    │   └── LoginCaptureHooks.cs                       (TODO Step 5 — [BeforeTestRun] real OIDC login per role → cache storageState)
    ├── Support/
    │   ├── UiFixture.cs                               ✅ (composes AppFixture + Browser/Playwright; Headless = CI=="true")
    │   ├── WorkflowState.cs                           (TODO — typed scenario state; injected via Reqnroll DI)
    │   ├── EnumTransformations.cs                     (TODO — [StepArgumentTransformation] for ContractType etc)
    │   ├── PageObjects/
    │   │   ├── HomePage.cs                            ✅
    │   │   ├── LoginPage.cs                           ✅
    │   │   ├── VenueManager/                          (TODO Step 5+)
    │   │   ├── Artist/                                (TODO Step 5+)
    │   │   └── Customer/                              (TODO Step 6)
    │   └── Helpers/
    │       ├── Money.cs                               (TODO Step 5 — escrow-aware assertions: AssertArtistReceivedAsync, AssertVenueChargedAsync, etc)
    │       ├── StripeUiHelpers.cs                     (TODO Step 5 — Stripe iframe walking + Cards constants)
    │       ├── SignalRWaiter.cs                       (TODO Step 5 — wait-for-event with PollingService)
    │       └── DbAssertions.cs                        (TODO Step 5 — read EF entities post-flow)
    ├── Steps/
    │   ├── LoginSteps.cs                              ✅
    │   ├── VenueManagerSteps.cs                       (TODO Step 5+)
    │   ├── ArtistSteps.cs                             (TODO Step 5+)
    │   ├── CustomerSteps.cs                           (TODO Step 6)
    │   ├── SystemSteps.cs                             (TODO Step 6 — /e2e/finish, SignalR)
    │   └── AssertionSteps.cs                          (TODO Step 5+)
    └── Features/
        ├── Login.feature                              ✅
        ├── FlatFeeWorkflow.feature                    (TODO Step 5)
        ├── DoorSplitWorkflow.feature                  (TODO Step 6)
        ├── VersusWorkflow.feature                     (TODO Step 6)
        ├── VenueHireWorkflow.feature                  (TODO Step 6)
        ├── CustomerTicketPurchase.feature             (TODO Step 6)
        └── ConcertSettlement.feature                  (TODO Step 6)
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
public class WorkflowState
{
    public int OpportunityId { get; set; }
    public int ApplicationId { get; set; }
    public int? ConcertId { get; set; }
}
```

`WorkflowState` is registered scenario-scoped via `Reqnroll.Microsoft.Extensions.DependencyInjection` and constructor-injected into step binding classes (DI already wired in `ScenarioDependencies.cs`).

### 3. Enum step parameters use `[StepArgumentTransformation]`

```csharp
[StepArgumentTransformation]
public ContractType TransformContractType(string value) => value switch
{
    "flat fee"   => ContractType.FlatFee,
    "door split" => ContractType.DoorSplit,
    "versus"     => ContractType.Versus,
    "venue hire" => ContractType.VenueHire,
    _ => throw new ArgumentException($"Unknown contract type: {value}")
};
```

A unit test in `Concertable.E2ETests.Ui` asserts that every `ContractType` enum value has a transformation row.

### 4. Page objects encapsulate all selector + Playwright + Stripe iframe logic

Steps don't know what a `data-testid` is. Pages don't know what a `[When]` is. Steps call methods like `await page.PayWithCardAsync(Cards.Success)` and the page handles iframe walking, fill, submit, wait-for-redirect.

**Selector standard:**

- **`data-testid` everywhere we touch DOM in the SPA** — every element a page object interacts with gets a stable `data-testid="<feature>-<element>"` (kebab-case, feature-prefixed). Add the attribute in the same PR as the page object that needs it.
- **Page objects use `page.GetByTestId(...)`**, not raw CSS — survives DOM/Tailwind churn.
- **Selector strings live as `private const` fields at the top of the page object** — never inline literals, never `public`.
- **Razor Pages on the Auth host follow the same `data-testid` rule.**
- Stripe iframe content is the unavoidable exception — selectors there target Stripe's stable element names (`[name='cardnumber']` etc), encapsulated in `StripeUiHelpers`.

Reference impl: `LoginPage.cs` + `Concertable.Auth/Pages/Account/Login.cshtml` (`login-email` / `login-password` / `login-submit`).

### 5. Background blocks for shared setup

Each `.feature` file uses `Background:` for the "fresh DB reseed + role auth state hydrated" setup that every scenario needs. Reduces scenario noise.

### 6. Money assertions go through `Money` helpers, never raw Stripe API

Escrow makes money flow phase-dependent (auth → capture → transfer). Page-level assertions like "the artist received the fee" must abstract over phase so Gherkin reads as business behaviour, not Stripe API shape.

```csharp
// ✅ GOOD — phase-agnostic, reads as business intent
[Then(@"the artist receives £(\d+)")]
public Task ArtistReceives(decimal amount) =>
    Money.AssertArtistReceivedAsync(fixture, _state, amount);

// ❌ BAD — leaks Stripe API shape; breaks if escrow timing shifts
[Then(@"the artist receives £(\d+)")]
public async Task ArtistReceives(decimal amount)
{
    var intent = await fixture.StripePaymentIntents.GetAsync(_state.PaymentIntentId);
    Assert.Equal(fixture.SeedData.ArtistManager.StripeAccountId, intent.TransferData?.DestinationId);
    Assert.Equal((long)(amount * 100), intent.Amount);
}
```

`Money.AssertArtistReceivedAsync` knows: at accept-time the funds are in platform balance, at settle-time a `Transfer` to the artist's connected account exists. Same Gherkin step, different assertion shape per phase.

## Authentication Strategy

Drive the real OIDC login flow once per role at test-run start, capture each role's `IBrowserContext.StorageStateAsync()` to an in-memory dict, hydrate per-scenario contexts from that captured state.

Lifecycle:
- `[BeforeTestRun]` in `LoginCaptureHooks` — for each role (`VenueManager`, `ArtistManager`, `Customer`), spin up a one-shot Playwright context, navigate to `/login`, complete the redirect dance with seeded credentials (`SeedData.Customer.Email` + `SeedData.TestPassword` etc), call `context.StorageStateAsync()` → string, cache by role.
- `[BeforeScenario]` in `PlaywrightHooks` — read scenario tags (`@VenueManager`, `@Artist`, `@Customer`), create a fresh `IBrowserContext` with `BrowserNewContextOptions { StorageState = cachedStateForRole }`. Hydrated and ready to navigate.
- `Login.feature` — explicitly does NOT use the cached state; drives the real redirect end-to-end. Single scenario, runs once. If it fails, the cache mechanism is broken.

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

### Step 5 — FlatFee happy path (NEXT — branch `Feature/UiE2eFlatFeeWorkflow-2`)

Multi-actor scenario covering the FlatFee strategy end-to-end via the UI, **post-escrow**:
1. VM posts a FlatFee opportunity (`Given a venue manager has posted a flat fee opportunity for £500`)
2. Artist applies (`When the artist applies to the opportunity`)
3. VM accepts with valid card (`And the venue manager accepts with a valid card`) — exercises Stripe Elements + 3DS in the UI; PaymentIntent authorised, funds parked in escrow
4. Application accepted, draft concert created (`Then the application is accepted` + DB assertions on draft concert)
5. *(Optional in same scenario or a separate `ConcertSettlement.feature` scenario)* concert finishes → escrow releases → transfer to artist (`Then the artist receives £500`)

**Build order (outside-in, scenario-first):**

1. **`FlatFeeWorkflow.feature`** — write the Gherkin first, ~10 lines. This drives everything else.
2. **`LoginCaptureHooks` + `PlaywrightHooks`** — auth precondition, must land before any scenario can run. Cache `VenueManager`, `ArtistManager`, `Customer` storageStates; hydrate `IBrowserContext` per scenario by tag.
3. **`WorkflowState` shell** + Reqnroll DI registration in `ScenarioDependencies` — fields added as steps need them.
4. **Per-step build, driven by Reqnroll's missing-binding errors:**
   - VM step → `OpportunitiesPage` (post opportunity form) → `data-testid` on SPA elements
   - Artist step → `OpportunityFindPage` + `MyApplicationsPage`
   - VM accept step → `ApplicationsPage` + `ApplicationCheckoutPage` (Stripe Elements) → `StripeUiHelpers` + `Cards.*` constants
   - Settlement step → `Money` helpers + `SystemSteps` (`/e2e/finish`)
   - Assertion steps → `DbAssertions` + `Money` helpers

5. **`EnumTransformations`** — added when Gherkin first uses an enum parameter.

**DoD:** scenario passes deterministically locally; trace viewer artefact saved on failure; test runs in CI on push to master.

### Step 6 — Remaining happy-path workflows (one PR each, in order)
- DoorSplit (no Stripe at apply/accept — payment moves to ticket purchase + settle)
- Versus (DoorSplit + extra contract field — guarantee transferred at settle alongside door share)
- VenueHire (apply-checkout — exercises new SPA work in `ApplicationCheckoutPage`; artist authorises at apply, captured + transferred to venue at settle)
- Customer ticket purchase
- Concert settlement (uses `/e2e/finish` — most workflows actually pay out here post-escrow, so this scenario gets real transfer assertions for FlatFee + VenueHire too)

### Phase 4 — Failure branches (deferred)
For each workflow with a payment step:
- Card declined (`Cards.Decline`)
- 3DS challenge failed (`Cards.Insufficient3ds`)
- User abandons checkout
- Escrow transfer failure at settle (Stripe rejects transfer to connected account)

For DoorSplit/Versus:
- VM rejects application

## Stripe-Specific Browser Concerns

- Stripe Elements iframes are cross-origin — Playwright handles via `page.FrameLocator("iframe[name^='__privateStripeFrame']")`. Encapsulated in `StripeUiHelpers`.
- 3DS challenge is a nested iframe — helper walks the iframe tree.
- Test cards: `Cards.Success = "4242424242424242"`, `Cards.Requires3ds = "4000002500003155"`, `Cards.Decline = "4000000000000002"`, `Cards.Insufficient3ds = "4000008400001629"`.
- Webhook timing: after `confirmPayment()` resolves on FE, BE may not have processed `payment_intent.succeeded` yet. Use `PollingService.UntilAsync(...)` to wait for BE state.
- Transfer timing (post-escrow): at settle, `Transfer` creation is async — `Money.AssertArtistReceivedAsync` polls for the transfer to land.

## CI Considerations (post-v1)

- UI tests run on push to `master` + nightly, NOT on every PR. API E2E + Unit + Integration on every PR.
- Trace viewer + video output on failure → CI artefacts.
- Browser install (`pwsh playwright.ps1 install chromium`) cached across CI runs.
- `CI=true` env var triggers headless Chromium automatically (set by GitHub Actions / Azure DevOps / GitLab by default).

## Open Questions (resolved)

- ~~ROPC vs UI login~~ — UI login per role, captured to storageState. Locked.
- ~~Build from scratch vs pre-seeded~~ — build from scratch. Locked.
- ~~Single project vs separate~~ — separate (parent `Concertable.E2ETests` + `.Api` + `.Ui`). Locked.
- ~~Reqnroll vs xUnit-direct~~ — Reqnroll for Ui, xUnit for Api. Locked.
- ~~Reqnroll vs SpecFlow~~ — Reqnroll. Locked.
- ~~TS vs C# Playwright~~ — C#. Locked.
- ~~Chromium-only v1~~ — yes. Locked.
- ~~Build escrow before E2E or after?~~ — escrow landed first (PR #36). E2E built against final shape.

## What this plan does not cover

- Step naming conventions (settled in Step 6 when first feature lands).
- Scenario tagging conventions (`@VenueManager` / `@Artist` / `@Customer` for auth hydration; rest decided as needed).

## Next Step

**Step 5: FlatFee happy path** on `Feature/UiE2eFlatFeeWorkflow-2`. Start by drafting `FlatFeeWorkflow.feature` Gherkin — needs UI flow clarification first (VM post-opportunity form fields, artist apply UX, VM accept page route, draft-concert post-state).
