using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IArtistManagerRepository : IGuidRepository<ArtistManagerEntity>
{
    Task<ArtistManagerEntity?> GetByConcertIdAsync(int concertId);
    Task<ArtistManagerEntity?> GetByApplicationIdAsync(int applicationId);
}
