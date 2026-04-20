namespace Concertable.Identity.Contracts;

public record LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public bool RememberMe { get; set; }
}

public record RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required Role Role { get; set; }
}

public record VerifyEmailRequest
{
    public required string Token { get; set; }
}

public record ForgotPasswordRequest
{
    public required string Email { get; set; }
}

public record ResetPasswordRequest
{
    public required string Token { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

public record ChangePasswordRequest
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}

public record ChangeEmailRequest
{
    public required string NewEmail { get; set; }
}

public record RefreshTokenRequest
{
    public required string RefreshToken { get; set; }
}
