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

namespace Infrastructure.Repositories
{
    public class ArtistRepository : Repository<Artist>, IArtistRepository
    {
        private readonly IReviewRepository reviewRepository;

        public ArtistRepository(ApplicationDbContext context, IReviewRepository reviewRepository) : base(context) 
        {
            this.reviewRepository = reviewRepository;
        }

        public async Task<PaginationResponse<ArtistHeaderDto>> GetRawHeadersAsync(SearchParams searchParams)
        {
            var query = context.Artists
                .Select(v => new ArtistHeaderDto
                {
                    Id = v.Id,
                    Name = v.Name,
                    ImageUrl = v.ImageUrl,
                    County = v.User.County,
                    Town = v.User.Town,
                });

            if (!string.IsNullOrWhiteSpace(searchParams?.Sort))
            {
                query = query.OrderBy(v => searchParams.Sort);
            }
            return await PaginationHelper.CreatePaginatedResponseAsync(query, searchParams.PageNumber, searchParams.PageSize);
        }

        public async Task<Artist?> GetByUserIdAsync(int id)
        {
            var query = context.Artists.AsQueryable();
            query = query.Where(v => v.UserId == id);

            return await query.FirstOrDefaultAsync();
        }
    }
}
