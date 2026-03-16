using Application.DTOs;
using Application.Requests;
using Application.Responses;
using Core.Entities;

namespace Application.Interfaces;

public interface IAccountService
{
    Task RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync(string refreshToken);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>Gets user DTO by id. Returns null if not found.</summary>
    Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Gets user entity by id. Returns null if not found.</summary>
    Task<UserEntity?> GetUserEntityByIdAsync(int userId, CancellationToken cancellationToken = default);
}
