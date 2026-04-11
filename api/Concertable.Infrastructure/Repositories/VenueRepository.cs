using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Search;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Mappers;
using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class VenueRepository : Repository<VenueEntity>, IVenueRepository
{
    private readonly IRatingSpecification<VenueEntity> ratingSpecification;

    public VenueRepository(ApplicationDbContext context, IRatingSpecification<VenueEntity> ratingSpecification) : base(context)
    {
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<VenueEntity?> GetFullByIdAsync(int id)
    {
        return await context.Venues
            .Where(v => v.Id == id)
            .Include(v => v.User)
            .FirstOrDefaultAsync();
    }

    public async Task<VenueSummaryDto?> GetSummaryAsync(int id)
    {
        var ratings = ratingSpecification.ApplyAggregate(context.Reviews);
        return await context.Venues
            .Where(v => v.Id == id)
            .ToSummaryDto(ratings)
            .FirstOrDefaultAsync();
    }

    public async Task<VenueEntity?> GetByUserIdAsync(Guid id)
    {
        return await context.Venues
            .Where(v => v.UserId == id)
            .Include(v => v.User)
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetIdByUserIdAsync(Guid userId)
    {
        return await context.Venues
            .Where(a => a.UserId == userId)
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<VenueDto?> GetDtoByIdAsync(int id)
    {
        var ratings = ratingSpecification.ApplyAggregate(context.Reviews);
        return await context.Venues
            .Where(v => v.Id == id)
            .ToDto(ratings)
            .FirstOrDefaultAsync();
    }

    public async Task<VenueDto?> GetDtoByUserIdAsync(Guid userId)
    {
        var ratings = ratingSpecification.ApplyAggregate(context.Reviews);
        return await context.Venues
            .Where(v => v.UserId == userId)
            .ToDto(ratings)
            .FirstOrDefaultAsync();
    }
}
