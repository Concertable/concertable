using Application.Responses;
using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<Pagination<Transaction>> GetAsync(PaginationParams pageParams, int userId);
    }
}
