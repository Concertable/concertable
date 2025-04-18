using Application.DTOs;
using Core.Entities;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IListingRepository : IRepository<Listing>
    {
        new Task<Listing?> GetByIdAsync(int id);
        Task<IEnumerable<Listing>> GetActiveByVenueIdAsync(int id);
        Task<Listing?> GetWithVenueByIdAsync(int id);
        Task<Listing?> GetByApplicationIdAsync(int id);
        Task<VenueManager> GetOwnerByIdAsync(int id);
    }
}
