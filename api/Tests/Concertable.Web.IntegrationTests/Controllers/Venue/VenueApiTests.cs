using System.Net;
using Application.DTOs;
using Concertable.Web.IntegrationTests.Infrastructure;
using Xunit;
using static Concertable.Web.IntegrationTests.Controllers.Venue.VenueRequestBuilders;

namespace Concertable.Web.IntegrationTests.Controllers.Venue;

[Collection("Integration")]
public class VenueApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public VenueApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region GetDetailsById

    [Fact]
    public async Task GetDetailsById_ShouldReturn200_WithVenueDto()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/api/Venue/{TestConstants.VenueId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venue = await response.Content.ReadAsync<VenueDto>();
        Assert.NotNull(venue);
        Assert.Equal(TestConstants.VenueId, venue.Id);
        Assert.Equal("Test Venue", venue.Name);
    }

    [Fact]
    public async Task GetDetailsById_ShouldReturn404_WhenVenueDoesNotExist()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Venue/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region GetDetailsForCurrentUser

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Venue/user");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn403_WhenNotVenueManager()
    {
        var client = fixture.CreateClient(TestConstants.ArtistManager);

        var response = await client.GetAsync("/api/Venue/user");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn200_WhenVenueExists()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        var response = await client.GetAsync("/api/Venue/user");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venue = await response.Content.ReadAsync<VenueDto>();
        Assert.NotNull(venue);
        Assert.Equal("Test Venue", venue.Name);
    }

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn204_WhenNoVenueExists()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager2);

        var response = await client.GetAsync("/api/Venue/user");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();
        var request = BuildCreateRequest();

        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenNotVenueManager()
    {
        var client = fixture.CreateClient(TestConstants.ArtistManager);
        var request = BuildCreateRequest();

        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WithVenueDto_WhenValidRequest()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager2);
        var request = BuildCreateRequest();

        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var venue = await response.Content.ReadAsync<VenueDto>();
        Assert.NotNull(venue);
        Assert.True(venue.Id > 0);
        Assert.Equal(request.Name, venue.Name);
        Assert.Equal(request.About, venue.About);
        Assert.Equal(request.Latitude, venue.Latitude);
        Assert.Equal(request.Longitude, venue.Longitude);
        Assert.Equal("Test County", venue.County);
        Assert.Equal("Test Town", venue.Town);
        Assert.Equal("venuemanager2@test.com", venue.Email);
        Assert.False(venue.Approved);
        Assert.EndsWith(".jpg", venue.ImageUrl);
        Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(venue.ImageUrl), out _));
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenGeocodingFails()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager2, o => o.UseFailingGeocoding());
        var request = BuildCreateRequest();

        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenNameIsEmpty()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager2);
        var request = BuildCreateRequest(name: "");

        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();
        var request = BuildUpdateRequest();

        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn403_WhenNotVenueManager()
    {
        var client = fixture.CreateClient(TestConstants.ArtistManager);
        var request = BuildUpdateRequest();

        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn403_WhenNotOwner()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager2);
        var request = BuildUpdateRequest();

        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenVenueDoesNotExist()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildUpdateRequest();

        var response = await client.PutAsync("/api/Venue/99999", request.ToFormContent());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn200_WithUpdatedVenueDto_WhenValidRequest()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildUpdateRequest();

        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venue = await response.Content.ReadAsync<VenueDto>();
        Assert.NotNull(venue);
        Assert.Equal("Updated Venue", venue.Name);
        Assert.Equal("Updated about", venue.About);
        Assert.Equal("Test County", venue.County);
        Assert.Equal("Test Town", venue.Town);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenNameIsEmpty()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildUpdateRequest(name: "");

        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region IsOwner

    [Fact]
    public async Task IsOwner_ShouldReturnTrue_WhenOwner()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        var response = await client.GetAsync($"/api/Venue/is-owner/{TestConstants.VenueId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<bool>();
        Assert.True(result);
    }

    [Fact]
    public async Task IsOwner_ShouldReturnFalse_WhenNotOwner()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);

        var response = await client.GetAsync("/api/Venue/is-owner/99999");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<bool>();
        Assert.False(result);
    }

    #endregion
}
