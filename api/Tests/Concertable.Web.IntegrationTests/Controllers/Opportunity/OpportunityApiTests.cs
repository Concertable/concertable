using System.Net;
using Concertable.Application.DTOs;
using Concertable.Application.Responses;
using Concertable.Concert.Api.Responses;
using Concertable.Web.IntegrationTests.Infrastructure;
using Concertable.Core.Enums;
using Xunit;
using static Concertable.Web.IntegrationTests.Controllers.Opportunity.OpportunityRequestBuilders;

namespace Concertable.Web.IntegrationTests.Controllers.Opportunity;

[Collection("Integration")]
internal class OpportunityApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public OpportunityApiTests(ApiFixture fixture)
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
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);
        var request = BuildRequest(contract, fixture.SeedData.Rock.Id);

        // Act
        var response = await client.PostAsync("/api/Opportunity", request);

        // Assert
        var opportunity = await response.Content.ReadAsync<OpportunityDto>();
        Assert.NotNull(opportunity);
        Assert.NotNull(opportunity.Id);
        Assert.Equal(request.StartDate, opportunity.StartDate);
        Assert.Equal(request.EndDate, opportunity.EndDate);
        Assert.Contains(opportunity.Genres, g => g.Id == fixture.SeedData.Rock.Id);
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenNotVenueManager()
    {
        // Arrange
        var client = fixture.CreateClient(fixture.SeedData.ArtistManager);

        // Act
        var response = await client.PostAsync("/api/Opportunity", BuildDefaultRequest(fixture.SeedData.Rock.Id));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PostAsync("/api/Opportunity", BuildDefaultRequest(fixture.SeedData.Rock.Id));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GetActiveByVenueId

    [Fact]
    public async Task GetActiveByVenueId_ShouldReturnSeededOpportunity()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var result = await client.GetAsync<Pagination<OpportunityDto>>(
            $"/api/Opportunity/active/venue/{fixture.SeedData.Venue.Id}");

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.Data, o => o.Id == fixture.SeedData.Opportunities[0].Id);
    }

    #endregion
}
