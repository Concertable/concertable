using Application.Interfaces;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient blobServiceClient;
        private readonly string containerName;

        public BlobStorageService(IConfiguration configuration)
        {
            blobServiceClient = new BlobServiceClient(configuration["AzureBlobStorage:ConnectionString"]);
            containerName = configuration["AzureBlobStorage:ContainerName"];
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
}
