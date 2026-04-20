using Concertable.Application.Interfaces;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Services;

internal class CurrentUserResolver : ICurrentUserResolver
{
    private readonly ICurrentUser currentUser;
    private readonly IUserRepository userRepository;

    public CurrentUserResolver(ICurrentUser currentUser, IUserRepository userRepository)
    {
        this.currentUser = currentUser;
        this.userRepository = userRepository;
    }

    public async Task<UserEntity> ResolveAsync(CancellationToken ct = default) =>
        await userRepository.GetByIdAsync(currentUser.GetId(), ct)
            ?? throw new UnauthorizedAccessException("User not found.");
}
