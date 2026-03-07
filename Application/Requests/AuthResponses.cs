namespace Application.Requests
{
    public record ForgotPasswordResponse
    {
        public string Message { get; set; }
    }

    public record ResetPasswordResponse
    {
        public string Message { get; set; }
    }
}
