namespace Application.Requests;

public record ForgotPasswordResponse
{
    public required string Message { get; set; }
}

public record ResetPasswordResponse
{
    public required string Message { get; set; }
}
