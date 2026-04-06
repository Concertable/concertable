using System.Net.Http.Headers;
using Concertable.Application.Requests;
using Concertable.Web.IntegrationTests.Controllers;
using Microsoft.AspNetCore.Http;

namespace Concertable.Web.IntegrationTests.Controllers.Venue;

public static class VenueMappers
{
    public static MultipartFormDataContent ToFormContent(this CreateVenueRequest req)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(req.Name), "Name" },
            { new StringContent(req.About), "About" },
            { new StringContent(req.Latitude.ToString()), "Latitude" },
            { new StringContent(req.Longitude.ToString()), "Longitude" }
        };

        var bannerBytes = new byte[req.Banner.Length];
        req.Banner.OpenReadStream().Read(bannerBytes, 0, bannerBytes.Length);
        var bannerContent = new ByteArrayContent(bannerBytes);
        bannerContent.Headers.ContentType = MediaTypeHeaderValue.Parse(req.Banner.ContentType);
        content.Add(bannerContent, "Banner", req.Banner.FileName);

        return content;
    }

    public static MultipartFormDataContent ToFormContent(this UpdateVenueRequest req)
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
            content.Add(req.Banner.ToFormContent());

        return content;
    }
}
