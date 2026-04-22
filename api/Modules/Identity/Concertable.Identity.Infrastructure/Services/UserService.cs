using Concertable.Application.Interfaces.Geometry;
using Concertable.Infrastructure.Services.Geometry;
using Concertable.Identity.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Identity.Infrastructure.Services;

internal class UserService : IUserService
{
    private readonly IUserRepository userRepsitory;
    private readonly ICurrentUser currentUser;
    private readonly IGeocodingService geocodingService;
    private readonly IGeometryProvider geometryProvider;
    private readonly IUserMapper userMapper;

    public UserService(
        IUserRepository userRepsitory,
        ICurrentUser currentUser,
        IGeocodingService geocodingService,
        [FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider,
        IUserMapper userMapper)
    {
        this.userRepsitory = userRepsitory;
        this.currentUser = currentUser;
        this.geocodingService = geocodingService;
        this.geometryProvider = geometryProvider;
        this.userMapper = userMapper;
    }

    public async Task<IUser> SaveLocationAsync(double latitude, double longitude)
    {
        var user = await userRepsitory.GetByIdAsync(currentUser.GetId())
            ?? throw new UnauthorizedAccessException("User not found.");

        var locationDto = await geocodingService.GetLocationAsync(latitude, longitude);
        user.UpdateLocation(
            geometryProvider.CreatePoint(latitude, longitude),
            new Address(locationDto.County, locationDto.Town));

        userRepsitory.Update(user);
        await userRepsitory.SaveChangesAsync();

        return userMapper.ToDto(user);
    }

    public async Task UpdateLocationAsync(UserEntity user, double latitude, double longitude)
    {
        var location = await geocodingService.GetLocationAsync(latitude, longitude);
        user.UpdateLocation(
            geometryProvider.CreatePoint(latitude, longitude),
            new Address(location.County, location.Town));
    }

    public async Task<UserEntity?> GetUserEntityByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await userRepsitory.GetByIdAsync(userId, cancellationToken);
    }
}
