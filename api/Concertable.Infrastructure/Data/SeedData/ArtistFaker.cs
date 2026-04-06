using Bogus;
using Concertable.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Infrastructure.Data.SeedData;

public static class ArtistFaker
{
    public static Faker<ArtistEntity> GetFaker(Guid userId, string name, string bannerUrl)
    {
        return new Faker<ArtistEntity>()
            .RuleFor(a => a.UserId, userId)
            .RuleFor(a => a.Name, name)
            .RuleFor(a => a.BannerUrl, bannerUrl)
            .RuleFor(a => a.About, f => f.Lorem.Paragraph(7));
    }
}
