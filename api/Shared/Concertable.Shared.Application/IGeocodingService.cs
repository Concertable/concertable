using Concertable.Application.DTOs;

namespace Concertable.Application.Interfaces;

public interface IGeocodingService
{
    Task<LocationDto> GetLocationAsync(double latitude, double longitude);
}
