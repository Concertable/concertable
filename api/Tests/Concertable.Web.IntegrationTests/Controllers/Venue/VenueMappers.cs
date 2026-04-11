using System.Net.Http.Headers;
using Concertable.Application.Requests;
using Concertable.Web.IntegrationTests.Controllers;
using Microsoft.AspNetCore.Http;

namespace Concertable.Web.IntegrationTests.Controllers.Venue;

public static class VenueMappers
{
    public static async Task<MultipartFormDataContent> ToFormContent(this CreateVenueRequest req)
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

    public static async Task<MultipartFormDataContent> ToFormContent(this UpdateVenueRequest req)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(req.Name), "Name" },
            { new StringContent(req.About), "About" },
            { new StringContent(req.Latitude.ToString()), "Latitude" },
            { new StringContent(req.Longitude.ToString()), "Longitude" },
            { new StringContent(req.Approved.ToString()), "Approved" }
        };

        if (req.Banner is not null)
            content.Add(await req.Banner.ToFormContent());

        return content;
    }
}
