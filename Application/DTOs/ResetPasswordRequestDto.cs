using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ResetPasswordRequestDto
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
}
