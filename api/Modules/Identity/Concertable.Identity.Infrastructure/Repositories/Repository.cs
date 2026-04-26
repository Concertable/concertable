using Concertable.Data.Infrastructure;
using Concertable.Identity.Infrastructure.Data;

namespace Concertable.Identity.Infrastructure.Repositories;

internal abstract class BaseRepository<TEntity>(IdentityDbContext context)
    : BaseRepository<TEntity, IdentityDbContext>(context)
    where TEntity : class;

internal abstract class Repository<TEntity>(IdentityDbContext context)
    : Repository<TEntity, IdentityDbContext>(context)
    where TEntity : class, IIdEntity;

internal abstract class GuidRepository<TEntity>(IdentityDbContext context)
    : GuidRepository<TEntity, IdentityDbContext>(context)
    where TEntity : class, IGuidEntity;
