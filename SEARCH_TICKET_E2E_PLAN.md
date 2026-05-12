# Search & Ticket Purchase E2E Test Plan

## Scope

Two feature areas:

1. **Search** — location + geo-radius filter, genre filter, header-type filter, order/sort,
   autocomplete navigation to a concert detail page.
2. **Ticket purchase → toast → ticket → QR code** — end-to-end happy path from search
   result through checkout, Stripe confirmation, toast notification, upcoming tickets page,
   and opening the QR code dialog.

---

## Required `data-testid` Additions

Add these attributes before implementing the steps.
Convention: kebab-case, purpose-named, no `btn-`/`txt-` type prefixes.
Feature prefix only on collision (there are none below).

### `SearchBar.tsx`

| Element                    | `data-testid`          |
|----------------------------|------------------------|
| Location picker input      | `search-location`      |
| Query text input           | `search-query`         |
| Date range trigger button  | `search-date-trigger`  |
| Submit / search button     | `search-submit`        |

### `AutocompleteDropdown.tsx`

| Element                        | `data-testid`               |
|--------------------------------|-----------------------------|
| Dropdown container             | `autocomplete-dropdown`     |
| Each result button             | `autocomplete-item` (nth)   |

### `FilterSlider.tsx`

| Element                        | `data-testid`              |
|--------------------------------|----------------------------|
| Filter open button (funnel)    | `filter-open`              |
| Filter sheet panel             | `filter-panel`             |
| Header type select trigger     | `filter-header-type`       |
| Genre select trigger           | `filter-genre-select`      |
| Add genre "+" button           | `filter-genre-add`         |
| Genre badge remove "×" button  | `filter-genre-remove` (nth)|
| Radius slider root             | `filter-radius-slider`     |
| Radius current value span      | `filter-radius-value`      |
| Order by select trigger        | `filter-order-by`          |
| Sort order select trigger      | `filter-sort-order`        |
| Show History checkbox          | `filter-show-history`      |
| Show Sold checkbox             | `filter-show-sold`         |
| Apply button                   | `filter-apply`             |

### `SearchResults.tsx`

| Element                    | `data-testid`         |
|----------------------------|-----------------------|
| Results grid container     | `search-results`      |
| Each concert card link     | `concert-card` (nth)  |

### `ConcertCard.tsx`

| Element              | `data-testid`   |
|----------------------|-----------------|
| Buy Tickets button   | `buy-tickets`   |

### `QuantitySelector.tsx`

| Element              | `data-testid`       |
|----------------------|---------------------|
| Decrease (−) button  | `quantity-decrease` |
| Quantity value span  | `quantity-value`    |
| Increase (+) button  | `quantity-increase` |

### `StripePaymentForm.tsx` — already present

| Element              | `data-testid` |
|----------------------|---------------|
| Submit button        | `confirm`     |

### `CheckoutFlow.tsx` / `CheckoutAwaiting.tsx` / `CheckoutSuccess.tsx`

| Element                        | `data-testid`       |
|--------------------------------|---------------------|
| Awaiting container             | `checkout-awaiting` |
| Success container              | `checkout-success`  |
| "View tickets" button          | `view-tickets`      |

### `UpcomingTicketsPage.tsx`

| Element                  | `data-testid`            |
|--------------------------|--------------------------|
| Page heading             | `upcoming-tickets-title` |
| Ticket list container    | `upcoming-tickets-list`  |

### `TicketCard.tsx`

| Element           | `data-testid`  |
|-------------------|----------------|
| Ticket card root  | `ticket-card`  |

### `QrPopover.tsx`

| Element                      | `data-testid`  |
|------------------------------|----------------|
| QR thumbnail trigger button  | `qr-trigger`   |
| QR dialog / overlay          | `qr-dialog`    |
| Full-size QR image           | `qr-image`     |

---

## Feature Files

### `SearchFilters.feature`

