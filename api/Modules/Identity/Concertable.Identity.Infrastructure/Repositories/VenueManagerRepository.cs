using Concertable.Data.Infrastructure;
using Concertable.Identity.Application.Interfaces;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Repositories;

internal class VenueManagerRepository(IdentityDbContext context)
    : GuidModuleRepository<VenueManagerEntity, IdentityDbContext>(context),
      IManagerRepository<VenueManagerEntity>
{
}
