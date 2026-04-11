using Concertable.Application.DTOs;
using Concertable.Application.Requests;
using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IArtistService
{
    Task<ArtistDetailsResponse> GetDetailsByIdAsync(int id);
    Task<ArtistDetailsResponse> GetDetailsForCurrentUserAsync();
    Task<ArtistDto> CreateAsync(CreateArtistRequest request);
    Task<ArtistDto> UpdateAsync(int id, UpdateArtistRequest request);
    Task<int> GetIdForCurrentUserAsync();
}
