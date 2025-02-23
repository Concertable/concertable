using Application.Interfaces;
using Core.Entities;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;
        private readonly Dictionary<Type, object> repositories;

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            this.repositories = new Dictionary<Type, object>();
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
        {
            if (repositories.ContainsKey(typeof(TEntity)))
            {
                return (IRepository<TEntity>)repositories[typeof(TEntity)];
            }

            var repository = new Repository<TEntity>(context);
            repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        public IBaseRepository<TEntity> GetBaseRepository<TEntity>() where TEntity : class
        {
            if (repositories.ContainsKey(typeof(TEntity)))
            {
                return (IBaseRepository<TEntity>)repositories[typeof(TEntity)];
            }

            var repository = new BaseRepository<TEntity>(context);
            repositories.Add(typeof(TEntity), repository);
            return repository;
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await context.Database.BeginTransactionAsync();
        }
    }

}
