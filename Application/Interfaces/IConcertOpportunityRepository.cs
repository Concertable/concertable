using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces;

public interface IConcertOpportunityRepository : IRepository<ConcertOpportunity>
{
    new Task<ConcertOpportunity?> GetByIdAsync(int id);
    Task<IEnumerable<ConcertOpportunity>> GetActiveByVenueIdAsync(int id);
    Task<ConcertOpportunity?> GetWithVenueByIdAsync(int id);
    Task<ConcertOpportunity?> GetByApplicationIdAsync(int id);
    Task<User?> GetOwnerByIdAsync(int id);
}
