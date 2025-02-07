using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class Repository<T> : BaseRepository<T>, IRepository<T> where T : BaseEntity
    {
        public Repository(ApplicationDbContext context) : base(context) { }

        public async Task<T?> GetByIdAsync(int id) => await context.Set<T>().FindAsync(id);

        public bool Exists(int id) => context.Set<T>().Any(e => e.Id == id);
    }
}
