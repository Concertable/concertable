using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Mappers;
using Concertable.Search.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class VenueRepository : Repository<VenueEntity>, IVenueRepository
{
    private readonly IRatingSpecification<VenueEntity> ratingSpecification;

    public VenueRepository(
        ApplicationDbContext context,
        IRatingSpecification<VenueEntity> ratingSpecification) : base(context)
    {
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<VenueEntity?> GetFullByIdAsync(int id)
    {
        return await context.Venues
            .Where(v => v.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<VenueSummaryDto?> GetSummaryAsync(int id)
    {
        return await context.Venues
            .Where(v => v.Id == id)
            .ToSummaryDto(ratingSpecification.ApplyAggregate(context.Reviews))
            .FirstOrDefaultAsync();
    }

    public async Task<VenueEntity?> GetByUserIdAsync(Guid id)
    {
        return await context.Venues
            .Where(v => v.UserId == id)
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
        return await context.Venues
            .Where(v => v.Id == id)
            .ToDto(ratingSpecification.ApplyAggregate(context.Reviews))
            .FirstOrDefaultAsync();
    }

    public async Task<VenueDto?> GetDtoByUserIdAsync(Guid userId)
    {
        return await context.Venues
            .Where(v => v.UserId == userId)
            .ToDto(ratingSpecification.ApplyAggregate(context.Reviews))
            .FirstOrDefaultAsync();
    }
}
