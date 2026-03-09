using Application.DTOs;

namespace Application.Responses;

public record LoginResponse(UserDto User, string AccessToken, int ExpiresInSeconds);