```gherkin
Feature: Search filters
  As a customer browsing Concertable
  I want to filter concerts by location, radius, genre, and ordering
  So that I find relevant events near me

  Background:
    Given I am logged in as a customer
    And I navigate to the find page

  Scenario: Filter concerts by London with a 25 km radius
    When I enter "London" as my search location
    And I open the filter panel
    And I set the distance radius to 25 km
    And I apply the filters
    Then I should see search results
    And the radius display should show "25 km"

  Scenario: Filter concerts by genre
    When I open the filter panel
    And I select the genre "Rock"
    And I add the selected genre
    And I apply the filters
    Then I should see the "Rock" genre badge in the filter panel
    And I should see search results

  Scenario: Remove a genre filter
    Given I have "Rock" added as a genre filter
    When I open the filter panel
    And I remove the "Rock" genre badge
    And I apply the filters
    Then the "Rock" genre badge should not be visible

  Scenario: Filter by header type — venue
    When I open the filter panel
    And I select the header type "Venue"
    And I apply the filters
    Then I should see search results

  Scenario: Order results by rating descending
    When I open the filter panel
    And I select order by "Rating"
    And I select sort order "Descending"
    And I apply the filters
    Then I should see search results

  Scenario: Navigate to a concert via autocomplete
    When I type "Summer" into the search query
    Then the autocomplete dropdown should appear
    When I click the first autocomplete result
    Then I should be on the concert detail page
```

---

### `TicketPurchase.feature`

```gherkin
Feature: Ticket purchase, toast notification, and QR code
  As a customer
  I want to purchase a ticket for a concert found via search
  So that I receive a ticket with a QR code I can use at the door

  Background:
    Given I am logged in as a customer
    And a concert "Summer Rock Night" is listed with available tickets
    And I navigate to the find page

  Scenario: Happy path — purchase ticket and view QR code
    # --- Search & navigate to concert ---
    When I enter "London" as my search location
    And I open the filter panel
    And I set the distance radius to 50 km
    And I select the header type "Concert"
    And I apply the filters
    Then I should see "Summer Rock Night" in the search results

    When I click on the "Summer Rock Night" concert card
    Then I should be on the concert detail page for "Summer Rock Night"

    # --- Initiate checkout ---
    When I click "Buy Tickets"
    Then I should be on the checkout page

    # --- Select quantity ---
    And the quantity should default to 1
    When I increase the quantity to 2
    Then the quantity should show "2"

    # --- Pay ---
    When I complete payment with a test card
    And I click confirm
    Then the checkout awaiting screen should be visible

    # --- Post-payment toast ---
    When the payment is confirmed by the system
    Then a success toast "You purchased 2 tickets" should appear
    And the toast should have a "View" action

    # --- Navigate to tickets via toast ---
    When I click the "View" action on the toast
    Then I should be on the upcoming tickets page
    And I should see a ticket for "Summer Rock Night"

    # --- QR code ---
    When I click the QR thumbnail on the "Summer Rock Night" ticket
    Then the QR dialog should be open
    And the QR code image should be visible

  Scenario: Purchase a single ticket and navigate via "View tickets" button
    Given I am on the checkout page for "Summer Rock Night"
    When I complete payment with a test card
    And I click confirm
    Then the checkout awaiting screen should be visible
    When the payment is confirmed by the system
    Then the checkout success screen should be visible
    When I click "View tickets"
    Then I should be on the upcoming tickets page
    And I should see a ticket for "Summer Rock Night"
    When I click the QR thumbnail on the "Summer Rock Night" ticket
    Then the QR dialog should be open
    And the QR code image should be visible
```

---

## Page Objects

### `FindPage.cs`

```csharp
internal sealed class FindPage(IPage page)
{
    // Search bar
    private ILocator LocationInput    => page.GetByTestId("search-location");
    private ILocator QueryInput       => page.GetByTestId("search-query");
    private ILocator SearchButton     => page.GetByTestId("search-submit");

    // Filter panel
    private ILocator FilterOpenButton => page.GetByTestId("filter-open");
    private ILocator FilterPanel      => page.GetByTestId("filter-panel");
    private ILocator HeaderTypeSelect => page.GetByTestId("filter-header-type");
    private ILocator GenreSelect      => page.GetByTestId("filter-genre-select");
    private ILocator GenreAddButton   => page.GetByTestId("filter-genre-add");
    private ILocator RadiusSlider     => page.GetByTestId("filter-radius-slider");
    private ILocator RadiusValue      => page.GetByTestId("filter-radius-value");
    private ILocator OrderBySelect    => page.GetByTestId("filter-order-by");
    private ILocator SortOrderSelect  => page.GetByTestId("filter-sort-order");
    private ILocator ApplyButton      => page.GetByTestId("filter-apply");

    // Results
    private ILocator ResultsGrid      => page.GetByTestId("search-results");
    private ILocator ConcertCards     => page.GetByTestId("concert-card");

    // Autocomplete
    private ILocator AutocompleteDropdown => page.GetByTestId("autocomplete-dropdown");
    private ILocator AutocompleteItems    => page.GetByTestId("autocomplete-item");

    public async Task EnterLocationAsync(string location) { ... }
    public async Task EnterQueryAsync(string query) { ... }
    public async Task OpenFilterPanelAsync() { ... }
    public async Task SetRadiusAsync(int km) { /* drag slider */ }
    public async Task SelectHeaderTypeAsync(string type) { ... }
    public async Task SelectGenreAsync(string genre) { ... }
    public async Task AddGenreAsync() { ... }
    public async Task ApplyFiltersAsync() { ... }
    public async Task ClickFirstAutocompleteResultAsync() { ... }
    public async Task ClickConcertCardAsync(string name) { ... }
    public async Task<string> GetRadiusValueAsync() => await RadiusValue.InnerTextAsync();
    public async Task<bool> HasResultsAsync() => await ResultsGrid.IsVisibleAsync();
}
```

