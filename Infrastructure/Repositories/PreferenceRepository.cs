using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PreferenceRepository : Repository<Preference>, IPreferenceRepository
    {
        public PreferenceRepository(ApplicationDbContext context) : base(context) { }

        public async new Task<IEnumerable<Preference>> GetAllAsync()
        {
            var query = context.Preferences
                .Include(p => p.User)
                .Include(p => p.GenrePreferences)
                    .ThenInclude(gp => gp.Genre);

            return await query.ToListAsync();
        }

        public async new Task<Preference?> GetByIdAsync(int id)
        {
            var query = context.Preferences
                .Include(p => p.User)
                .Include(p => p.GenrePreferences)
                    .ThenInclude(gp => gp.Genre)
                .Where(p => p.Id == id);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Preference?> GetByUserIdAsync(int id)
        {
            var query = context.Preferences
                .Include(p => p.User)
                .Include(p => p.GenrePreferences)
                    .ThenInclude(gp => gp.Genre)
                .Where(p => p.UserId == id);

            return await query.FirstOrDefaultAsync();
        }

    }
}
