using System.ComponentModel.DataAnnotations.Schema;

namespace Concertable.Shared;

[Table("Genres")]
public class GenreEntity : IIdEntity
{
    public int Id { get; private set; }
    public required string Name { get; set; }

    public static GenreEntity Create(int id, string name) => new() { Id = id, Name = name };
}
