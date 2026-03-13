using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Core.Entities;

namespace Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepsitory;
    private readonly ICurrentUser currentUser;
    private readonly IGeocodingService geocodingService;
    private readonly IGeometryProvider geometryProvider;

    public UserService(
        IUserRepository userRepsitory,
        ICurrentUser currentUser,
        IGeocodingService geocodingService,
        IGeometryProvider geometryProvider)
    {
        this.userRepsitory = userRepsitory;
        this.currentUser = currentUser;
        this.geocodingService = geocodingService;
        this.geometryProvider = geometryProvider;
    }

    public async Task<int> GetIdByApplicationIdAsync(int applicationId)
    {
        return await userRepsitory.GetIdByApplicationIdAsync(applicationId);
    }

    public async Task<User> GetByApplicationIdAsync(int applicationId)
    {
        return await userRepsitory.GetByApplicationIdAsync(applicationId);
    }

    public async Task<int> GetIdByConcertIdAsync(int concertId)
    {
        return await userRepsitory.GetIdByConcertIdAsync(concertId);
    }

    public async Task<User> GetByConcertIdAsync(int concertId)
    {
        return await userRepsitory.GetByConcertIdAsync(concertId);
    }

    public async Task<UserDto> UpdateLocationAsync(double latitude, double longitude)
    {
        var user = currentUser.GetEntity();

        user.Location = geometryProvider.CreatePoint(latitude, longitude);

        var locationDto = await geocodingService.GetLocationAsync(latitude, longitude);

        user.County = locationDto.County;
        user.Town = locationDto.Town;

        userRepsitory.Update(user);
        await userRepsitory.SaveChangesAsync();

        return user.ToDto();
    }
}
