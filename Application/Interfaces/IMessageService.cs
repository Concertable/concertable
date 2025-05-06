using Application.DTOs;
using Core.Entities;
using Core.Parameters;
using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMessageService
    {
        Task SendAsync(int fromUserId, int toUserId, string action, int actionId, string content);
        Task SendAndSaveAsync(int fromUserId, int toUserId, string action, int actionId, string content);
        Task<MessageSummaryDto> GetSummaryForUser();
        Task<PaginationResponse<MessageDto>> GetForUserAsync(PaginationParams? pageParams);
        Task<int> GetUnreadCountForUserAsync();
        Task MarkAsReadAsync(List<int> ids);
    }
}
