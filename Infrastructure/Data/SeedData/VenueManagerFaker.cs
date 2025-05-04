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
    public static class VenueManagerFaker
    {
        public static Faker<VenueManager> GetFaker(int userId, Location location, string? stripeId = null)
        {
            return new Faker<VenueManager>()
                .RuleFor(vm => vm.UserName, $"venuemanager{userId}@test.com")
                .RuleFor(vm => vm.Email, $"venuemanager{userId}@test.com")
                .RuleFor(vm => vm.EmailConfirmed, true)
                .RuleFor(vm => vm.County, location.County)
                .RuleFor(vm => vm.Town, location.Town)
                .RuleFor(vm => vm.Location, new Point(location.Longitude, location.Latitude) { SRID = 4326 })
                .RuleFor(vm => vm.StripeId, stripeId);
        }
    }
}
