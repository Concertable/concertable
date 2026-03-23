using Application.DTOs;
using Application.Requests;
using Core.Entities;

namespace Application.Interfaces;

public interface IArtistService
{
    Task<ArtistDto> GetDetailsByIdAsync(int id);
    Task<ArtistDto?> GetDetailsForCurrentUserAsync();
    Task<ArtistDto> CreateAsync(CreateArtistRequest request);
    Task<ArtistDto> UpdateAsync(int id, UpdateArtistRequest request);
    Task<int> GetIdForCurrentUserAsync();
}
