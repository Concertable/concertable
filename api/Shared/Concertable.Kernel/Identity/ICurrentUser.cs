namespace Concertable.Shared.Infrastructure.Identity;

public interface ICurrentUser
{
    Guid? Id { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
