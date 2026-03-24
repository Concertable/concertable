using Core.Entities.Interfaces;

namespace Core.Entities;

public class PreferenceEntity : IEntity
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public double RadiusKm { get; set; }
    public UserEntity User { get; set; } = null!;
    public ICollection<GenrePreferenceEntity> GenrePreferences { get; set; } = [];
}
