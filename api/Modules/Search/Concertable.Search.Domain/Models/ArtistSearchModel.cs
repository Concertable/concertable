namespace Concertable.Search.Domain.Models;

public sealed class ArtistSearchModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Avatar { get; set; }
}
