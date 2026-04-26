using Concertable.Identity.Application.Interfaces;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Repositories;

internal class ArtistManagerRepository(IdentityDbContext context)
    : GuidRepository<ArtistManagerEntity>(context),
      IManagerRepository<ArtistManagerEntity>
{
}
