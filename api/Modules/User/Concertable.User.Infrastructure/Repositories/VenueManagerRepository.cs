using Concertable.User.Application.Interfaces;
using Concertable.User.Infrastructure.Data;

namespace Concertable.User.Infrastructure.Repositories;

internal class VenueManagerRepository(UserDbContext context)
    : GuidRepository<VenueManagerEntity>(context),
      IManagerRepository<VenueManagerEntity>
{
}
