using Application.DTOs;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IListingService
    {
        Task CreateAsync(ListingDto listingDto);
        Task<IEnumerable<ListingDto>> GetActiveByVenueIdAsync(int id);
    }
}
