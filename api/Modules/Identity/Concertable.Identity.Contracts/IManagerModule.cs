namespace Concertable.Identity.Contracts;

public interface IManagerModule
{
    Task<ManagerDto?> GetManagerAsync(Guid userId);
    Task<ManagerDto> GetVenueManagerByConcertIdAsync(int concertId);
    Task<ManagerDto> GetArtistManagerByConcertIdAsync(int concertId);
    Task<ManagerDto> GetVenueManagerByApplicationIdAsync(int applicationId);
    Task<ManagerDto> GetArtistManagerByApplicationIdAsync(int applicationId);
}
