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
    public interface IVenueRepository : IRepository<Venue>
    {
        Task<PaginationResponse<Venue>> GetHeadersAsync(SearchParams? searchParams);
        Task<Venue?> GetByUserIdAsync(int id);
    }
}
