using Concertable.Application.Interfaces;
using Concertable.Application.Models;
using Concertable.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces.Concert;

public interface IOpportunityApplicationRepository : IRepository<OpportunityApplicationEntity>
{
    Task<IEnumerable<OpportunityApplicationEntity>> GetByOpportunityIdAsync(int opportunityId);
    Task<IEnumerable<OpportunityApplicationEntity>> GetPendingByArtistIdAsync(int id);
    Task<IEnumerable<OpportunityApplicationEntity>> GetRecentDeniedByArtistIdAsync(int id);
    Task<(ArtistEntity, VenueEntity)?> GetArtistAndVenueByIdAsync(int id);
    Task<OpportunityApplicationEntity?> GetByConcertIdAsync(int concertId);
    Task RejectAllExceptAsync(int opportunityId, int applicationId);
}
