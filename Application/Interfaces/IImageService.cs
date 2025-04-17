using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file);
        Task DeleteAsync(string imageUrl);
        Task<string> ReplaceAsync(IFormFile newFile, string? oldImageUrl = null);
    }

}
