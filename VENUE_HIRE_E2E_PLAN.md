# DoorSplit & Versus UI E2E Plan

Both are `IStandardConcertWorkflow` (artist applies without payment). The only
structural difference from FlatFee is that the accept step runs a **SetupSession**
(card saved for future deferred settlement) rather than an immediate payment intent.
The Stripe UI and `ApplicationCheckoutPage.PayWithSavedCardAsync` behave identically
either way — no new checkout page is needed.

---

## Workflow recap

| Workflow  | Apply  | Accept                               | Settlement trigger       |
|-----------|--------|--------------------------------------|--------------------------|
| FlatFee   | No pay | Venue mgr pays (OnSession)           | Upfront capture          |
| VenueHire | Artist pays (Authorize)  | No pay              | Upfront capture          |
| DoorSplit | No pay | Venue mgr saves card (SetupSession)  | Deferred: % of door      |
| Versus    | No pay | Venue mgr saves card (SetupSession)  | Deferred: guarantee + %  |

---

## 1. Feature files

### `Features/DoorSplitWorkflow.feature`

```gherkin
Feature: DoorSplit workflow happy path
  A venue manager posts a door-split opportunity, an artist applies, the venue
  manager accepts and registers a card for future settlement. A draft concert is created.

  @VenueManager
  Scenario: Venue manager books artist on a door split
    When the venue manager posts a door split opportunity for 70% door
    And the artist applies to the opportunity
    And the venue manager accepts the application with a valid card
    Then a draft concert is created
```

### `Features/VersusWorkflow.feature`

```gherkin
Feature: Versus workflow happy path
  A venue manager posts a versus opportunity (guarantee + door percentage), an artist
  applies, the venue manager accepts and registers a card for future settlement. A draft
  concert is created.

  @VenueManager
  Scenario: Venue manager books artist on a versus deal
    When the venue manager posts a versus opportunity for £100 guarantee and 70% door
    And the artist applies to the opportunity
    And the venue manager accepts the application with a valid card
    Then a draft concert is created
```

Both scenarios reuse two existing steps unchanged:

