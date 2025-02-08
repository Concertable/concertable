using Core.Entities;
using Core.Entities.Identity;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Core.Responses;

namespace Application.Interfaces
{
    public interface IVenueService
    {
        Task<PaginationResponse<VenueHeaderDto>> GetHeadersAsync(SearchParams? searchParams);

        Task<Venue> GetDetailsByIdAsync(int id);

        Task<VenueDto?> GetUserVenueAsync();

        Task<VenueDto> CreateAsync(CreateVenueDto venue);
    }
}
