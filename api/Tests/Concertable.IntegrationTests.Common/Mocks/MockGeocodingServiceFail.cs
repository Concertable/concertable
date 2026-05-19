using Concertable.Application.DTOs;
using Concertable.DataAccess;
using Concertable.Shared.Exceptions;

namespace Concertable.IntegrationTests.Common.Mocks;

public class MockGeocodingServiceFail : IGeocodingService
{
    public Task<LocationDto> GetLocationAsync(double latitude, double longitude)
        => throw new BadRequestException("County or Town not found");
}
