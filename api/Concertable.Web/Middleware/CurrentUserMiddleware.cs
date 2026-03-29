using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Concertable.Web.Middleware;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<CurrentUserMiddleware> logger;

    public CurrentUserMiddleware(RequestDelegate next, ILogger<CurrentUserMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserService userService)
    {
        if (context.User.Identity?.IsAuthenticated == true &&
            context.User.FindFirst("sub") is { } subClaim &&
            Guid.TryParse(subClaim.Value, out var userId))
        {
            var dto = await userService.GetUserByIdAsync(userId, context.RequestAborted);
            if (dto is not null)
            {
                var entity = await userService.GetUserEntityByIdAsync(userId, context.RequestAborted);
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
