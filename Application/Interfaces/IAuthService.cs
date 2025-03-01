using Application.DTOs;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        public Task Register(RegisterDto registerDto);
        public Task<ApplicationUser> Login(LoginDto loginDto);
        public Task Logout();
        public Task<ApplicationUser> GetCurrentUserAsync();
        public Task<string> GetFirstUserRoleAsync(ApplicationUser user);
        public Task<string> GetFirstUserRoleAsync();
        Task<bool> ConfirmEmail(string userId, string token);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto requestDto);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto requestDto);
    }
}
