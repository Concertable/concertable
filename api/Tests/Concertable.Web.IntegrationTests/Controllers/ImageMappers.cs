using System.Net.Http.Headers;
using Concertable.Application.DTOs;

namespace Concertable.Web.IntegrationTests.Controllers;

public static class ImageMappers
{
    public static MultipartFormDataContent ToFormContent(this ImageDto image)
    {
        var imageBytes = new byte[image.File.Length];
        image.File.OpenReadStream().Read(imageBytes, 0, imageBytes.Length);
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(image.File.ContentType);

        return new MultipartFormDataContent
        {
            { new StringContent(image.Url), "Image.Url" },
            { fileContent, "Image.File", image.File.FileName }
        };
    }
}
