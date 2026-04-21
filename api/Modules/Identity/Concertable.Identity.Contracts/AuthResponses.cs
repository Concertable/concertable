namespace Concertable.Identity.Contracts;

public record LoginResponse(IUser User, string AccessToken, string RefreshToken, int ExpiresInSeconds);

public record ForgotPasswordResponse
{
    public required string Message { get; set; }
}

public record ResetPasswordResponse
{
    public required string Message { get; set; }
}
