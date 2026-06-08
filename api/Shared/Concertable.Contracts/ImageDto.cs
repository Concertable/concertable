using Microsoft.AspNetCore.Http;

namespace Concertable.Contracts;

public sealed record ImageDto
{
    public required string Url { get; init; }
    public required IFormFile File { get; init; }
}
