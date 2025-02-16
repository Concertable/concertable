using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBlobStorageService
    {
        Task UploadAsync(Stream content, string blobName);
        Task DeleteAsync(string blobName);
        Task<Stream> DownloadAsync(string blobName);
    }
}
