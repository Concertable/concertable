using Core.Entities;
using Core.Parameters;
using Core.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IArtistRepository : IRepository<Artist>
    {
        Task<PaginationResponse<Artist>> GetHeadersAsync(SearchParams searchParams);
        Task<Artist?> GetByUserIdAsync(int id);
    }
}
