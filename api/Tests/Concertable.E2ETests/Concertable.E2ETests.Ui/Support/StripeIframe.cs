namespace Concertable.E2ETests.Ui.Support;

public static class StripeCards
{
    public const string Success = "4242424242424242";
    public const string Requires3ds = "4000002500003155";
    public const string Decline = "4000000000000002";
    public const string Insufficient3ds = "4000008400001629";
}

public class StripeIframe
{
    private readonly IPage page;

    public StripeIframe(IPage page) => this.page = page;

    private IFrameLocator PaymentFrame =>
        page.FrameLocator("iframe[name^='__privateStripeFrame']").First;

    private ILocator NumberField => PaymentFrame.Locator("[name='number']");
    private ILocator ExpirationField => PaymentFrame.Locator("[name='expiration']");
    private ILocator CvcField => PaymentFrame.Locator("[name='cvc']");
    private ILocator PostalField => PaymentFrame.Locator("[name='postalCode']");

    public async Task FillCardAsync(string cardNumber)
    {
        await NumberField.WaitForAsync(new() { Timeout = 60_000 });
        await NumberField.FillAsync(cardNumber);
        await ExpirationField.FillAsync("12 / 30");
        await CvcField.FillAsync("123");
        if (await PostalField.CountAsync() > 0) await PostalField.FillAsync("12345");
    }

    public async Task Complete3dsChallengeAsync()
    {
        var challenge = page.FrameLocator("iframe[name^='__privateStripeFrame'] iframe");
        await challenge.GetByText("Complete authentication").ClickAsync(new() { Timeout = 15000 });
    }
}
