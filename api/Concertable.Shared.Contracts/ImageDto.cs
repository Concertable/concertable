using Microsoft.AspNetCore.Http;

namespace Concertable.Shared;

public record ImageDto
{
    public required string Url { get; init; }
    public required IFormFile File { get; init; }
}
