using Concertable.Venue.Application.Requests;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Concertable.Web.IntegrationTests.Controllers.Venue;

internal static class VenueRequestBuilders
{
    internal static CreateVenueRequest BuildCreateRequest(
        string name = "New Venue",
        string about = "About the venue",
        double latitude = 51.5,
        double longitude = -0.1)
    {
        using var image = new Image<Rgba32>(1000, 250);
        var bannerStream = new MemoryStream();
        image.SaveAsJpeg(bannerStream);
        var imageBytes = bannerStream.ToArray();
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
            Banner = file
        };
    }

    internal static UpdateVenueRequest BuildUpdateRequest(
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
