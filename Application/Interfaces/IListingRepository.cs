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
        Task<IEnumerable<Listing>> GetActiveByVenueIdAsync(int id);

        Task<VenueManager> GetOwnerByIdAsync(int id);
    }
}
