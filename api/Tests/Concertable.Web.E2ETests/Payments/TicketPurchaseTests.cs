using Concertable.Web.E2ETests.Infrastructure;
using Xunit;

namespace Concertable.Web.E2ETests.Payments;

[Collection("E2E")]
public class TicketPurchaseTests : IAsyncLifetime
{
    private readonly AppFixture fixture;

    public TicketPurchaseTests(AppFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public void Boilerplate_AlwaysPasses() { }
}
