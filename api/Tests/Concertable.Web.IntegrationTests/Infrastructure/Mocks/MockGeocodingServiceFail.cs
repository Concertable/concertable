using Concertable.Application.DTOs;
using Concertable.Application.Interfaces;
using Concertable.Application.Exceptions;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public class MockGeocodingServiceFail : IGeocodingService
{
    public Task<LocationDto> GetLocationAsync(double latitude, double longitude)
        => throw new BadRequestException("County or Town not found");
}
