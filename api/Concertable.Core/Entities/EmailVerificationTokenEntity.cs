using Concertable.Core.Entities.Interfaces;

namespace Concertable.Core.Entities;

public class EmailVerificationTokenEntity : IEntity
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
    public bool IsUsed { get; set; }

    public UserEntity User { get; set; } = null!;

    public bool IsActive => !IsUsed && DateTime.UtcNow < Expires;
}
