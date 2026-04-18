using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Geometry;
using Concertable.Core.Entities;
using Concertable.Core.ValueObjects;
using Concertable.Application.Exceptions;
using Concertable.Infrastructure.Services.Geometry;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Services;

public class UserService : IUserService
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

    public async Task<Guid> GetIdByApplicationIdAsync(int applicationId)
    {
        return await userRepsitory.GetIdByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("User not found for application");
    }

    public async Task<UserEntity> GetByApplicationIdAsync(int applicationId)
    {
        return await userRepsitory.GetByApplicationIdAsync(applicationId)
            ?? throw new NotFoundException("User not found for application");
    }

    public async Task<Guid> GetIdByConcertIdAsync(int concertId)
    {
        return await userRepsitory.GetIdByConcertIdAsync(concertId)
            ?? throw new NotFoundException("User not found for concert");
    }

    public async Task<UserEntity> GetByConcertIdAsync(int concertId)
    {
        return await userRepsitory.GetByConcertIdAsync(concertId)
            ?? throw new NotFoundException("User not found for concert");
    }

    public async Task<IUser> SaveLocationAsync(double latitude, double longitude)
    {
        var user = currentUser.GetEntity();

        var locationDto = await geocodingService.GetLocationAsync(latitude, longitude);
        user.Location = geometryProvider.CreatePoint(latitude, longitude);
        user.Address = new Address(locationDto.County, locationDto.Town);

        userRepsitory.Update(user);
        await userRepsitory.SaveChangesAsync();

        return userMapper.ToDto(user);
    }

    public async Task UpdateLocationAsync(UserEntity user, double latitude, double longitude)
    {
        var location = await geocodingService.GetLocationAsync(latitude, longitude);
        user.Location = geometryProvider.CreatePoint(latitude, longitude);
        user.Address = new Address(location.County, location.Town);
    }

    public async Task<IUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepsitory.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : userMapper.ToDto(user);
    }

    public async Task<UserEntity?> GetUserEntityByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await userRepsitory.GetByIdAsync(userId, cancellationToken);
    }
}
