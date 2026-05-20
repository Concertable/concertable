namespace Concertable.Shared.Geocoding;

public interface IGeocodingService
{
    Task<LocationDto> GetLocationAsync(double latitude, double longitude);
}
