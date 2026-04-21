
namespace Concertable.Identity.Application.Interfaces;

internal interface IUserService
{
    Task<UserEntity> GetByApplicationIdAsync(int applicationId);
    Task<UserEntity> GetByConcertIdAsync(int id);
    Task<Guid> GetIdByApplicationIdAsync(int id);
    Task<Guid> GetIdByConcertIdAsync(int id);
    Task<IUser> SaveLocationAsync(double latitude, double longitude);
    Task UpdateLocationAsync(UserEntity user, double latitude, double longitude);

    Task<UserEntity?> GetUserEntityByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
