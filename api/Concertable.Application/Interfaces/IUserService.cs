using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces;

public interface IUserService
{
    Task<UserEntity> GetByApplicationIdAsync(int applicationId);
    Task<UserEntity> GetByConcertIdAsync(int id);
    Task<Guid> GetIdByApplicationIdAsync(int id);
    Task<Guid> GetIdByConcertIdAsync(int id);
    Task<IUser> UpdateLocationAsync(double latitude, double longitude);

    /// <summary>Gets user DTO by id. Returns null if not found.</summary>
    Task<IUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Gets user entity by id. Returns null if not found.</summary>
    Task<UserEntity?> GetUserEntityByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
