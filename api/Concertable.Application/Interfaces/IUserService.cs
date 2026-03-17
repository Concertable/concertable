using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces;

public interface IUserService
{
    Task<UserEntity> GetByApplicationIdAsync(int applicationId);
    Task<UserEntity> GetByConcertIdAsync(int id);
    Task<int> GetIdByApplicationIdAsync(int id);
    Task<int> GetIdByConcertIdAsync(int id);
    Task<UserDto> UpdateLocationAsync(double latitude, double longitude);

    /// <summary>Gets user DTO by id. Returns null if not found.</summary>
    Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Gets user entity by id. Returns null if not found.</summary>
    Task<UserEntity?> GetUserEntityByIdAsync(int userId, CancellationToken cancellationToken = default);
}
