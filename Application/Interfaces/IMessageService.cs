using Application.DTOs;
using Core.Entities;
using Core.Parameters;
using Core.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMessageService
    {
        Task SendAsync(int fromUserId, int toUserId, string action, string content);
        Task<MessageSummaryDto> GetSummaryForUser();
        Task<PaginationResponse<MessageDto>> GetForUserAsync(PaginationParams? pageParams);

    }
}
