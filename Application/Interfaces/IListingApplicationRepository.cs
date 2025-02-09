using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IListingApplicationRepository : IRepository<ListingApplication>
    {
        Task<IEnumerable<ListingApplication>> GetAllForListingIdAsync(int listingId);
    }
}
