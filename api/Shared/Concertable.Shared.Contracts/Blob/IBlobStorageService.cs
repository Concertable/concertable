namespace Concertable.Application.Interfaces.Blob;

public interface IBlobStorageService
{
    Task UploadAsync(Stream content, string blobName);
    Task DeleteAsync(string blobName);
    Task<Stream> DownloadAsync(string blobName);
}
