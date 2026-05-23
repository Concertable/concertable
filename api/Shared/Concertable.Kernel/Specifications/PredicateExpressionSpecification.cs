using System.Linq.Expressions;
using Concertable.Kernel.Expressions;

namespace Concertable.Kernel.Specifications;

public abstract class PredicateExpressionSpecification<TEntity>
    : PredicateSpecification<TEntity>, IExpressionSpecification<TEntity>
    where TEntity : class
{
    public IQueryable<TNav> ApplyVia<TNav>(
        IQueryable<TNav> query,
        Expression<Func<TNav, TEntity>> navigation)
        => query.Where(navigation.Substitute(Predicate));
}

public abstract class PredicateExpressionSpecification<TEntity, TParams>
    : PredicateSpecification<TEntity, TParams>, IExpressionSpecification<TEntity, TParams>
    where TEntity : class
{
    public IQueryable<TNav> ApplyVia<TNav>(
        IQueryable<TNav> query,
        Expression<Func<TNav, TEntity>> navigation,
        TParams @params)
        => query.Where(navigation.Substitute(BuildPredicate(@params)));
}
