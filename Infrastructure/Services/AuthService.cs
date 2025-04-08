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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;
using static QRCoder.PayloadGenerator;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IStripeAccountService stripeAccountService;
        private readonly IEmailService emailService;
        private readonly IUriService uriService;
        private readonly IPreferenceService preferenceService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AuthService(
            IStripeAccountService stripeAccountService,
            IEmailService emailService,
            IUriService uriService,
            IPreferenceService preferenceService,
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager)
        {
            this.stripeAccountService = stripeAccountService;
            this.emailService = emailService;
            this.uriService = uriService;
            this.preferenceService = preferenceService;
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

            // Create default preferences for every user
            var preferenceDto = new CreatePreferenceDto
            {
                RadiusKm = 10,
                Genres = Enumerable.Empty<GenreDto>()
            };

            await preferenceService.CreateAsync(preferenceDto);

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var uri = uriService.GetEmailConfirmationUri(user.Id, token);

            await emailService.SendEmailAsync(0, user.Email, "Confirm Your Email",
            $"Please confirm your email by clicking <a href='{uri}'>here</a>");
        }

        public async Task Logout()
        {
            await signInManager.SignOutAsync();
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await userManager.FindByEmailAsync(email) != null;
        }

        public async Task<UserDto> Login(LoginDto loginDto)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user is null)
                throw new BadRequestException("Invalid email or password.");

            if (!await userManager.IsEmailConfirmedAsync(user))
                throw new BadRequestException("Please confirm your email before logging in.");

            var result = await signInManager.PasswordSignInAsync(
                loginDto.Email,
                loginDto.Password,
                loginDto.RememberMe,
                true);

            if (!result.Succeeded)
                throw new BadRequestException("Invalid email or password");

            var role = (await userManager.GetRolesAsync(user)).FirstOrDefault();

            if (role is null)
                throw new BadRequestException("User has no role");

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = role
            };

            return userDto;
        }

        public async Task<bool> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadRequestException("Invalid user ID.");

            var result = await userManager.ConfirmEmailAsync(user, token);

            return result.Succeeded;
        }

        public async Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto requestDto)
        {
            var user = await userManager.FindByEmailAsync(requestDto.Email);
            if (user is not null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);

                var resetLink = uriService.GetPasswordResetUri(user.Id, token);

                await emailService.SendEmailAsync(user.Id, user.Email, "Reset your password",
                    $"Please reset your password by clicking <a href='{resetLink}'>here</a>");
            }
            return new ForgotPasswordResponseDto { Message = "If this email is associated with an account, a password reset link has been sent" };
        }

        public async Task<ResetPasswordResponseDto> ResetPasswordAsync(ResetPasswordRequestDto requestDto)
        {
            var user = await userManager.FindByIdAsync(requestDto.UserId);
            if (user == null)
            {
                throw new BadRequestException("Invalid user");
            }

            var result = await userManager.ResetPasswordAsync(user, requestDto.Token, requestDto.NewPassword);

            if (!result.Succeeded)
            {
                throw new BadRequestException("Password reset failed.");
            }

            return new ResetPasswordResponseDto { Message = "Password has been reset successfully." };
        }
    }
}
