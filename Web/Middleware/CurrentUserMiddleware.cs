using Application.Interfaces;
using Infrastructure.Services;

namespace Web.Middleware;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IAccountService accountService)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.User.FindFirst("sub") is { } subClaim &&
            int.TryParse(subClaim.Value, out var userId))
        {
            var dto = await accountService.GetUserByIdAsync(userId, context.RequestAborted);
            if (dto is not null)
            {
                var entity = await accountService.GetUserEntityByIdAsync(userId, context.RequestAborted);
                context.Items[nameof(CurrentUser)] = new CurrentUser(dto, entity);
            }
        }

        await _next(context);
    }
}
