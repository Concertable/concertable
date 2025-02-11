using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Core.Responses;

namespace Application.Interfaces
{
    public interface IArtistService
    {
        Task<ArtistDto> GetDetailsByIdAsync(int id);
        Task<PaginationResponse<ArtistHeaderDto>> GetHeadersAsync(SearchParams searchParams);
        Task<ArtistDto?> GetDetailsForCurrentUserAsync();
    }
}
