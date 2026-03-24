using Core.Entities.Interfaces;

namespace Core.Entities;

public class RefreshTokenEntity : IEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }

    public UserEntity User { get; set; } = null!;

    public bool IsActive => !IsRevoked && DateTime.UtcNow < Expires;
}
