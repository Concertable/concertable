using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using Concertable.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Concertable.Infrastructure.Data;

namespace Concertable.Infrastructure.Repositories;

public class MessageRepository : Repository<MessageEntity>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IPagination<MessageEntity>> GetByUserIdAsync(Guid id, IPageParams pageParams)
    {
        var query = context.Messages
            .Include(m => m.FromUser)
            .Where(m => m.ToUserId == id)
            .OrderByDescending(m => m.SentDate);

        return await query.ToPaginationAsync(pageParams);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid id)
    {
        var query = context.Messages
            .Where(m => m.ToUserId == id && !m.Read);

        return await query.CountAsync();
    }

    public async Task MarkAsReadAsync(List<int> ids)
    {
        var messages = await context.Messages
            .Where(m => ids.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
            message.MarkAsRead();

        await context.SaveChangesAsync();
    }
}
