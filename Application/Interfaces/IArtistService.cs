using Application.DTOs;
using Application.Requests;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IArtistService
{
    Task<ArtistDto> GetDetailsByIdAsync(int id);
    Task<ArtistDto?> GetDetailsForCurrentUserAsync();
    Task<ArtistDto> CreateAsync(CreateArtistRequest request, IFormFile image);
    Task<ArtistDto> UpdateAsync(ArtistDto artistDto, IFormFile? image);
    Task<int> GetIdForCurrentUserAsync();
}
