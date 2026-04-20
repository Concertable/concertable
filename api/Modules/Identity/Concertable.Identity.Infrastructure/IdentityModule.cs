using Concertable.Core.Entities;
using Concertable.Identity.Contracts;

namespace Concertable.Identity.Infrastructure;

internal class IdentityModule(
    IUserRepository userRepository,
    IManagerRepository<ArtistManagerEntity> artistManagerRepository,
    IManagerRepository<VenueManagerEntity> venueManagerRepository) : IManagerModule
{
    public async Task<ManagerDto?> GetManagerAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is not ManagerEntity manager) return null;
        return ToDto(manager);
    }

    public async Task<ManagerDto?> GetVenueManagerByConcertIdAsync(int concertId)
    {
        var manager = await venueManagerRepository.GetByConcertIdAsync(concertId);
        return manager is null ? null : ToDto(manager);
    }

    public async Task<ManagerDto?> GetArtistManagerByConcertIdAsync(int concertId)
    {
        var manager = await artistManagerRepository.GetByConcertIdAsync(concertId);
        return manager is null ? null : ToDto(manager);
    }

    public async Task<ManagerDto?> GetVenueManagerByApplicationIdAsync(int applicationId)
    {
        var manager = await venueManagerRepository.GetByApplicationIdAsync(applicationId);
        return manager is null ? null : ToDto(manager);
    }

    public async Task<ManagerDto?> GetArtistManagerByApplicationIdAsync(int applicationId)
    {
        var manager = await artistManagerRepository.GetByApplicationIdAsync(applicationId);
        return manager is null ? null : ToDto(manager);
    }

    private static ManagerDto ToDto(ManagerEntity manager) => new()
    {
        Id = manager.Id,
        Email = manager.Email,
        Avatar = manager.Avatar,
        StripeAccountId = manager.StripeAccountId,
        StripeCustomerId = manager.StripeCustomerId
    };
}
