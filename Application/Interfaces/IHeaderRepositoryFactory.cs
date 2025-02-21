using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IHeaderRepositoryFactory
    {
        IHeaderRepository<TDto> Create<TEntity, TDto>(Expression<Func<TEntity, TDto>> selector)
        where TEntity : class
        where TDto : HeaderDto;

        IHeaderRepository<TDto> Create<TEntity, TDto>(Expression<Func<TEntity, TDto>> selector, List<Expression<Func<TEntity, bool>>> filters)
        where TEntity : class
        where TDto : HeaderDto;
    }
}
