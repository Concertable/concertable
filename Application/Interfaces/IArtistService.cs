using Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces
{
    public interface IArtistService
    {
        Task<ArtistDto> GetDetailsByIdAsync(int id);
        Task<ArtistDto?> GetDetailsForCurrentUserAsync();
        Task<ArtistDto> CreateAsync(CreateArtistDto createArtistDto, IFormFile image);
        Task<ArtistDto> UpdateAsync(ArtistDto artistDto, IFormFile? image);
        Task<int> GetIdForCurrentUserAsync();
    }
}
