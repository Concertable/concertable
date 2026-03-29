using System.Security.Claims;

namespace Concertable.Web.Extentions;

public static class ClaimsPrincipalExtensions
{
    public static string GetId(this ClaimsPrincipal user)
    {
        return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }
}
