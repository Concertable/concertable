using Concertable.Core.Entities;
using Concertable.Identity.Contracts;

namespace Concertable.Identity.Infrastructure;

internal class IdentityModule : IIdentityModule
{
    private readonly IUserRepository userRepository;

    public IdentityModule(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<ManagerDto?> GetManagerAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is not ManagerEntity manager) return null;

        return new ManagerDto
        {
            Id = manager.Id,
            Email = manager.Email,
            Avatar = manager.Avatar,
            StripeAccountId = manager.StripeAccountId
        };
    }

}
