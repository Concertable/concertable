using Core.Interfaces;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IMessageRepository : IRepository<MessageEntity>
{
    Task<Pagination<MessageEntity>> GetByUserIdAsync(int id, IPageParams pageParams);
    Task<int> GetUnreadCountByUserIdAsync(int id);
    Task MarkAsReadAsync(List<int> ids);
}
