using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlobController : ControllerBase
    {
        private readonly IBlobStorageService blobStorageService;
        public BlobController(IBlobStorageService blobStorageService)
        {
            this.blobStorageService = blobStorageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file");

            using var stream = file.OpenReadStream();
            await blobStorageService.UploadAsync(stream, file.FileName);

            return Ok("Blob uploaded successfully");
        }

        [HttpGet("download/{blobName}")]
        public async Task<IActionResult> Download(string blobName)
        {
            var stream = await blobStorageService.DownloadAsync(blobName);

            if (stream == null)
                return NotFound("Blob not found");

            return File(stream, "application/octet-stream", blobName);
        }
    }
}
