using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Core.Entities.Identity;
using Core.Exceptions;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly UserManager<ApplicationUser> userManager;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.userManager = userManager;
    }

    public async Task<UserDto> GetAsync()
    {
        var user = await GetEntityAsync();
        var role = await GetFirstRoleAsync(user);

        var userDto = user.ToDto();
        userDto.Role = role;
        userDto.BaseUrl = RoleRoutes.BaseUrls[role];

        return userDto;
    }

    public async Task<UserDto?> GetOrDefaultAsync()
    {
        try
        {
            var user = await GetEntityAsync();
            var role = await GetFirstRoleAsync(user);

            var userDto = user.ToDto();
            userDto.Role = role;
            userDto.BaseUrl = RoleRoutes.BaseUrls[role];

            return userDto;
        }
        catch (UnauthorizedException)
        {
            return null;
        }
    }

    public async Task<int> GetIdAsync()
    {
        return (await GetAsync()).Id;
    }

    public async Task<string> GetFirstRoleAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedException("User not authenticated");
        var user = await userManager.GetUserAsync(principal)
            ?? throw new UnauthorizedException("User not authenticated");
        var roles = await userManager.GetRolesAsync(user);

        if (!roles.Any())
            throw new BadRequestException("User has no roles assigned.");

        return roles.First();
    }

    public async Task<string> GetFirstRoleAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        if (!roles.Any())
            throw new BadRequestException("User has no roles assigned.");

        return roles.First();
    }

    public async Task<ApplicationUser> GetEntityAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedException("User not authenticated");
        var user = await userManager.GetUserAsync(principal);
        if (user is null) throw new UnauthorizedException("User not authenticated");

        return user;
    }
}
