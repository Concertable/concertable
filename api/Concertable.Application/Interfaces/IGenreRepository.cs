using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface IGenreRepository : IRepository<GenreEntity>
{
    new Task<IEnumerable<GenreEntity>> GetAllAsync();
    Task<IEnumerable<GenreEntity>> GetByIdsAsync(IEnumerable<int> ids);
}
