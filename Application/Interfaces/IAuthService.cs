using Application.DTOs;
using Application.Responses;
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
        public Task<UserDto> Login(LoginDto loginDto);
        public Task Logout();
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto requestDto);
        Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto requestDto);
        Task RequestEmailChangeAsync(string newEmail);
        Task<bool> ConfirmEmailChangeAsync(string token, string newEmail);
    }
}
