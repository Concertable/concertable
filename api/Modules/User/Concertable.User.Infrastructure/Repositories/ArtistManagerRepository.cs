using Concertable.User.Application.Interfaces;
using Concertable.User.Infrastructure.Data;

namespace Concertable.User.Infrastructure.Repositories;

internal class ArtistManagerRepository(UserDbContext context)
    : GuidRepository<ArtistManagerEntity>(context),
      IManagerRepository<ArtistManagerEntity>
{
}
