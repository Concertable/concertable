namespace Concertable.E2ETests.Ui.Support;

internal sealed class StripePayment(Browser browser, StripeCardEntry cardEntry) : IStripePayment
{
    private IPage Page => browser.Page;

    public Task PayWithSavedCardAsync() => cardEntry.PayWithSavedCardAsync();
    public Task PayWithNewCardAsync(string cardNumber) => cardEntry.PayWithNewCardAsync(cardNumber);

    public async Task CompleteChallengeAsync()
    {
        var outer = Page.Locator("iframe[src*='three-ds-2-challenge']");
        await outer.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 30_000 });

        var challengeFrame = outer.ContentFrame.Locator("#challengeFrame");
        await challengeFrame.WaitForAsync(new() { Timeout = 30_000 });

        var button = challengeFrame.ContentFrame.Locator("#test-source-authorize-3ds");
        await button.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 30_000 });

        await button.EvaluateAsync("el => el.click()");
        await outer.WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = 30_000 });
    }
}
