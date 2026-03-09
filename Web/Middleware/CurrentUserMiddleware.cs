using Application.DTOs;
using Application.Mappers;
using Core.Entities.Identity;
using Infrastructure.Constants;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;

namespace Web.Middleware;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var identityUser = await userManager.GetUserAsync(context.User);
            if (identityUser is not null)
            {
                var roles = await userManager.GetRolesAsync(identityUser);
                var role = roles.FirstOrDefault() ?? string.Empty;

                var userDto = identityUser.ToDto();
                userDto.Role = role;
                userDto.BaseUrl = RoleRoutes.BaseUrls.TryGetValue(role, out var baseUrl) ? baseUrl : "/";

                var currentUser = context.RequestServices.GetRequiredService<CurrentUser>();
                currentUser.Set(userDto, identityUser);
            }
        }

        await _next(context);
    }
}
