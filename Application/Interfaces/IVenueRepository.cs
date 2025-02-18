using Application.DTOs;
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
        Task<PaginationResponse<VenueHeaderDto>> GetRawHeadersAsync(SearchParams? searchParams);
        Task<Venue?> GetByUserIdAsync(int id);
    }
}
