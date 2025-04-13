using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Constants
{
    public static class RoleRoutes
    {
        public static readonly Dictionary<string, string> BaseUrls = new()
        {
            { "Admin", "/admin" },
            { "User", "/user" },
            { "VenueManager", "/venue" },
            { "ArtistManager", "/artist" }
        };
    }
}
