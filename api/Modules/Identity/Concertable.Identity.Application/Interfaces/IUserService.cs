
namespace Concertable.Identity.Application.Interfaces;

internal interface IUserService
{
    Task<IUser> SaveLocationAsync(double latitude, double longitude);
    Task UpdateLocationAsync(UserEntity user, double latitude, double longitude);
    Task<UserEntity?> GetUserEntityByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
