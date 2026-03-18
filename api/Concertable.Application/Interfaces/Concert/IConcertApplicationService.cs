using Application.DTOs;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Concert;

public interface IConcertApplicationService
{
    Task<ConcertApplicationDto> GetByIdAsync(int id);
    Task<IEnumerable<ConcertApplicationDto>> GetByOpportunityIdAsync(int id);
    /// <summary>
    /// Gets the list of Applications that havent expired, and dont yet have
    /// an event created
    /// </summary>
    Task<IEnumerable<ArtistConcertApplicationDto>> GetPendingForArtistAsync();
    /// <summary>
    /// Gets the list of Applications where another application was accepted
    /// over these ones
    /// </summary>
    Task<IEnumerable<ArtistConcertApplicationDto>> GetRecentDeniedForArtistAsync();
    Task ApplyAsync(int opportunityId);
    Task<(ArtistDto, VenueDto)> GetArtistAndVenueByIdAsync(int id);
}
