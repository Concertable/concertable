using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces;

public interface IListingRepository : IRepository<Listing>
{
    new Task<Listing?> GetByIdAsync(int id);
    Task<IEnumerable<Listing>> GetActiveByVenueIdAsync(int id);
    Task<Listing?> GetWithVenueByIdAsync(int id);
    Task<Listing?> GetByApplicationIdAsync(int id);
    Task<User?> GetOwnerByIdAsync(int id);
}