### `ConcertDetailsPage.cs`

```csharp
internal sealed class ConcertDetailsPage(IPage page)
{
    private ILocator BuyTicketsButton => page.GetByTestId("buy-tickets");

    public async Task ClickBuyTicketsAsync() => await BuyTicketsButton.ClickAsync();
}
```

### `CheckoutPage.cs`

```csharp
internal sealed class CheckoutPage(IPage page)
{
    private ILocator DecreaseButton   => page.GetByTestId("quantity-decrease");
    private ILocator QuantityValue    => page.GetByTestId("quantity-value");
    private ILocator IncreaseButton   => page.GetByTestId("quantity-increase");
    private ILocator ConfirmButton    => page.GetByTestId("confirm");
    private ILocator AwaitingScreen   => page.GetByTestId("checkout-awaiting");
    private ILocator SuccessScreen    => page.GetByTestId("checkout-success");
    private ILocator ViewTicketsButton => page.GetByTestId("view-tickets");

    public async Task<string> GetQuantityAsync() => await QuantityValue.InnerTextAsync();
    public async Task IncreaseQuantityAsync(int times = 1)
    {
        for (var i = 0; i < times; i++)
            await IncreaseButton.ClickAsync();
    }

    // Fills the Stripe test card inside the iframe PaymentElement
    public async Task FillTestCardAsync() { /* interact with Stripe iframe */ }

    public async Task ClickConfirmAsync() => await ConfirmButton.ClickAsync();
    public async Task WaitForAwaitingScreenAsync() => await AwaitingScreen.WaitForAsync();
    public async Task WaitForSuccessScreenAsync() => await SuccessScreen.WaitForAsync();
    public async Task ClickViewTicketsAsync() => await ViewTicketsButton.ClickAsync();
}
```

### `UpcomingTicketsPage.cs`

```csharp
internal sealed class UpcomingTicketsPage(IPage page)
{
    private ILocator Title       => page.GetByTestId("upcoming-tickets-title");
    private ILocator TicketList  => page.GetByTestId("upcoming-tickets-list");
    private ILocator TicketCards => page.GetByTestId("ticket-card");
    private ILocator QrTrigger   => page.GetByTestId("qr-trigger");
    private ILocator QrDialog    => page.GetByTestId("qr-dialog");
    private ILocator QrImage     => page.GetByTestId("qr-image");

    public async Task<bool> HasTicketForAsync(string concertName)
    {
        var cards = await TicketCards.AllAsync();
        foreach (var card in cards)
            if ((await card.InnerTextAsync()).Contains(concertName))
                return true;
        return false;
    }

    public async Task ClickQrTriggerForAsync(string concertName)
    {
        var cards = await TicketCards.AllAsync();
        foreach (var card in cards)
            if ((await card.InnerTextAsync()).Contains(concertName))
            {
                await card.GetByTestId("qr-trigger").ClickAsync();
                return;
            }
    }

    public async Task<bool> IsQrDialogOpenAsync() => await QrDialog.IsVisibleAsync();
    public async Task<bool> IsQrImageVisibleAsync() => await QrImage.IsVisibleAsync();
}
```

### Toast helper (inline in steps or shared fixture)

```csharp
// Sonner renders with role="status"; match by text content
private ILocator Toast(string text) =>
    page.Locator("[data-sonner-toast]").Filter(new() { HasText = text });

private ILocator ToastAction(string text, string actionLabel) =>
    Toast(text).GetByRole(AriaRole.Button, new() { Name = actionLabel });
```

