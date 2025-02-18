using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
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
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<double?> GetRatingByArtistIdAsync(int id)
        {
            var query = context.Reviews
                .Where(r => r.Ticket.Event.Application.ArtistId == id);

            return await query.AverageAsync(r => (double?)r.Stars);
        }

        public async Task<double?> GetRatingByEventIdAsync(int id)
        {
            var query = context.Reviews
                .Where(r => r.Ticket.EventId == id);

            return await query.AverageAsync(r => (double?)r.Stars);
        }

        public async Task<double?> GetRatingByVenueIdAsync(int id)
        {
            var query = context.Reviews
                .Where(r => r.Ticket.Event.Application.Listing.VenueId == id);
            
            return await query.AverageAsync(r => (double?)r.Stars);
        }
    }
}
