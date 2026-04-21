using Concertable.Core.Entities;
using Concertable.Identity.Contracts;
using Concertable.Identity.Infrastructure.Extensions;
using Concertable.Shared.Exceptions;

namespace Concertable.Identity.Infrastructure;

internal class IdentityModule(
    IUserRepository userRepository,
    IManagerRepository<ArtistManagerEntity> artistManagerRepository,
    IManagerRepository<VenueManagerEntity> venueManagerRepository) : IManagerModule
{
    public async Task<ManagerDto?> GetManagerAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        return user is ManagerEntity manager ? manager.ToDto() : null;
    }

    public async Task<ManagerDto> GetVenueManagerByConcertIdAsync(int concertId)
    {
        var manager = await venueManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException($"Venue manager not found for concert {concertId}");
        return manager.ToDto();
    }

    public async Task<ManagerDto> GetArtistManagerByConcertIdAsync(int concertId)
    {
        var manager = await artistManagerRepository.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException($"Artist manager not found for concert {concertId}");
        return manager.ToDto();
    }

    public async Task<ManagerDto> GetVenueManagerByApplicationIdAsync(int applicationId)
    {
        var manager = await venueManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException($"Venue manager not found for application {applicationId}");
        return manager.ToDto();
    }

    public async Task<ManagerDto> GetArtistManagerByApplicationIdAsync(int applicationId)
    {
        var manager = await artistManagerRepository.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException($"Artist manager not found for application {applicationId}");
        return manager.ToDto();
    }
}
