using Microsoft.EntityFrameworkCore;

namespace Concertable.Core.Entities;

[PrimaryKey(nameof(ConcertId), nameof(GenreId))]
public class ConcertGenreEntity : IEquatable<ConcertGenreEntity>
{
    public int ConcertId { get; set; }
    public int GenreId { get; set; }
    public ConcertEntity Concert { get; set; } = null!;
    public GenreEntity Genre { get; set; } = null!;

    public bool Equals(ConcertGenreEntity? other) =>
        other is not null && ConcertId == other.ConcertId && GenreId == other.GenreId;

    public override bool Equals(object? obj) => Equals(obj as ConcertGenreEntity);

    public override int GetHashCode() => HashCode.Combine(ConcertId, GenreId);
}
