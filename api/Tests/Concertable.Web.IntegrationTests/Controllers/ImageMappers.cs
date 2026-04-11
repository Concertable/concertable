using System.Net.Http.Headers;
using Concertable.Application.DTOs;

namespace Concertable.Web.IntegrationTests.Controllers;

public static class ImageMappers
{
    public static async Task<MultipartFormDataContent> ToFormContent(this ImageDto image)
    {
        using var imageStream = new MemoryStream();
        await image.File.CopyToAsync(imageStream);
        var fileContent = new ByteArrayContent(imageStream.ToArray());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(image.File.ContentType);

        return new MultipartFormDataContent
        {
            { new StringContent(image.Url), "Image.Url" },
            { fileContent, "Image.File", image.File.FileName }
        };
    }
}
