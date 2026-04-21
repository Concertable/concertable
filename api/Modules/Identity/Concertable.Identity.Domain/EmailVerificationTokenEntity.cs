using System.ComponentModel.DataAnnotations.Schema;

namespace Concertable.Identity.Domain;

[Table("EmailVerificationTokens")]
public class EmailVerificationTokenEntity : IIdEntity
{
    private EmailVerificationTokenEntity() { }

    public int Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime Expires { get; private set; }
    public bool IsUsed { get; private set; }

    public UserEntity User { get; set; } = null!;

    public bool IsActive => !IsUsed && DateTime.UtcNow < Expires;

    public static EmailVerificationTokenEntity Create(Guid userId, string token, DateTime expires) => new()
    {
        UserId = userId,
        Token = token,
        Expires = expires
    };

    public void Use() => IsUsed = true;
}
