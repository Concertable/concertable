namespace Core.Entities;

public class Preference : BaseEntity
{
    public int UserId { get; set; }
    public double RadiusKm { get; set; }
    public User User { get; set; } = null!;
    public ICollection<GenrePreference> GenrePreferences { get; set; } = new List<GenrePreference>();
}
