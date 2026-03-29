using Concertable.Core.Entities;
using Concertable.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Concertable.Infrastructure.Data;

namespace Concertable.Infrastructure.Repositories;

public class GenreRepository : Repository<GenreEntity>, IGenreRepository
{
    public GenreRepository(ApplicationDbContext context) : base(context) { }

    public new async Task<IEnumerable<GenreEntity>> GetAllAsync()
    {
        return await base.GetAllAsync();
    }

    public async Task<IEnumerable<GenreEntity>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await context.Genres
            .Where(g => ids.Contains(g.Id))
            .ToListAsync();
    }
}
