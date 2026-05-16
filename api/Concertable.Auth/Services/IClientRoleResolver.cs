using Concertable.User.Contracts;

namespace Concertable.Auth.Services;

public interface IClientRoleResolver
{
    Task<Role?> ResolveFromReturnUrlAsync(string? returnUrl);
    Role? ResolveFromClientId(string? clientId);
    Role? ResolveFromHint(string? hint);
}
