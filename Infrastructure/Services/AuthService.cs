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
        private readonly IStripeAccountService stripeAccountService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AuthService(
            IStripeAccountService stripeAccountService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager)
        {
            this.stripeAccountService = stripeAccountService;
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

            ApplicationUser user;

            switch(registerDto.Role)
            {
                case "Customer":
                    user = new Customer { UserName = registerDto.Email, Email = registerDto.Email };
                    break;
                case "VenueManager":
                    user = new VenueManager { UserName = registerDto.Email, Email = registerDto.Email };
                    break;
                case "ArtistManager":
                    user = new ArtistManager { UserName = registerDto.Email, Email = registerDto.Email };
                    break;
                default:
                    throw new BadRequestException("Invalid role Specified");
            }

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                throw new BadRequestException("Failed to Register User");

            await userManager.AddToRoleAsync(user, registerDto.Role);

            await stripeAccountService.CreateStripeAccountAsync(user);
        }

        public async Task Logout()
        {
            await signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var principal = httpContextAccessor?.HttpContext?.User;

            var user = await userManager.GetUserAsync(principal);

            if (user == null) throw new UnauthorizedException("User not Authenticated");

            return user;
        }

        public async Task<string> GetFirstUserRoleAsync(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            return roles.First();
        }

        public async Task<string> GetFirstUserRoleAsync()
        {
            var user = await GetCurrentUserAsync();

            return await GetFirstUserRoleAsync(user);
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await userManager.FindByEmailAsync(email) != null;
        }
    }
}
