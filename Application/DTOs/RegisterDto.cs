using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public required string Email { get; set; } 

        public string Password { get; set; }
        public string Role { get; set; }
    }
}
