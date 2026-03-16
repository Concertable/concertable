using Application.Interfaces;
using Application.Responses;
using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Payment;

public interface ITransactionRepository : IRepository<TransactionEntity>
{
    Task<Pagination<TransactionEntity>> GetAsync(IPageParams pageParams, int userId);
}
