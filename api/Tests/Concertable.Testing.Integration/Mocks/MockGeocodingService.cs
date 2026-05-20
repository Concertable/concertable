using Concertable.Shared.Geocoding;

namespace Concertable.Testing.Integration.Mocks;

public class MockGeocodingService : IGeocodingService
{
    public Task<LocationDto> GetLocationAsync(double latitude, double longitude)
        => Task.FromResult(new LocationDto("Test County", "Test Town"));
}
