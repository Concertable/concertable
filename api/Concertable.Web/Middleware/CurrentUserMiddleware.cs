using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Web.Middleware;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<CurrentUserMiddleware> logger;

    public CurrentUserMiddleware(RequestDelegate next, ILogger<CurrentUserMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

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
            else
                logger.LogWarning(
                    "Authenticated user with id {UserId} from claim 'sub' was not found in the database. Path: {Path}, TraceId: {TraceId}",
                    userId,
                    context.Request.Path,
                    context.TraceIdentifier
                );
        }

        await next(context);
    }
}
