using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class PreferenceEntity : IIdEntity
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public double RadiusKm { get; set; }
    public CustomerEntity User { get; set; } = null!;
    public ICollection<GenrePreferenceEntity> GenrePreferences { get; set; } = [];
}
