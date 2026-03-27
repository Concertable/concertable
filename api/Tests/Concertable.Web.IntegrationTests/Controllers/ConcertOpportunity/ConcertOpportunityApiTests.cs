using System.Net;
using Application.DTOs;
using Application.Interfaces.Concert;
using Concertable.Web.IntegrationTests.Infrastructure;
using Core.Enums;
using Xunit;
using static Concertable.Web.IntegrationTests.Controllers.ConcertOpportunity.ConcertOpportunityRequestBuilders;

namespace Concertable.Web.IntegrationTests.Controllers.ConcertOpportunity;

[Collection("Integration")]
public class ConcertOpportunityApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ConcertOpportunityApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    public static TheoryData<IContract> AllContractTypes =>
    [
        new FlatFeeContractDto { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
        new DoorSplitContractDto { PaymentMethod = PaymentMethod.Cash, ArtistDoorPercent = 70 },
        new VersusContractDto { PaymentMethod = PaymentMethod.Cash, Guarantee = 200, ArtistDoorPercent = 60 },
        new VenueHireContractDto { PaymentMethod = PaymentMethod.Cash, HireFee = 300 },
    ];

    #region Create

    [Theory]
    [MemberData(nameof(AllContractTypes))]
    public async Task Create_ShouldReturnCreatedOpportunity(IContract contract)
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildRequest(contract);

        var response = await client.PostAsync("/api/ConcertOpportunity", request);
        var opportunity = await response.Content.ReadAsync<ConcertOpportunityDto>();

        Assert.NotNull(opportunity);
        Assert.NotNull(opportunity.Id);
        Assert.Equal(request.StartDate, opportunity.StartDate);
        Assert.Equal(request.EndDate, opportunity.EndDate);
        Assert.Contains(opportunity.Genres, g => g.Id == TestConstants.GenreId);
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenNotVenueManager()
    {
        var client = fixture.CreateClient(TestConstants.ArtistManager);

        var response = await client.PostAsync("/api/ConcertOpportunity", BuildDefaultRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsync("/api/ConcertOpportunity", BuildDefaultRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GetActiveByVenueId

    [Fact]
    public async Task GetActiveByVenueId_ShouldReturnSeededOpportunity()
    {
        var client = fixture.CreateClient();

        var opportunities = await client.GetAsync<IEnumerable<ConcertOpportunityDto>>(
            $"/api/ConcertOpportunity/active/venue/{TestConstants.VenueId}");

        Assert.NotNull(opportunities);
        Assert.Contains(opportunities, o => o.Id == TestConstants.FlatFee.OpportunityId);
    }

    #endregion
}
