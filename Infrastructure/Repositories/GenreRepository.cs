using Core.Entities;
using Application.Interfaces;
using Infrastructure.Data.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class GenreRepository : Repository<Genre>, IGenreRepository
    {
        public GenreRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }

        public async Task<IEnumerable<Genre>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await context.Genres
                .Where(g => ids.Contains(g.Id))
                .ToListAsync();
        }
    }
}