---

## Step Definitions Sketch

### `SearchSteps.cs`

```
Given I navigate to the find page
  → await page.GotoAsync("/find");

When I enter {string} as my search location
  → await findPage.EnterLocationAsync(location);

When I open the filter panel
  → await findPage.OpenFilterPanelAsync();

When I set the distance radius to {int} km
  → await findPage.SetRadiusAsync(km);

When I select the header type {string}
  → await findPage.SelectHeaderTypeAsync(type);

When I select the genre {string}
  → await findPage.SelectGenreAsync(genre);

When I add the selected genre
  → await findPage.AddGenreAsync();

When I apply the filters
  → await findPage.ApplyFiltersAsync();

When I type {string} into the search query
  → await findPage.EnterQueryAsync(query);

When I click the first autocomplete result
  → await findPage.ClickFirstAutocompleteResultAsync();

When I click on the {string} concert card
  → await findPage.ClickConcertCardAsync(name);

Then I should see search results
  → Assert.True(await findPage.HasResultsAsync());

Then the radius display should show {string}
  → Assert.Equal(expected, await findPage.GetRadiusValueAsync());

Then I should see {string} in the search results
  → (find card by text in results grid)

Then the autocomplete dropdown should appear
  → await findPage.AutocompleteDropdown.WaitForAsync();

Then I should be on the concert detail page
  → StringAssert.Contains("/find/concert/", page.Url);
```

### `CheckoutSteps.cs`

```
When I click "Buy Tickets"
  → await concertDetailsPage.ClickBuyTicketsAsync();

Then I should be on the checkout page
  → StringAssert.Contains("/concert/checkout/", page.Url);

Then the quantity should default to 1
  → Assert.Equal("1", await checkoutPage.GetQuantityAsync());

When I increase the quantity to {int}
  → await checkoutPage.IncreaseQuantityAsync(n - 1);  // starts at 1

Then the quantity should show {string}
  → Assert.Equal(expected, await checkoutPage.GetQuantityAsync());

When I complete payment with a test card
  → await checkoutPage.FillTestCardAsync();

When I click confirm
  → await checkoutPage.ClickConfirmAsync();

Then the checkout awaiting screen should be visible
  → await checkoutPage.WaitForAwaitingScreenAsync();

When the payment is confirmed by the system
  → await page.WaitForSelectorAsync("[data-sonner-toast]", new() { Timeout = 30_000 });

Then a success toast {string} should appear
  → await Toast(expected).WaitForAsync();

Then the toast should have a {string} action
  → Assert.True(await ToastAction(toastText, action).IsVisibleAsync());

When I click the {string} action on the toast
  → await ToastAction(toastText, action).ClickAsync();

Then the checkout success screen should be visible
  → await checkoutPage.WaitForSuccessScreenAsync();

When I click "View tickets"
  → await checkoutPage.ClickViewTicketsAsync();
```

### `TicketSteps.cs`

```
Then I should be on the upcoming tickets page
  → StringAssert.Contains("/profile/tickets/upcoming", page.Url);

Then I should see a ticket for {string}
  → Assert.True(await upcomingTicketsPage.HasTicketForAsync(concertName));

When I click the QR thumbnail on the {string} ticket
  → await upcomingTicketsPage.ClickQrTriggerForAsync(concertName);

Then the QR dialog should be open
  → Assert.True(await upcomingTicketsPage.IsQrDialogOpenAsync());

And the QR code image should be visible
  → Assert.True(await upcomingTicketsPage.IsQrImageVisibleAsync());
```

---

## Notes

- **Stripe test card** — use `4242 4242 4242 4242`, any future expiry, any CVC.
  The `PaymentElement` renders inside a cross-origin iframe; use
  `page.FrameLocator("iframe[name^='__privateStripeFrame']")` to reach inputs.
- **Toast timing** — set a 30 s timeout on the toast locator; it fires via SignalR
  after the payment webhook lands, which can take a few seconds in the test environment.
- **Slider drag** — the Radix `Slider` accepts keyboard input; focus the thumb and
  use `page.Keyboard.PressAsync("ArrowRight")` repeatedly rather than a mouse drag.
- **QR dialog** — after clicking the trigger, wait for `qr-dialog` to be visible
  before asserting `qr-image`; the dialog has a short CSS transition.
- **Feature files location** — `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Features/`
- **Page objects location** — `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/PageObjects/`
- **Steps location** — `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Steps/`
