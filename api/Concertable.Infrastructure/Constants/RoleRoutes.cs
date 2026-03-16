using Core.Enums;

namespace Infrastructure.Constants;

public static class RoleRoutes
{
    public static readonly Dictionary<Role, string> BaseUrls = new()
    {
        { Role.Admin, "/admin" },
        { Role.Customer, "/" },
        { Role.VenueManager, "/venue" },
        { Role.ArtistManager, "/artist" }
    };
}
