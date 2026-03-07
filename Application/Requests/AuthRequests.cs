namespace Application.Requests
{
    public record LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public record RegisterRequest
    {
        public required string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public record ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public record ResetPasswordRequest
    {
        public int UserId { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public record ChangeEmailRequest
    {
        public string NewEmail { get; set; }
    }
}
