using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data.Identity;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Factories
{
    public class HeaderRepositoryFactory : IHeaderRepositoryFactory
    {
        private readonly ApplicationDbContext context;

        public HeaderRepositoryFactory(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IHeaderRepository<TDto> Create<TEntity, TDto>(Expression<Func<TEntity, TDto>> selector, List<Expression<Func<TEntity, bool>>> filters)
            where TEntity : class
            where TDto : HeaderDto
        {
            return new HeaderRepository<TEntity, TDto>(context, selector, filters);
        }

        public IHeaderRepository<TDto> Create<TEntity, TDto>(Expression<Func<TEntity, TDto>> selector)
            where TEntity : class
            where TDto : HeaderDto
        {
            return new HeaderRepository<TEntity, TDto>(context, selector);
        }
    }
}
