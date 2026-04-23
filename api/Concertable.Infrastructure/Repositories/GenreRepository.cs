using Concertable.Application.Interfaces;
using Concertable.Data.Infrastructure;
using Concertable.Data.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

internal class GenreRepository(SharedDbContext context)
    : IdModuleRepository<GenreEntity, SharedDbContext>(context), IGenreRepository
{
    public async Task<IEnumerable<GenreEntity>> GetByIdsAsync(IEnumerable<int> ids)
        => await context.Genres.Where(g => ids.Contains(g.Id)).ToListAsync();
}
