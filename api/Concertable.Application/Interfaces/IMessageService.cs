using Concertable.Core.Interfaces;
using Concertable.Application.DTOs;
using Concertable.Core.Entities;
using Concertable.Core.Parameters;
using Concertable.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface IMessageService
{
    Task SendAsync(Guid fromUserId, Guid toUserId, string action, int actionId, string content);
    Task SendAndSaveAsync(Guid fromUserId, Guid toUserId, string action, int actionId, string content);
    Task<MessageSummaryDto> GetSummaryForUser();
    Task<Pagination<MessageDto>> GetForUserAsync(IPageParams pageParams);
    Task<int> GetUnreadCountForUserAsync();
    Task MarkAsReadAsync(List<int> ids);
}
