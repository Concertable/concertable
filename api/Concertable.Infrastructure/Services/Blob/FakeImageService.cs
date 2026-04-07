using Concertable.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Concertable.Infrastructure.Services.Blob;

public class FakeImageService : IImageService
{
    private static readonly Assembly Assembly = typeof(FakeImageService).Assembly;

    public Task<string> UploadAsync(IFormFile file) => Task.FromResult(file.FileName);

    public Task DeleteAsync(string imageUrl) => Task.CompletedTask;

    public Task<string> ReplaceAsync(IFormFile newFile, string? oldImageUrl = null) =>
        Task.FromResult(newFile.FileName);

    public Task<Stream> DownloadAsync(string imageUrl)
    {
        var resourceName = imageUrl.Contains("avatar")
            ? "Concertable.Infrastructure.Resources.avatar.png"
            : "Concertable.Infrastructure.Resources.banner.png";

        var stream = Assembly.GetManifestResourceStream(resourceName)
            ?? Stream.Null;

        return Task.FromResult(stream);
    }
}
