using Bogus;
using Core.Entities.Identity;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.SeedData
{
    public static class ArtistManagerFaker
    {
        public static Faker<ArtistManager> GetFaker(int userId, Location location, string? stripeId = null)
        {
            return new Faker<ArtistManager>()
                .RuleFor(am => am.UserName, $"artistmanager{userId}@test.com")
                .RuleFor(am => am.Email, $"artistmanager{userId}@test.com")
                .RuleFor(am => am.EmailConfirmed, true)
                .RuleFor(am => am.County, location.County)
                .RuleFor(am => am.Town, location.Town)
                .RuleFor(am => am.Location, new Point(location.Longitude, location.Latitude) { SRID = 4326 })
                .RuleFor(vm => vm.StripeId, stripeId);
        }
    }
}
