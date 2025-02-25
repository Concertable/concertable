using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Responses;

namespace Application.Interfaces
{
    public interface IArtistService : IHeaderService<ArtistHeaderDto>
    {
        Task<ArtistDto> GetDetailsByIdAsync(int id);
        Task<PaginationResponse<ArtistHeaderDto>> GetHeadersAsync(SearchParams searchParams);
        Task<ArtistDto?> GetDetailsForCurrentUserAsync();
        Task<ArtistDto> CreateAsync(CreateArtistDto createArtistDto);
    }
}
