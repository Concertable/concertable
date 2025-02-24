using Application.DTOs;
using Application.Interfaces;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context) { }

        public async Task<double?> GetAverageRatingByArtistIdAsync(int id)
        {
            var query = context.Reviews
                .Where(r => r.Ticket.Event.Application.ArtistId == id);

            return await query.AverageAsync(r => (double?)r.Stars);
        }

        public async Task<double?> GetAverageRatingByEventIdAsync(int id)
        {
            var query = context.Reviews
                .Where(r => r.Ticket.EventId == id);

            return await query.AverageAsync(r => (double?)r.Stars);
        }

        public async Task<double?> GetAverageRatingByVenueIdAsync(int id)
        {
            var query = context.Reviews
                .Where(r => r.Ticket.Event.Application.Listing.VenueId == id);
            
            return await query.AverageAsync(r => (double?)r.Stars);
        }

        public async Task<PaginationResponse<Review>> GetByArtistIdAsync(int id, PaginationParams pageParams)
        {
            var query = context.Reviews
                .Include(r => r.Ticket)
                .ThenInclude(t => t.User)
                .Where(r => r.Ticket.Event.Application.ArtistId == id);

            return await PaginationHelper.CreatePaginatedResponseAsync(query, pageParams);
        }

        public async Task<PaginationResponse<Review>> GetByEventIdAsync(int id, PaginationParams pageParams)
        {
            var query = context.Reviews
                .Include(r => r.Ticket)
                .ThenInclude(t => t.User)
                .Where(r => r.Ticket.EventId == id);

            return await PaginationHelper.CreatePaginatedResponseAsync(query, pageParams);
        }

        public async Task<PaginationResponse<Review>> GetByVenueIdAsync(int id, PaginationParams pageParams)
        {
            var query = context.Reviews
                .Include(r => r.Ticket)
                .ThenInclude(t => t.User)
                .Where(r => r.Ticket.Event.Application.Listing.VenueId == id);

            return await PaginationHelper.CreatePaginatedResponseAsync(query, pageParams);
        }
    }
}
