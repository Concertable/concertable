using Concertable.Artist.Infrastructure.Data;
using Concertable.Artist.Infrastructure.Mappers;
using Concertable.Data.Application;
using Concertable.Search.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Artist.Infrastructure.Repositories;

internal class ArtistRepository : IdModuleRepository<ArtistEntity, ArtistDbContext>, IArtistRepository
{
    private readonly IReadDbContext readDb;
    private readonly IRatingSpecification<ArtistEntity> ratingSpecification;

    public ArtistRepository(
        ArtistDbContext context,
        IReadDbContext readDb,
        IRatingSpecification<ArtistEntity> ratingSpecification) : base(context)
    {
        this.readDb = readDb;
        this.ratingSpecification = ratingSpecification;
    }

    public async Task<ArtistEntity?> GetByUserIdAsync(Guid id) =>
        await context.Artists
            .Where(a => a.UserId == id)
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
            .FirstOrDefaultAsync();

    public async Task<ArtistEntity?> GetFullByIdAsync(int id) =>
        await context.Artists
            .Where(a => a.Id == id)
            .Include(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre)
            .FirstOrDefaultAsync();

    public async Task<ArtistSummaryDto?> GetSummaryAsync(int id) =>
        await readDb.Artists
            .Where(a => a.Id == id)
            .ToSummaryDto(ratingSpecification.ApplyAggregate(readDb.Reviews))
            .FirstOrDefaultAsync();

    public async Task<int?> GetIdByUserIdAsync(Guid id) =>
        await readDb.Artists
            .Where(a => a.UserId == id)
            .Select(a => a.Id)
            .FirstOrDefaultAsync();

    public async Task<ArtistDto?> GetDtoByIdAsync(int id) =>
        await readDb.Artists
            .Where(a => a.Id == id)
            .ToDto(ratingSpecification.ApplyAggregate(readDb.Reviews))
            .FirstOrDefaultAsync();

    public async Task<ArtistDto?> GetDtoByUserIdAsync(Guid userId) =>
        await readDb.Artists
            .Where(a => a.UserId == userId)
            .ToDto(ratingSpecification.ApplyAggregate(readDb.Reviews))
            .FirstOrDefaultAsync();
}
