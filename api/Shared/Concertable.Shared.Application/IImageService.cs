using Microsoft.AspNetCore.Http;

namespace Concertable.Application.Interfaces;

public interface IImageService
{
    Task<string> UploadAsync(IFormFile file);
    Task DeleteAsync(string imageUrl);
    Task<string> ReplaceAsync(IFormFile newFile, string? oldImageUrl = null);
    Task<Stream> DownloadAsync(string imageUrl);
}
