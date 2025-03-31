using Application.Interfaces;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using Infrastructure.Data.Identity;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infrastructure.Repositories
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PaginationResponse<Message>> GetByUserIdAsync(int id, PaginationParams pageParams)
        {
            var query = context.Messages
                .Include(m => m.FromUser)
                .Where(m => m.ToUserId == id)
                .OrderByDescending(m => m.SentDate);

            return await PaginationHelper.CreatePaginatedResponseAsync(query, pageParams);
        }

        public async Task<int> GetUnreadCountByUserIdAsync(int id)
        {
            var query = context.Messages
                .Where(m => m.ToUserId == id && !m.Read);

            return await query.CountAsync();
        }
    }
}
