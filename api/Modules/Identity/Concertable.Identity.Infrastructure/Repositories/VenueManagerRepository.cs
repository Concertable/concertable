using Concertable.Data.Infrastructure;
using Concertable.Identity.Application.Interfaces;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Repositories;

internal class VenueManagerRepository(IdentityDbContext context)
    : GuidRepository<VenueManagerEntity, IdentityDbContext>(context),
      IManagerRepository<VenueManagerEntity>
{
}
