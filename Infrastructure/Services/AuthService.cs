using Infrastructure.Repositories;
using Core.Exceptions;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Application.DTOs;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AuthService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task Register(RegisterDto registerDto)
        {
            if (CheckEmailExistsAsync(registerDto.Email).Result)
                throw new BadRequestException("Email already exists");
            if (registerDto.Role.Equals("Admin"))
                throw new UnauthorizedException("You cannot make yourself an admin");

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email
            };

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                throw new BadRequestException("Failed to Register User");

            await userManager.AddToRoleAsync(user, registerDto.Role);
        }

        public async Task Logout()
        {
            await signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var principal = httpContextAccessor.HttpContext.User;
            return await userManager.GetUserAsync(principal);
        }

        public async Task<string> GetFirstUserRole(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            return roles.First();
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await userManager.FindByEmailAsync(email) != null;
        }
    }
}
