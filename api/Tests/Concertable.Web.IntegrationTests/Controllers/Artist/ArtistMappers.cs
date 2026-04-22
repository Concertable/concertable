using System.Net.Http.Headers;
using Concertable.Artist.Application.Requests;

namespace Concertable.Web.IntegrationTests.Controllers.Artist;

internal static class ArtistMappers
{
    internal static async Task<MultipartFormDataContent> ToFormContent(this CreateArtistRequest req)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(req.Name), "Name" },
            { new StringContent(req.About), "About" },
            { new StringContent(req.Latitude.ToString()), "Latitude" },
            { new StringContent(req.Longitude.ToString()), "Longitude" }
        };

        using var bannerStream = new MemoryStream();
        await req.Banner.CopyToAsync(bannerStream);
        var bannerContent = new ByteArrayContent(bannerStream.ToArray());
        bannerContent.Headers.ContentType = MediaTypeHeaderValue.Parse(req.Banner.ContentType);
        content.Add(bannerContent, "Banner", req.Banner.FileName);

        return content;
    }

    internal static Task<MultipartFormDataContent> ToFormContent(this UpdateArtistRequest req)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(req.Name), "Name" },
            { new StringContent(req.About), "About" },
            { new StringContent(req.Latitude.ToString()), "Latitude" },
            { new StringContent(req.Longitude.ToString()), "Longitude" }
        };

        return Task.FromResult(content);
    }
}
