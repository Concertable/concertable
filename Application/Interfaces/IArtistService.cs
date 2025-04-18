using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Responses;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IArtistService : IHeaderService<ArtistHeaderDto>
    {
        Task<ArtistDto> GetDetailsByIdAsync(int id);
        Task<PaginationResponse<ArtistHeaderDto>> GetHeadersAsync(SearchParams searchParams);
        Task<ArtistDto?> GetDetailsForCurrentUserAsync();
        Task<ArtistDto> CreateAsync(CreateArtistDto createArtistDto, IFormFile image);
        Task<ArtistDto> UpdateAsync(ArtistDto artistDto, IFormFile? image);
        Task<int> GetIdForCurrentUserAsync();
    }
}
