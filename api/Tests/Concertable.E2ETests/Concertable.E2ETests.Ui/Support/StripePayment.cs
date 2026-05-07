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

    private IFrameLocator PaymentFrame =>
        page.FrameLocator("iframe[title='Secure payment input frame']");

    private ILocator CardNumber    => PaymentFrame.Locator("[name='number']");
    private ILocator Expiry        => PaymentFrame.Locator("[autocomplete='cc-exp']");
    private ILocator Cvc           => PaymentFrame.Locator("[autocomplete='cc-csc']");
    private ILocator PostalCode    => PaymentFrame.Locator("[name='postalCode']");
    private ILocator NewCardTab    => PaymentFrame.GetByText("Card", new() { Exact = true });
    private ILocator ConfirmButton => page.GetByTestId("confirm");

    public Task PayWithSavedCardAsync() => ConfirmButton.ClickAsync();

    public async Task PayWithNewCardAsync(string cardNumber)
    {
        await SelectNewCardTabIfPresentAsync();
        await FillCardAsync(cardNumber);
        await ConfirmButton.ClickAsync();
    }

    public async Task Complete3dsChallengeAsync()
    {
        var challenge = PaymentFrame.FrameLocator("iframe");
        await challenge.GetByText("Complete authentication").ClickAsync(new() { Timeout = 15000 });
    }

    private async Task SelectNewCardTabIfPresentAsync()
    {
        if (await NewCardTab.CountAsync() == 0)
        {
            try { await NewCardTab.WaitForAsync(new() { Timeout = 30_000 }); }
            catch (TimeoutException) { return; }
        }
        await NewCardTab.ClickAsync();
    }

    private async Task FillCardAsync(string cardNumber)
    {
        await CardNumber.ClickAsync();
        await CardNumber.PressSequentiallyAsync(cardNumber, new() { Delay = 30 });
        await CardNumber.PressAsync("Tab");

        await Expiry.ClickAsync();
        await Expiry.PressSequentiallyAsync("1230", new() { Delay = 30 });
        await Expiry.PressAsync("Tab");

        await Cvc.ClickAsync();
        await Cvc.PressSequentiallyAsync("123", new() { Delay = 30 });

        if (await PostalCode.CountAsync() > 0)
            await PostalCode.FillAsync("12345");
    }
}
