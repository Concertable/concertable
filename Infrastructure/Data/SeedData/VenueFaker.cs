using Bogus;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.SeedData
{
    public static class VenueFaker
    {
        public static Faker<Venue> GetFaker(int userId, string name, string imageUrl)
        {
            return new Faker<Venue>()
                .RuleFor(v => v.UserId, userId)
                .RuleFor(v => v.Name, name)
                .RuleFor(v => v.ImageUrl, imageUrl)
                .RuleFor(v => v.About, f => f.Lorem.Paragraph(7))
                .RuleFor(v => v.Approved, true);
        }
    }
}
