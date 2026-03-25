using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Requests;
using Concertable.Web.IntegrationTests.Infrastructure;
using Core.Enums;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers;

public class ConcertOpportunityApiTests : IClassFixture<ApiFixture>
{
    private readonly ApiFixture fixture;


    public ConcertOpportunityApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public static TheoryData<IContract> AllContractTypes => new()
    {
        new FlatFeeContractDto { PaymentMethod = PaymentMethod.Cash, Fee = 500 },
        new DoorSplitContractDto { PaymentMethod = PaymentMethod.Cash, ArtistDoorPercent = 70 },
        new VersusContractDto { PaymentMethod = PaymentMethod.Cash, Guarantee = 200, ArtistDoorPercent = 60 },
        new VenueHireContractDto { PaymentMethod = PaymentMethod.Cash, HireFee = 300 },
    };

    #region Create

    [Theory]
    [MemberData(nameof(AllContractTypes))]
    public async Task Create_ShouldReturn201(IContract contract)
    {
        var client = fixture.CreateClient(TestDbInitializer.VenueManagerId, "VenueManager");

        var response = await client.PostAsync("/api/ConcertOpportunity", BuildRequest(contract));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenNotVenueManager()
    {
        var client = fixture.CreateClient(TestDbInitializer.ArtistManagerId, "ArtistManager");

        var response = await client.PostAsync("/api/ConcertOpportunity", DefaultRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsync("/api/ConcertOpportunity", DefaultRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GetActiveByVenueId

    [Fact]
    public async Task GetActiveByVenueId_ShouldReturnOpportunities()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/ConcertOpportunity/active/venue/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IEnumerable<ConcertOpportunityDto>>();
        Assert.NotNull(result);
    }

    #endregion

    #region Update

    [Theory]
    [MemberData(nameof(AllContractTypes))]
    public async Task Update_ShouldReturn200(IContract contract)
    {
        var client = fixture.CreateClient(TestDbInitializer.VenueManagerId, "VenueManager");

        var created = await client.PostAsync<ConcertOpportunityRequest, ConcertOpportunityDto>("/api/ConcertOpportunity", DefaultRequest());
        var response = await client.PutAsync($"/api/ConcertOpportunity/{created!.Id}", BuildRequest(contract));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn403_WhenNotVenueManager()
    {
        var client = fixture.CreateClient(TestDbInitializer.ArtistManagerId, "ArtistManager");

        var response = await client.PutAsync("/api/ConcertOpportunity/1", DefaultRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    #endregion

    #region Helpers

    private static ConcertOpportunityRequest DefaultRequest() =>
        BuildRequest(new FlatFeeContractDto { PaymentMethod = PaymentMethod.Cash, Fee = 500 });

    private static ConcertOpportunityRequest BuildRequest(IContract contract) =>
        new()
        {
            StartDate = DateTime.UtcNow.AddMonths(1),
            EndDate = DateTime.UtcNow.AddMonths(1).AddHours(3),
            Genres = [new GenreDto(1, "Rock")],
            Contract = contract
        };

    #endregion
}
