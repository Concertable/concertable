namespace Core.Entities;

public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }

    public User User { get; set; } = null!;

    public bool IsActive => !IsRevoked && DateTime.UtcNow < Expires;
}
