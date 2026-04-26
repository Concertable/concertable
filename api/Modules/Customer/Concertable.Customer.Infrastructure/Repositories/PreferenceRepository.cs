using Concertable.Customer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Customer.Infrastructure.Repositories;

internal class PreferenceRepository : Repository<PreferenceEntity>, IPreferenceRepository
{
    public PreferenceRepository(CustomerDbContext context) : base(context) { }

    public async new Task<IEnumerable<PreferenceEntity>> GetAllAsync()
    {
        return await context.Preferences
            .Include(p => p.GenrePreferences)
                .ThenInclude(gp => gp.Genre)
            .ToListAsync();
    }

    public async new Task<PreferenceEntity?> GetByIdAsync(int id)
    {
        return await context.Preferences
            .Include(p => p.GenrePreferences)
                .ThenInclude(gp => gp.Genre)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PreferenceEntity?> GetByUserIdAsync(Guid id)
    {
        return await context.Preferences
            .Include(p => p.GenrePreferences)
                .ThenInclude(gp => gp.Genre)
            .FirstOrDefaultAsync(p => p.UserId == id);
    }

    public async Task<IEnumerable<PreferenceEntity>> GetByMatchingGenresAsync(IEnumerable<int> genreIds)
    {
        var ids = genreIds.ToArray();
        return await context.Preferences
            .Where(p => p.GenrePreferences.Any(gp => ids.Contains(gp.GenreId)))
            .ToListAsync();
    }
}
