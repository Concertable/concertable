using Application.Interfaces;
using Infrastructure.Services;

namespace Web.Middleware;

/// <summary>
/// Sets ICurrentAppUser from the User table when request has a JWT (Bearer). Loads user by sub; does not touch Identity.
/// </summary>
public class CurrentAppUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentAppUserMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IAppAuthService appAuthService)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.User.FindFirst("sub") is { } subClaim &&
            int.TryParse(subClaim.Value, out var userId))
        {
            var user = await appAuthService.GetUserByIdAsync(userId, context.RequestAborted);
            if (user is not null)
            {
                var currentAppUser = context.RequestServices.GetRequiredService<CurrentAppUser>();
                currentAppUser.Set(user);
            }
        }

        await _next(context);
    }
}
