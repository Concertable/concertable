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

        public async Task<IEnumerable<Preference>> GetAllAsync()
        {
            return await context.Preferences
                .Include(p => p.User)
                .ToListAsync();
        }

        public async Task<Preference> GetByIdAsync(int id)
        {
            return await context.Preferences
                .Include(p => p.GenrePreferences)
                    .ThenInclude(gp => gp.Genre)
                .Include(p => p.User)
                .FirstAsync(p => p.Id == id);
        }

        public async Task<Preference?> GetByUserIdAsync(int id)
        {
            return await context.Preferences
                .Where(p => p.UserId == id)
                .Include(p => p.GenrePreferences)
                    .ThenInclude(gp => gp.Genre) 
                .Include(p => p.User)
                .FirstOrDefaultAsync();
        }
    }
}
