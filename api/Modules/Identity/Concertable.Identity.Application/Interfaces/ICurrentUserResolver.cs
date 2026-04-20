
namespace Concertable.Identity.Application.Interfaces;

public interface ICurrentUserResolver
{
    Task<UserEntity> ResolveAsync(CancellationToken ct = default);
}
