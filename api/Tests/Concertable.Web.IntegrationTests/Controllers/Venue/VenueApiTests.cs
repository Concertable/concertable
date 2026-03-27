using System.Net;
using System.Net.Http.Headers;
using Application.DTOs;
using Concertable.Web.IntegrationTests.Infrastructure;
using Xunit;

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

    private static MultipartFormDataContent BuildCreateRequest(
        string name = "New Venue",
        string about = "About the venue",
        double latitude = 51.5,
        double longitude = -0.1)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent(about), "About" },
            { new StringContent(latitude.ToString()), "Latitude" },
            { new StringContent(longitude.ToString()), "Longitude" }
        };

        var imageContent = new ByteArrayContent([0xFF, 0xD8, 0xFF]);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "Image", "image.jpg");

        return content;
    }

    #region GetDetailsById

    #endregion

    #region GetDetailsForCurrentUser

    #endregion

    #region Create

    [Fact]
    public async Task Create_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();

        var response = await client.PostAsync("/api/Venue", BuildCreateRequest());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenNotVenueManager()
    {
        var client = fixture.CreateClient(TestConstants.ArtistManager);

        var response = await client.PostAsync("/api/Venue", BuildCreateRequest());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WhenValidRequest()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager2);

        var response = await client.PostAsync("/api/Venue", BuildCreateRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var venue = await response.Content.ReadAsync<VenueDto>();
        Assert.NotNull(venue);
        Assert.Equal("New Venue", venue.Name);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenGeocodingFails()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager2, o => o.UseFailingGeocoding());

        var response = await client.PostAsync("/api/Venue", BuildCreateRequest());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenNameIsEmpty()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager2);

        var response = await client.PostAsync("/api/Venue", BuildCreateRequest(name: ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update

    #endregion

    #region IsOwner

    #endregion
}
