using Concertable.User.Contracts;
using Microsoft.AspNetCore.Http;

namespace Concertable.Authorization.Infrastructure;

internal class CurrentUserAccessor : ICurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? User =>
        httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? Id =>
        User?.FindFirst("sub") is { } c && Guid.TryParse(c.Value, out var id) ? id : null;

    public Role? Role =>
        User?.FindFirst("role") is { } c && Enum.TryParse<Role>(c.Value, out var role) ? role : null;

    public string? Email => User?.FindFirst("email")?.Value;
}
