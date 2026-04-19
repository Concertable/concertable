
namespace Concertable.Core.Entities;

public class RefreshTokenEntity : IIdEntity
{
    private RefreshTokenEntity() { }

    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime Expires { get; private set; }
    public bool IsRevoked { get; private set; }

    public UserEntity User { get; set; } = null!;

    public bool IsActive => !IsRevoked && DateTime.UtcNow < Expires;

    public static RefreshTokenEntity Create(Guid userId, string token, DateTime expires) => new()
    {
        UserId = userId,
        Token = token,
        Expires = expires
    };

    public void Revoke() => IsRevoked = true;
}
