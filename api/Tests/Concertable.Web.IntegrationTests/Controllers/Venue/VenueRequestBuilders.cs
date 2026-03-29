using Concertable.Application.Requests;
using Microsoft.AspNetCore.Http;

namespace Concertable.Web.IntegrationTests.Controllers.Venue;

public static class VenueRequestBuilders
{
    public static CreateVenueRequest BuildCreateRequest(
        string name = "New Venue",
        string about = "About the venue",
        double latitude = 51.5,
        double longitude = -0.1)
    {
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF };
        var stream = new MemoryStream(imageBytes);
        var file = new FormFile(stream, 0, imageBytes.Length, "Image", "image.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        return new CreateVenueRequest
        {
            Name = name,
            About = about,
            Latitude = latitude,
            Longitude = longitude,
            Image = file
        };
    }

    public static UpdateVenueRequest BuildUpdateRequest(
        string name = "Updated Venue",
        string about = "Updated about",
        double latitude = 51.5,
        double longitude = -0.1,
        bool approved = false) => new()
    {
        Name = name,
        About = about,
        Latitude = latitude,
        Longitude = longitude,
        Approved = approved
    };
}
