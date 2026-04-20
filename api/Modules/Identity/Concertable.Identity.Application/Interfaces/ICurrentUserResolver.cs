
namespace Concertable.Application.Interfaces;

public interface ICurrentUserResolver
{
    Task<UserEntity> ResolveAsync(CancellationToken ct = default);
}
