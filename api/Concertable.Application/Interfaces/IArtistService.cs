using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IArtistService
{
    Task<ArtistDto> GetDetailsByIdAsync(int id);
    Task<ArtistDto> GetDetailsForCurrentUserAsync();
    Task<ArtistDto> CreateAsync(CreateArtistRequest request);
    Task<ArtistDto> UpdateAsync(int id, UpdateArtistRequest request);
    Task<int> GetIdForCurrentUserAsync();
}
