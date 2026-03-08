using Application.Interfaces;

namespace Infrastructure.Services;

public class FakeBlobStorageService : IBlobStorageService
{
    public Task UploadAsync(Stream content, string blobName) => Task.CompletedTask;
    public Task DeleteAsync(string blobName) => Task.CompletedTask;
    public Task<Stream> DownloadAsync(string blobName) => Task.FromResult(Stream.Null);
}
