using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces.Concert;

public interface IConcertApplicationService
{
    Task<IConcertApplication> GetByIdAsync(int id);
    Task<IEnumerable<IConcertApplication>> GetByOpportunityIdAsync(int id);
    Task<IEnumerable<IConcertApplication>> GetPendingForArtistAsync();
    Task<IEnumerable<IConcertApplication>> GetRecentDeniedForArtistAsync();
    Task ApplyAsync(int opportunityId);
    Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id);
}
