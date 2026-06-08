using Concertable.Shared.Geocoding.Application;

namespace Concertable.Testing.Integration.Mocks;

public sealed class MockGeocodingService : IGeocodingService
{
    public Task<LocationDto> GetLocationAsync(double latitude, double longitude)
        => Task.FromResult(new LocationDto("Test County", "Test Town"));
}
