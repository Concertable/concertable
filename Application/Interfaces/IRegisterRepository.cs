using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRegisterRepository : IRepository<ListingApplication>
    {
        Task<IEnumerable<ListingApplication>> GetAllForListingIdAsync(int listingId);
    }
}
