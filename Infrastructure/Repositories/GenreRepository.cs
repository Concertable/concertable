using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenreRepository : Repository<Genre>, IGenreRepository
    {
        public GenreRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Genre>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }
    }
}
