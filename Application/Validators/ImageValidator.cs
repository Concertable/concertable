using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public static class ImageValidator
    {
        private static readonly string[] mimeTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp"
        };

        public static bool Validate(IFormFile file, out string? error)
        {
            error = null;
            if (!mimeTypes.Contains(file.ContentType))
            {
                error = "Unsupported image format";
                return false;
            }

            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                error = "Image file exceeds the maximum size of 5MB";
                return false;
            }

            return true;
        }
    }
}
