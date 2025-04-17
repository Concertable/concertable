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
        Task<IEnumerable<ListingApplication>> GetForListingIdAsync(int listingId);
        Task<IEnumerable<ListingApplication>> GetActiveByArtistIdAsync(int listingId);
        Task<(Artist, Venue)> GetArtistAndVenueByIdAsync(int id);
        Task<decimal> GetListingPayByIdAsync(int id);
    }
}
