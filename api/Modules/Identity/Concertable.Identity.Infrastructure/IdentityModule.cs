using Concertable.Identity.Contracts;
using Concertable.Identity.Infrastructure.Extensions;

namespace Concertable.Identity.Infrastructure;

internal class IdentityModule(IUserRepository userRepository) : IManagerModule
{
    public async Task<ManagerDto?> GetByIdAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user is ManagerEntity manager ? manager.ToDto() : null;
    }
}
