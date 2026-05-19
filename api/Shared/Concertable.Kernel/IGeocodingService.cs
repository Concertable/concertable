using Concertable.Application.DTOs;

namespace Concertable.DataAccess;

public interface IGeocodingService
{
    Task<LocationDto> GetLocationAsync(double latitude, double longitude);
}
