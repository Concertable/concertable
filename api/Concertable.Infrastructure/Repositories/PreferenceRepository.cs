using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Infrastructure.Repositories;

public class PreferenceRepository : Repository<PreferenceEntity>, IPreferenceRepository
{
    public PreferenceRepository(ApplicationDbContext context) : base(context) { }

    public async new Task<IEnumerable<PreferenceEntity>> GetAllAsync()
    {
        var query = context.Preferences
            .Include(p => p.User)
            .Include(p => p.GenrePreferences)
                .ThenInclude(gp => gp.Genre);

        return await query.ToListAsync();
    }

    public async new Task<PreferenceEntity?> GetByIdAsync(int id)
    {
        var query = context.Preferences
            .Include(p => p.User)
            .Include(p => p.GenrePreferences)
                .ThenInclude(gp => gp.Genre)
            .Where(p => p.Id == id);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<PreferenceEntity?> GetByUserIdAsync(Guid id)
    {
        var query = context.Preferences
            .Include(p => p.User)
            .Include(p => p.GenrePreferences)
                .ThenInclude(gp => gp.Genre)
            .Where(p => p.UserId == id);

        return await query.FirstOrDefaultAsync();
    }

}
