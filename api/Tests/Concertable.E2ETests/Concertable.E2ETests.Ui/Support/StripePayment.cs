namespace Concertable.E2ETests.Ui.Support;

public static class StripeCards
{
    public const string Success = "4242424242424242";
    public const string Requires3ds = "4000002500003155";
    public const string Decline = "4000000000000002";
    public const string Insufficient3ds = "4000008400001629";
}

public class StripePayment
{
    private readonly IPage page;

    public StripePayment(IPage page) => this.page = page;

    // Card inputs (and tab strip when a saved card is present) are always rendered inside
    // elements-inner-accessory-target. elements-inner-easel is the Stripe developer tools
    // frame and never contains card inputs.
    private IFrameLocator CardForm =>
        page.FrameLocator("iframe[src*='elements-inner-accessory-target']");

    private ILocator CardTab => CardForm.GetByText("Card", new() { Exact = true });
    private ILocator ConfirmButton => page.GetByTestId("confirm");

    public Task PayWithSavedCardAsync() => ConfirmButton.ClickAsync();

    public async Task PayWithNewCardAsync(string cardNumber)
    {
        await CardTab.ClickAsync();
        await FillCardAsync(cardNumber);
        await ConfirmButton.ClickAsync();
    }

    public Task Complete3dsChallengeAsync() =>
        CardForm.FrameLocator("iframe")
            .GetByText("Complete authentication")
            .ClickAsync(new() { Timeout = 15000 });

    private async Task FillCardAsync(string cardNumber)
    {
        var number = CardForm.Locator("[name='number']");
        var expiry = CardForm.Locator("[autocomplete='cc-exp']");
        var cvc = CardForm.Locator("[autocomplete='cc-csc']");
        var postalCode = CardForm.Locator("[name='postalCode']");

        await number.ClickAsync();
        await number.PressSequentiallyAsync(cardNumber, new() { Delay = 30 });
        await number.PressAsync("Tab");

        await expiry.ClickAsync();
        await expiry.PressSequentiallyAsync("1230", new() { Delay = 30 });
        await expiry.PressAsync("Tab");

        await cvc.ClickAsync();
        await cvc.PressSequentiallyAsync("123", new() { Delay = 30 });

        if (await postalCode.CountAsync() > 0)
            await postalCode.FillAsync("12345");
    }
}
