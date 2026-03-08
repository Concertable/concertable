using Application.DTOs;
using Application.Interfaces.Search;
using Application.Mappers;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Search
{
    public class ConcertHeaderRepository : IConcertHeaderRepository
    {
        private readonly ApplicationDbContext context;
        private readonly IConcertSearchSpecification specification;

        public ConcertHeaderRepository(ApplicationDbContext context, IConcertSearchSpecification specification)
        {
            this.context = context;
            this.specification = specification;
        }

        public async Task<Pagination<Concert>> SearchAsync(SearchParams searchParams)
        {
            var query = specification.Apply(context.Concerts.AsQueryable(), searchParams);
            return await query.ToPaginationAsync(searchParams);
        }

        public async Task<IEnumerable<ConcertHeaderDto>> GetByAmountAsync(int amount)
        {
            var concerts = await context.Concerts
                .Where(e => e.DatePosted != null)
                .Where(e => e.Application.Listing.EndDate > DateTime.UtcNow)
                .OrderByDescending(e => e.DatePosted)
                .Take(amount)
                .ToListAsync();

            return concerts.ToHeaderDtos();
        }

        public async Task<IEnumerable<ConcertHeaderDto>> GetPopularAsync()
        {
            var concerts = await context.Concerts
                .Where(e => e.DatePosted != null)
                .Where(e => e.Application.Listing.EndDate > DateTime.UtcNow)
                .OrderByDescending(e => e.TotalTickets - e.AvailableTickets)
                .Take(10)
                .ToListAsync();

            return concerts.ToHeaderDtos();
        }

        public async Task<IEnumerable<ConcertHeaderDto>> GetFreeAsync()
        {
            var concerts = await context.Concerts
                .Where(e => e.DatePosted != null)
                .Where(e => e.Application.Listing.EndDate > DateTime.UtcNow)
                .Where(e => e.Price == 0)
                .OrderByDescending(e => e.DatePosted)
                .Take(10)
                .ToListAsync();

            return concerts.ToHeaderDtos();
        }
    }
}
