using System.ComponentModel.DataAnnotations;

namespace Application.Requests
{
    public record LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public record RegisterRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    public record ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public record ResetPasswordRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The confirmation password does not match.")]
        public string ConfirmPassword { get; set; }
    }

    public record ChangeEmailRequest
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}
