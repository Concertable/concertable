using Concertable.E2ETests.Ui.Support;

namespace Concertable.E2ETests.Ui.Hooks;

[Binding]
public class TestRunHooks
{
    public static UiFixture Fixture { get; private set; } = null!;

    [BeforeTestRun]
    public static async Task BeforeTestRun()
    {
        Fixture = new UiFixture();
        await Fixture.InitializeAsync();
        await Fixture.App.ResetAsync();
    }

    [AfterTestRun]
    public static async Task AfterTestRun()
    {
        if (Fixture is not null)
            await Fixture.DisposeAsync();
    }
}
