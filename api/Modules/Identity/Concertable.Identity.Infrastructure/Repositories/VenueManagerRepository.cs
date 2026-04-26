using Concertable.Identity.Application.Interfaces;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Repositories;

internal class VenueManagerRepository(IdentityDbContext context)
    : GuidRepository<VenueManagerEntity>(context),
      IManagerRepository<VenueManagerEntity>
{
}
