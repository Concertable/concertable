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
using Infrastructure.Constants;
using AutoMapper;
using Application.Responses;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IStripeAccountService stripeAccountService;
        private readonly IEmailService emailService;
        private readonly IUriService uriService;
        private readonly IPreferenceService preferenceService;
        private readonly ICurrentUserService currentUserService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IMapper mapper;

        public AuthService(
            IStripeAccountService stripeAccountService,
            IEmailService emailService,
            IUriService uriService,
            IPreferenceService preferenceService,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            IMapper mapper)
        {
            this.stripeAccountService = stripeAccountService;
            this.emailService = emailService;
            this.uriService = uriService;
            this.preferenceService = preferenceService;
            this.currentUserService = currentUserService;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.mapper = mapper;
        }

        public async Task Register(RegisterDto registerDto)
        {
            var reasons = new List<string>();

            // Perform checks and add reasons if validation fails
            if (await CheckEmailExistsAsync(registerDto.Email))
                reasons.Add("Email already exists");
            if (registerDto.Role.Equals("Admin"))
                reasons.Add("You cannot make yourself an admin");

            // Determine user type based on role
            ApplicationUser? user = registerDto.Role switch
            {
                "Customer" => new Customer { UserName = registerDto.Email, Email = registerDto.Email },
                "VenueManager" => new VenueManager { UserName = registerDto.Email, Email = registerDto.Email },
                "ArtistManager" => new ArtistManager { UserName = registerDto.Email, Email = registerDto.Email },
                _ => null
            };

            if (user is null)
                reasons.Add("Invalid role specified");

            if (reasons.Any())
                throw new BadRequestException(reasons);
            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                reasons.AddRange(result.Errors.Select(e => e.Description));
                throw new BadRequestException(reasons);
            }

            await userManager.AddToRoleAsync(user, registerDto.Role);

            await stripeAccountService.CreateStripeAccountAsync(user);

            var preferenceDto = new CreatePreferenceDto
            {
                RadiusKm = 10,
                Genres = Enumerable.Empty<GenreDto>()
            };
            await preferenceService.CreateAsync(preferenceDto, user.Id);

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

            var userDto = mapper.Map<UserDto>(user);
            userDto.Role = role;
            userDto.BaseUrl = RoleRoutes.BaseUrls[role];

            return userDto;
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
                throw new NotFoundException("User not found");

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
            var user = await userManager.FindByIdAsync(requestDto.UserId.ToString());

            var result = await userManager.ResetPasswordAsync(user, requestDto.Token, requestDto.NewPassword);

            if (!result.Succeeded)
                throw new BadRequestException("Password reset failed.");

            return new ResetPasswordResponseDto { Message = "Password has been reset successfully." };
        }

        public async Task RequestEmailChangeAsync(string newEmail)
        {
            var user = await currentUserService.GetEntityAsync();

            if (await userManager.FindByEmailAsync(newEmail) is not null)
                throw new BadRequestException("Email already in use");

            var token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var uri = uriService.GetEmailChangeConfirmationUri(user.Id, token, newEmail);

            await emailService.SendEmailAsync(user.Id, user.Email, "Confirm your new email",
                $"Please confirm your email change by clicking <a href='{uri}'>here</a>");
        }


        public async Task<bool> ConfirmEmailChangeAsync(string token, string newEmail)
        {
            var user = await currentUserService.GetEntityAsync();

            var result = await userManager.ChangeEmailAsync(user, newEmail, token);
            if (!result.Succeeded)
                throw new BadRequestException("Failed to change email");

            await userManager.SetUserNameAsync(user, newEmail);
            return true;
        }


    }
}
