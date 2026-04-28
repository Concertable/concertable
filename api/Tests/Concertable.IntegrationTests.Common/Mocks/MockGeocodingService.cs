using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;

namespace Concertable.IntegrationTests.Common.Mocks;

public class MockGeocodingService : IGeocodingService
{
    public Task<LocationDto> GetLocationAsync(double latitude, double longitude)
        => Task.FromResult(new LocationDto("Test County", "Test Town"));
}
