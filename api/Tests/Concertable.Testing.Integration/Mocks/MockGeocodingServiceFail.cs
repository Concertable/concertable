using Concertable.Kernel.Exceptions;
using Concertable.Shared.Geocoding.Application;

namespace Concertable.Testing.Integration.Mocks;

public class MockGeocodingServiceFail : IGeocodingService
{
    public Task<LocationDto> GetLocationAsync(double latitude, double longitude)
        => throw new BadRequestException("County or Town not found");
}
