using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Responses;
using Infrastructure.Helpers;
using Application.DTOs;
using Infrastructure.Factories;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class ArtistRepository : Repository<Artist>, IArtistRepository
    {
        private readonly IReviewRepository reviewRepository;
        private readonly IHeaderRepositoryFactory headerRepositoryFactory;

        public ArtistRepository(ApplicationDbContext context, IReviewRepository reviewRepository, IHeaderRepositoryFactory headerRepositoryFactory) : base(context) 
        {
            this.reviewRepository = reviewRepository;
            this.headerRepositoryFactory = headerRepositoryFactory;
        }

        public async Task<PaginationResponse<ArtistHeaderDto>> GetRawHeadersAsync(SearchParams searchParams)
        {
            Expression<Func<Artist, ArtistHeaderDto>> selector = v => new ArtistHeaderDto
            {
                Id = v.Id,
                Name = v.Name,
                ImageUrl = v.ImageUrl,
                County = v.User.County,
                Town = v.User.Town,
                Latitude = v.User.Latitude,
                Longitude = v.User.Longitude,
            };

            var filters = new List<Expression<Func<Event, bool>>>();

            if (searchParams.Date != null)
            {
                filters.Add(e => e.Application.Listing.StartDate >= searchParams.Date);
            }

            var headerRepository = headerRepositoryFactory.Create(selector);

            return await headerRepository.GetRawHeadersAsync(searchParams);
        }

        public async Task<Artist?> GetByUserIdAsync(int id)
        {
            var query = context.Artists.AsQueryable();
            query = query.Where(v => v.UserId == id);

            return await query.FirstOrDefaultAsync();
        }
    }
}
