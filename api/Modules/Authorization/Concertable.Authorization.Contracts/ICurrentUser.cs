namespace Concertable.Authorization.Contracts;

public interface ICurrentUser
{
    Guid? Id { get; }
    Role? Role { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
