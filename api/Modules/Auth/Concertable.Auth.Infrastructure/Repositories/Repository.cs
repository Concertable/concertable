using Concertable.Auth.Infrastructure.Data;
using Concertable.Data.Infrastructure;

namespace Concertable.Auth.Infrastructure.Repositories;

internal abstract class BaseRepository<TEntity>(AuthDbContext context)
    : BaseRepository<TEntity, AuthDbContext>(context)
    where TEntity : class;

internal abstract class Repository<TEntity>(AuthDbContext context)
    : Repository<TEntity, AuthDbContext>(context)
    where TEntity : class, IIdEntity;
