using System.Net;
using Concertable.Application.DTOs;
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
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Venue/{TestConstants.VenueId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venue = await response.Content.ReadAsync<VenueDto>();
        Assert.NotNull(venue);
        Assert.Equal(TestConstants.VenueId, venue.Id);
        Assert.Equal("Test Venue", venue.Name);
    }

    [Fact]
    public async Task GetDetailsById_ShouldReturn404_WhenVenueDoesNotExist()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Venue/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region GetDetailsForCurrentUser

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync("/api/Venue/user");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn403_WhenNotVenueManager()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.ArtistManager);

        // Act
        var response = await client.GetAsync("/api/Venue/user");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn200_WhenVenueExists()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        var response = await client.GetAsync("/api/Venue/user");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var venue = await response.Content.ReadAsync<VenueDto>();
        Assert.NotNull(venue);
        Assert.Equal("Test Venue", venue.Name);
    }

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn204_WhenNoVenueExists()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager2);

        // Act
        var response = await client.GetAsync("/api/Venue/user");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();
        var request = BuildCreateRequest();

        // Act
        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenNotVenueManager()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.ArtistManager);
        var request = BuildCreateRequest();

        // Act
        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WithVenueDto_WhenValidRequest()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager2);
        var request = BuildCreateRequest();

        // Act
        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        // Assert
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
        Assert.EndsWith(".jpg", venue.BannerUrl);
        Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(venue.BannerUrl), out _));
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenGeocodingFails()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager2, o => o.UseFailingGeocoding());
        var request = BuildCreateRequest();

        // Act
        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenNameIsEmpty()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager2);
        var request = BuildCreateRequest(name: "");

        // Act
        var response = await client.PostAsync("/api/Venue", request.ToFormContent());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();
        var request = BuildUpdateRequest();

        // Act
        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn403_WhenNotVenueManager()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.ArtistManager);
        var request = BuildUpdateRequest();

        // Act
        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn403_WhenNotOwner()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager2);
        var request = BuildUpdateRequest();

        // Act
        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenVenueDoesNotExist()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildUpdateRequest();

        // Act
        var response = await client.PutAsync("/api/Venue/99999", request.ToFormContent());

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn200_WithUpdatedVenueDto_WhenValidRequest()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildUpdateRequest();

        // Act
        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        // Assert
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
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildUpdateRequest(name: "");

        // Act
        var response = await client.PutAsync($"/api/Venue/{TestConstants.VenueId}", request.ToFormContent());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Approve

    [Fact]
    public async Task Approve_ShouldReturn401_WhenUnauthenticated()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.PatchAsync($"/api/Venue/{TestConstants.VenueId}/approve", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Approve_ShouldReturn403_WhenNotAdmin()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        var response = await client.PatchAsync($"/api/Venue/{TestConstants.VenueId}/approve", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Approve_ShouldReturn404_WhenVenueDoesNotExist()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.Admin);

        // Act
        var response = await client.PatchAsync("/api/Venue/99999/approve", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Approve_ShouldReturn204_AndApproveVenue()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.Admin);

        // Act
        var response = await client.PatchAsync($"/api/Venue/{TestConstants.VenueId}/approve", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var venue = await fixture.CreateClient().GetAsync<VenueDto>($"/api/Venue/{TestConstants.VenueId}");
        Assert.True(venue!.Approved);
    }

    #endregion

    #region IsOwner

    [Fact]
    public async Task IsOwner_ShouldReturnTrue_WhenOwner()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        var response = await client.GetAsync($"/api/Venue/is-owner/{TestConstants.VenueId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<bool>();
        Assert.True(result);
    }

    [Fact]
    public async Task IsOwner_ShouldReturnFalse_WhenNotOwner()
    {
        // Arrange
        var client = fixture.CreateClient(TestConstants.VenueManager);

        // Act
        var response = await client.GetAsync("/api/Venue/is-owner/99999");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<bool>();
        Assert.False(result);
    }

    #endregion
}
