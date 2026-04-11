using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Search;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Mappers;
using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class ArtistRepository : Repository<ArtistEntity>, IArtistRepository
{
    private readonly IRatingSpecification<ArtistEntity> ratingSpecification;

    public ArtistRepository(ApplicationDbContext context, IRatingSpecification<ArtistEntity> ratingSpecification) : base(context)
    {
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<ArtistEntity?> GetByUserIdAsync(Guid id)
    {
        return await context.Artists
            .Where(v => v.UserId == id)
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
            .Include(a => a.User)
            .FirstOrDefaultAsync();
    }

    public async Task<ArtistEntity?> GetAggregateByIdAsync(int id)
    {
        return await context.Artists
            .Where(a => a.Id == id)
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
            .Include(a => a.User)
            .FirstOrDefaultAsync();
    }

    public async Task<ArtistSummaryDto?> GetSummaryAsync(int id)
    {
        var ratings = ratingSpecification.ApplyAggregate(context.Reviews);
        return await context.Artists
            .Where(a => a.Id == id)
            .ToSummaryDto(ratings)
            .FirstOrDefaultAsync();
    }

    public async Task<int?> GetIdByUserIdAsync(Guid id)
    {
        return await context.Artists
            .Where(a => a.UserId == id)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<ArtistDto?> GetDtoByIdAsync(int id)
    {
        var ratings = ratingSpecification.ApplyAggregate(context.Reviews);
        return await context.Artists
            .Where(a => a.Id == id)
            .ToDto(ratings)
            .FirstOrDefaultAsync();
    }

    public async Task<ArtistDto?> GetDtoByUserIdAsync(Guid userId)
    {
        var ratings = ratingSpecification.ApplyAggregate(context.Reviews);
        return await context.Artists
            .Where(a => a.UserId == userId)
            .ToDto(ratings)
            .FirstOrDefaultAsync();
    }
}
