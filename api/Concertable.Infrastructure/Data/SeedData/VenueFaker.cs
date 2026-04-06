using Bogus;
using Concertable.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Infrastructure.Data.SeedData;

public static class VenueFaker
{
    public static Faker<VenueEntity> GetFaker(Guid userId, string name, string bannerUrl)
    {
        return new Faker<VenueEntity>()
            .RuleFor(v => v.UserId, userId)
            .RuleFor(v => v.Name, name)
            .RuleFor(v => v.BannerUrl, bannerUrl)
            .RuleFor(v => v.About, f => f.Lorem.Paragraph(7))
            .RuleFor(v => v.Approved, true);
    }
}
