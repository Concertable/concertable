using Application.DTOs;
using Application.Requests;
using Application.Responses;
using Core.Entities;

namespace Application.Interfaces;

public interface IAuthService
{
    Task Register(RegisterRequest request);
    Task<LoginResponse> Login(LoginRequest request);
    Task Logout();

    /// <summary>Gets user DTO by id (e.g. for current-request resolution). Returns null if not found.</summary>
    Task<UserDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Gets user entity by id (e.g. for services that need StripeId, etc). Returns null if not found.</summary>
    Task<User?> GetUserEntityByIdAsync(int userId, CancellationToken cancellationToken = default);
}