- `ArtistSteps` — `"the artist applies to the opportunity"` (already covers the no-payment path)
- `VenueManagerSteps` — `"the venue manager accepts the application with a valid card"` and `"a draft concert is created"` (FlatFee accept; works because `ApplicationCheckoutPage` doesn't distinguish payment vs setup sessions)

---

## 2. New step definitions — `VenueManagerSteps.cs`

Add two `When` steps alongside the existing FlatFee/VenueHire posting steps:

```csharp
[When(@"the venue manager posts a door split opportunity for (\d+)% door")]
public async Task WhenTheVenueManagerPostsADoorSplitOpportunity(int doorPercent)
{
    await browser.UseRoleAsync(Role.VenueManager);
    state.VenueId = fixture.App.SeedData.VenueManager1.VenueId;
    myVenuePage = new MyVenuePage(browser.Page, fixture.App.SpaBaseUrl);
    await myVenuePage.GotoAsync();
    await myVenuePage.PostDoorSplitOpportunityAsync(doorPercent);
    await myVenuePage.WaitUntilSavedAsync();
    state.OpportunityId = await FetchNewestOpportunityIdAsync(state.VenueId);
}

[When(@"the venue manager posts a versus opportunity for £(\d+) guarantee and (\d+)% door")]
public async Task WhenTheVenueManagerPostsAVersusOpportunity(int guarantee, int doorPercent)
{
    await browser.UseRoleAsync(Role.VenueManager);
    state.VenueId = fixture.App.SeedData.VenueManager1.VenueId;
    myVenuePage = new MyVenuePage(browser.Page, fixture.App.SpaBaseUrl);
    await myVenuePage.GotoAsync();
    await myVenuePage.PostVersusOpportunityAsync(guarantee, doorPercent);
    await myVenuePage.WaitUntilSavedAsync();
    state.OpportunityId = await FetchNewestOpportunityIdAsync(state.VenueId);
}
```

---

## 3. Page object changes — `Pages/MyVenuePage.cs`

Add two methods alongside the existing `PostFlatFeeOpportunityAsync` /
`PostVenueHireOpportunityAsync`. The contract-type selector is a Shadcn `Select`,
so use click-trigger → click-option (not `SelectOptionAsync`).

New locators needed (add alongside existing ones):

```csharp
private ILocator DoorSplitPercentInput =>
    page.GetByTestId("opportunity-card-edit").Last.GetByTestId("contract-doorsplit-percent");
private ILocator VersusGuaranteeInput =>
    page.GetByTestId("opportunity-card-edit").Last.GetByTestId("contract-versus-guarantee");
private ILocator VersusPercentInput =>
    page.GetByTestId("opportunity-card-edit").Last.GetByTestId("contract-versus-percent");
```

### `PostDoorSplitOpportunityAsync(int doorPercent)`

```
1. EditButton.ClickAsync()
2. AddOpportunityButton.ClickAsync()
3. ContractTypeSelect.ClickAsync()
4. page.GetByRole(AriaRole.Option, new() { Name = "Door Split" }).ClickAsync()
5. DoorSplitPercentInput.FillAsync(doorPercent.ToString())
6. SaveButton.ClickAsync()
```

### `PostVersusOpportunityAsync(int guarantee, int doorPercent)`

```
1. EditButton.ClickAsync()
2. AddOpportunityButton.ClickAsync()
3. ContractTypeSelect.ClickAsync()
4. page.GetByRole(AriaRole.Option, new() { Name = "Versus" }).ClickAsync()
5. VersusGuaranteeInput.FillAsync(guarantee.ToString())
6. VersusPercentInput.FillAsync(doorPercent.ToString())
7. SaveButton.ClickAsync()
```

No changes needed to:
- `ApplicationsPage` — accept flow identical to FlatFee
- `ApplicationCheckoutPage` — `PayWithSavedCardAsync` works for setup sessions
- `ArtistVenueDetailsPage` — no-payment apply flow already covered

---

## 4. Test data

No pre-seeded contracts required. Both scenarios create the opportunity (and its
contract) live through the UI during each test run. The existing test seeder
baseline (venue + artist accounts) is already sufficient — FlatFee and VenueHire
share the same setup.

---

## 5. Things to verify during implementation

1. **Shadcn Select option labels** — confirm the display labels for `doorSplit` and
   `versus` in the frontend `CONTRACT_TYPE_LABELS` map. The steps above assume
   `"Door Split"` and `"Versus"` respectively; adjust if they differ.

2. **SetupSession Stripe CLI** — the deferred booking registers a payment method
   (SetupIntent) rather than capturing a charge. Confirm `StripeCliFixture` forwards
   `setup_intent.succeeded` webhooks to the app, not just `payment_intent.*` ones.

3. **Draft concert URL after SetupSession accept** — FlatFee redirects to
   `/venue/my/concerts/concert/{id}`. Verify DoorSplit/Versus do the same;
   if they redirect to `/venue/my/applications/{opportunityId}` (like VenueHire),
   the `WaitForSuccessAsync` in the shared `"a draft concert is created"` step will
   need the same dual-path guard that was added for VenueHire.

---

## 6. Implementation order

1. Add `Features/DoorSplitWorkflow.feature`
2. Add `Features/VersusWorkflow.feature`
3. Extend `Pages/MyVenuePage.cs` with three new locators + two new posting methods
4. Add the two `When` steps to `StepDefinitions/VenueManagerSteps.cs`
5. Run both scenarios headed locally; confirm draft concert URL appears
6. Verify only `Category=Ui` trait is emitted (auto-traits already stripped by
   `StripReqnrollFeatureTitleAndDescriptionTraits` MSBuild target)
