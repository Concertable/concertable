namespace Concertable.Shared.Geocoding.Application;

public interface IGeocodingClient
{
    Task<LocationDto> GetLocationAsync(double latitude, double longitude);
}
