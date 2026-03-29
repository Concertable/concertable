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

        var imageBytes = new byte[req.Image.Length];
        req.Image.OpenReadStream().Read(imageBytes, 0, imageBytes.Length);
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(req.Image.ContentType);
        content.Add(imageContent, "Image", req.Image.FileName);

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

        if (req.Image is not null)
            content.Add(req.Image.ToFormContent());

        return content;
    }
}
