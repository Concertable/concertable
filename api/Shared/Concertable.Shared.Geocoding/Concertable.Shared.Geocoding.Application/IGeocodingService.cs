namespace Concertable.Shared.Geocoding.Application;

public interface IGeocodingService
{
    Task<LocationDto> GetLocationAsync(double latitude, double longitude);
}
