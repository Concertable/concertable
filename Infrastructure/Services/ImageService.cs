using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ImageService : IImageService
    {
        private readonly IBlobStorageService blobStorageService;

        public ImageService(IBlobStorageService blobStorageService)
        {
            this.blobStorageService = blobStorageService;
        }

        public async Task<string> ReplaceAsync(IFormFile newFile, string? oldImageUrl = null)
        {
            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                await blobStorageService.DeleteAsync(oldImageUrl); // Delete the old image
            }

            var extension = Path.GetExtension(newFile.FileName);
            var imageName = $"{Guid.NewGuid()}{extension}";

            using var stream = newFile.OpenReadStream();
            await blobStorageService.UploadAsync(stream, imageName); // Replace with new image

            return imageName;
        }

        public async Task<string> UploadAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName);
            var imageName = $"{Guid.NewGuid()}{extension}";

            using var stream = file.OpenReadStream();
            await blobStorageService.UploadAsync(stream, imageName);

            return imageName;
        }

        public async Task DeleteAsync(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
                await blobStorageService.DeleteAsync(imageUrl);
        }
    }
}
