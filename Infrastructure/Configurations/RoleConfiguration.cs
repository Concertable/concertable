using Core.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.HasData(
            new ApplicationRole()
            {
                Id = 1,
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new ApplicationRole()
            {
                Id = 2,
                Name = "Customer",
                NormalizedName = "CUSTOMER"
            },
            new ApplicationRole()
            {
                Id = 3,
                Name = "ArtistManager",
                NormalizedName = "ARTISTMANAGER"
            },
            new ApplicationRole()
            {
                Id = 4,
                Name = "VenueManager",
                NormalizedName = "VENUEMANAGER"
            });
    }
}
