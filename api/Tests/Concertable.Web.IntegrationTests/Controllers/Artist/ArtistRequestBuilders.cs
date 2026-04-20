using Concertable.Application.Requests;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Concertable.Web.IntegrationTests.Controllers.Artist;

public static class ArtistRequestBuilders
{
    public static CreateArtistRequest BuildCreateRequest(
        string name = "New Artist",
        string about = "About the artist",
        double latitude = 51.5,
        double longitude = -0.1)
    {
        using var image = new Image<Rgba32>(1000, 250);
        var bannerStream = new MemoryStream();
        image.SaveAsJpeg(bannerStream);
        var imageBytes = bannerStream.ToArray();
        var stream = new MemoryStream(imageBytes);
        var file = new FormFile(stream, 0, imageBytes.Length, "Banner", "banner.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        return new CreateArtistRequest
        {
            Name = name,
            About = about,
            Latitude = latitude,
            Longitude = longitude,
            Banner = file
        };
    }

    public static UpdateArtistRequest BuildUpdateRequest(
        string name = "Updated Artist",
        string about = "Updated about",
        double latitude = 51.5,
        double longitude = -0.1) => new()
    {
        Name = name,
        About = about,
        Latitude = latitude,
        Longitude = longitude
    };
}
