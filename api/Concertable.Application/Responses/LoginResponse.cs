using Concertable.Application.Interfaces;

namespace Concertable.Application.Responses;

public record LoginResponse(IUser User, string AccessToken, string RefreshToken, int ExpiresInSeconds);
