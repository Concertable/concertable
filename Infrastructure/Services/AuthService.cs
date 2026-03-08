using Infrastructure.Repositories;
using Core.Exceptions;
using Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Application.Interfaces;
using Core.Entities.Identity;
using Application.DTOs;
using Application.Mappers;
using Application.Requests;
using Application.Responses;
using Infrastructure.Constants;
using static QRCoder.PayloadGenerator;

namespace Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IStripeAccountService stripeAccountService;
    private readonly IEmailService emailService;
    private readonly IUriService uriService;
    private readonly IPreferenceService preferenceService;
    private readonly ICurrentUserService currentUserService;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;

    public AuthService(
        IStripeAccountService stripeAccountService,
        IEmailService emailService,
        IUriService uriService,
        IPreferenceService preferenceService,
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        this.stripeAccountService = stripeAccountService;
        this.emailService = emailService;
        this.uriService = uriService;
        this.preferenceService = preferenceService;
        this.currentUserService = currentUserService;
        this.userManager = userManager;
        this.signInManager = signInManager;
    }

    public async Task Register(RegisterRequest request)
    {
        var reasons = new List<string>();

        if (await CheckEmailExistsAsync(request.Email))
            reasons.Add("Email already exists");
        if (request.Role.Equals("Admin"))
            reasons.Add("You cannot make yourself an admin");

        ApplicationUser? user = request.Role switch
        {
            "Customer" => new Customer { UserName = request.Email, Email = request.Email },
            "VenueManager" => new VenueManager { UserName = request.Email, Email = request.Email },
            "ArtistManager" => new ArtistManager { UserName = request.Email, Email = request.Email },
            _ => null
        };

        if (user is null)
            reasons.Add("Invalid role specified");

        if (reasons.Any())
            throw new BadRequestException(reasons);

        var result = await userManager.CreateAsync(user!, request.Password);

        if (!result.Succeeded)
        {
            reasons.AddRange(result.Errors.Select(e => e.Description));
            throw new BadRequestException(reasons);
        }

        await userManager.AddToRoleAsync(user!, request.Role);
        await stripeAccountService.CreateStripeAccountAsync(user!);

        var createPreferenceRequest = new CreatePreferenceRequest
        {
            RadiusKm = 10,
            Genres = Enumerable.Empty<GenreDto>()
        };
        await preferenceService.CreateAsync(createPreferenceRequest, user!.Id);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user!);
        var uri = uriService.GetEmailConfirmationUri(user!.Id, token);

        await emailService.SendEmailAsync(user!.Email!, "Confirm Your Email",
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

    public async Task<UserDto> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            throw new BadRequestException("Invalid email or password.");

        if (!await userManager.IsEmailConfirmedAsync(user))
            throw new BadRequestException("Please confirm your email before logging in.");

        var result = await signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            request.RememberMe,
            true);

        if (!result.Succeeded)
            throw new BadRequestException("Invalid email or password");

        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault();

        if (role is null)
            throw new BadRequestException("User has no role");

        var userDto = user.ToDto();
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

    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is not null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = uriService.GetPasswordResetUri(user.Id, token);

            await emailService.SendEmailAsync(user.Email!, "Reset your password",
                $"Please reset your password by clicking <a href='{resetLink}'>here</a>");
        }
        return new ForgotPasswordResponse { Message = "If this email is associated with an account, a password reset link has been sent" };
    }

    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new NotFoundException("User not found");
        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
            throw new BadRequestException("Password reset failed.");

        return new ResetPasswordResponse { Message = "Password has been reset successfully." };
    }

    public async Task RequestEmailChangeAsync(string newEmail)
    {
        var user = await currentUserService.GetEntityAsync();

        if (await userManager.FindByEmailAsync(newEmail) is not null)
            throw new BadRequestException("Email already in use");

        var token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var uri = uriService.GetEmailChangeConfirmationUri(user.Id, token, newEmail);

        await emailService.SendEmailAsync(user.Email!, "Confirm your new email",
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
