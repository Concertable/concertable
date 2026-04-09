using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class RefreshTokenEntity : IIdEntity
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
    public bool IsRevoked { get; set; }

    public UserEntity User { get; set; } = null!;

    public bool IsActive => !IsRevoked && DateTime.UtcNow < Expires;
}
