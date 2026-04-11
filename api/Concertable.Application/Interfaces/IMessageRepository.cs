using Concertable.Core.Interfaces;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using Concertable.Application.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface IMessageRepository : IRepository<MessageEntity>
{
    Task<Pagination<MessageEntity>> GetByUserIdAsync(Guid id, IPageParams pageParams);
    Task<int> GetUnreadCountByUserIdAsync(Guid id);
    Task MarkAsReadAsync(List<int> ids);
}
