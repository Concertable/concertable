using Concertable.Data.Infrastructure;
using Concertable.Identity.Application.Interfaces;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Repositories;

internal class ArtistManagerRepository(IdentityDbContext context)
    : GuidModuleRepository<ArtistManagerEntity, IdentityDbContext>(context),
      IManagerRepository<ArtistManagerEntity>
{
}
