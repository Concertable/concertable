using Application.Models;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IConcertApplicationRepository : IRepository<ConcertApplication>
{
    Task<IEnumerable<ConcertApplication>> GetByOpportunityIdAsync(int opportunityId);
    Task<IEnumerable<ConcertApplication>> GetPendingByArtistIdAsync(int id);
    Task<IEnumerable<ConcertApplication>> GetRecentDeniedByArtistIdAsync(int id);
    Task<(Artist, Venue)?> GetArtistAndVenueByIdAsync(int id);
    Task<decimal?> GetOpportunityPayByIdAsync(int id);
}
