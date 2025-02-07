using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBaseEntityRepository<T> : IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        bool Exists(int id);
    }
}
