using Concertable.Application.Interfaces;
using Concertable.Data.Infrastructure.Data;
using Concertable.Shared;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Data.Infrastructure.Repositories;

internal class GenreRepository(SharedDbContext context)
    : Repository<GenreEntity, SharedDbContext>(context), IGenreRepository
{
    public async Task<IEnumerable<GenreEntity>> GetByIdsAsync(IEnumerable<int> ids)
        => await context.Genres.Where(g => ids.Contains(g.Id)).ToListAsync();
}
