using Application.Interfaces;
using Application.Responses;
using Core.Entities;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context) : base(context) { }

        public Task<PaginationResponse<Transaction>> GetAsync(PaginationParams pageParams, int userId)
        {
            var query = context.Transactions
                .Where(t => t.FromUserId == userId || t.ToUserId == userId)
                .OrderByDescending(t => t.CreatedAt);

            return PaginationHelper.CreatePaginatedResponseAsync(query, pageParams);
        }
    }
}
