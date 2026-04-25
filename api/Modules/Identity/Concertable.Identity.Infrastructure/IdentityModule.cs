using Concertable.Identity.Application.Mappers;
using Concertable.Identity.Contracts;
using Concertable.Identity.Infrastructure.Extensions;

namespace Concertable.Identity.Infrastructure;

internal class IdentityModule(IUserRepository userRepository, IUserMapper userMapper) : IManagerModule, IIdentityModule
{
    public async Task<ManagerDto?> GetByIdAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user is ManagerEntity manager ? manager.ToDto() : null;
    }

    public async Task<IUser?> GetUserByIdAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user is null ? null : userMapper.ToDto(user);
    }
}
