
namespace Concertable.Identity.Application.Interfaces;

internal interface ICurrentUserResolver
{
    Task<UserEntity> ResolveAsync(CancellationToken ct = default);
}
