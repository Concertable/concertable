using Application.DTOs;

namespace Application.Responses;

public record LoginResponse(UserDto User, string AccessToken, string RefreshToken, int ExpiresInSeconds, string BaseUrl);
