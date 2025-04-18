using Application.Models;
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
        Task<IEnumerable<ListingApplication>> GetByListingIdAsync(int listingId);
        Task<IEnumerable<ListingApplication>> GetPendingByArtistIdAsync(int id);
        Task<IEnumerable<ListingApplication>> GetRecentDeniedByArtistIdAsync(int id);
        Task<(Artist, Venue)> GetArtistAndVenueByIdAsync(int id);
        Task<decimal> GetListingPayByIdAsync(int id);
    }
}
