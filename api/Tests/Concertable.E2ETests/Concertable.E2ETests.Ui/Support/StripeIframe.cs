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

    public async Task FillCardAsync(string cardNumber)
    {
        await PaymentFrame.Locator("[name='number']").FillAsync(cardNumber);
        await PaymentFrame.Locator("[name='expiration']").FillAsync("12 / 30");
        await PaymentFrame.Locator("[name='cvc']").FillAsync("123");
        var postal = PaymentFrame.Locator("[name='postalCode']");
        if (await postal.CountAsync() > 0) await postal.FillAsync("12345");
    }

    public async Task Complete3dsChallengeAsync()
    {
        var challenge = page.FrameLocator("iframe[name^='__privateStripeFrame'] iframe");
        await challenge.GetByText("Complete authentication").ClickAsync(new() { Timeout = 15000 });
    }
}
