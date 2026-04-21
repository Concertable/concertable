using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Concertable.Infrastructure.Mappers;
using Concertable.Search.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class ArtistRepository : Repository<ArtistEntity>, IArtistRepository
{
    private readonly IRatingSpecification<ArtistEntity> ratingSpecification;

    public ArtistRepository(
        ApplicationDbContext context,
        IRatingSpecification<ArtistEntity> ratingSpecification) : base(context)
    {
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<ArtistEntity?> GetByUserIdAsync(Guid id)
    {
        return await context.Artists
            .Where(v => v.UserId == id)
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
            .FirstOrDefaultAsync();
    }

    public async Task<ArtistEntity?> GetFullByIdAsync(int id)
    {
        return await context.Artists
            .Where(a => a.Id == id)
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
            .FirstOrDefaultAsync();
    }

    public async Task<ArtistSummaryDto?> GetSummaryAsync(int id)
    {
        return await context.Artists
            .Where(a => a.Id == id)
            .ToSummaryDto(ratingSpecification.ApplyAggregate(context.Reviews))
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
        return await context.Artists
            .Where(a => a.Id == id)
            .ToDto(ratingSpecification.ApplyAggregate(context.Reviews))
            .FirstOrDefaultAsync();
    }

    public async Task<ArtistDto?> GetDtoByUserIdAsync(Guid userId)
    {
        return await context.Artists
            .Where(a => a.UserId == userId)
            .ToDto(ratingSpecification.ApplyAggregate(context.Reviews))
            .FirstOrDefaultAsync();
    }
}
