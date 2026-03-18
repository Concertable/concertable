using Application.Interfaces;
using Application.Models;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Concert;

public interface IConcertApplicationRepository : IRepository<ConcertApplicationEntity>
{
    Task<IEnumerable<ConcertApplicationEntity>> GetByOpportunityIdAsync(int opportunityId);
    Task<IEnumerable<ConcertApplicationEntity>> GetPendingByArtistIdAsync(int id);
    Task<IEnumerable<ConcertApplicationEntity>> GetRecentDeniedByArtistIdAsync(int id);
    Task<(ArtistEntity, VenueEntity)?> GetArtistAndVenueByIdAsync(int id);
}
