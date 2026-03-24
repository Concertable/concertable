using Core.Entities;

namespace Application.Interfaces;

public interface IArtistManagerRepository : IGuidRepository<ArtistManagerEntity>
{
    Task<ArtistManagerEntity?> GetByConcertIdAsync(int concertId);
    Task<ArtistManagerEntity?> GetByApplicationIdAsync(int applicationId);
}
