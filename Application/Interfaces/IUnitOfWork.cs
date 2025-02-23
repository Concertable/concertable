using Core.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;
        IBaseRepository<TEntity> GetBaseRepository<TEntity>() where TEntity : class;
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }

}
