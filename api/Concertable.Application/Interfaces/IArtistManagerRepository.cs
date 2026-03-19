using Core.Entities;
using Core.Interfaces;

namespace Application.Interfaces;

public interface IArtistManagerRepository : IRepository<ArtistManagerEntity>
{
    Task<ArtistManagerEntity?> GetByConcertIdAsync(int concertId);
}
