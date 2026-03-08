using Application.Interfaces;
using Azure.Storage.Blobs;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient blobServiceClient;
    private readonly string containerName;

    public BlobStorageService(IOptions<BlobStorageSettings> options)
    {
        var settings = options.Value;
        blobServiceClient = new BlobServiceClient(settings.ConnectionString);
        containerName = settings.ContainerName!;
    }

    public async Task UploadAsync(Stream stream, string blobName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream, overwrite: true);
    }

    public async Task DeleteAsync(string blobName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<Stream> DownloadAsync(string blobName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }
}
