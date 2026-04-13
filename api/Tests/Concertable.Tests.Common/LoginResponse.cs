namespace Concertable.Tests.Common;

public record LoginResponse(string AccessToken, string RefreshToken, int ExpiresInSeconds);
