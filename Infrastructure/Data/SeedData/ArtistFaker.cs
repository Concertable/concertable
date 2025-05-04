using Bogus;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.SeedData
{
    public static class ArtistFaker
    {
        public static Faker<Artist> GetFaker(int userId, string name, string imageUrl)
        {
            return new Faker<Artist>()
                .RuleFor(a => a.UserId, userId)
                .RuleFor(a => a.Name, name)
                .RuleFor(a => a.ImageUrl, imageUrl)
                .RuleFor(a => a.About, f => f.Lorem.Paragraph());
        }
    }
}
