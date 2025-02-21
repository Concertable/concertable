using Core.Entities;
using Application.Interfaces;
using Infrastructure.Repositories;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Helpers;
using Core.Responses;
using Core.Entities.Identity;
using Application.DTOs;

namespace Infrastructure.Repositories
{
    public class VenueRepository : Repository<Venue>, IVenueRepository
    {
        private readonly IReviewRepository reviewRepository;

        public VenueRepository(ApplicationDbContext context, IReviewRepository reviewRepository) : base(context) 
        {
            this.reviewRepository = reviewRepository;
        }

        public async Task<PaginationResponse<VenueHeaderDto>> GetRawHeadersAsync(SearchParams? searchParams)
        {
            var query = context.Venues
                .Select(v => new VenueHeaderDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    ImageUrl = v.ImageUrl,
                    County = v.User.County,
                    Town = v.User.Town
                });

            if (!string.IsNullOrWhiteSpace(searchParams?.Sort))
            {
                query = query.OrderBy(v => searchParams.Sort);
            }
            return await PaginationHelper.CreatePaginatedResponseAsync(query, searchParams);
        }

        public async Task<Venue> GetByIdAsync(int id)
        {
            var query = context.Venues
                .Where(v => v.Id == id)
                .Include(v => v.User);

            return await query.FirstAsync();
        } 

        public async Task<Venue?> GetByUserIdAsync(int id)
        {
            var query = context.Venues
                .Where(v => v.UserId == id)
                .Include(v => v.User);

            return await query.FirstOrDefaultAsync();
        }
    }
}
