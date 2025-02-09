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
        Task SendAsync(int fromUserId, int toUserId, string content);
        Task<MessageSummaryDto> GetSummaryForUser(PaginationParams? pageParams);
        Task<PaginationResponse<MessageDto>> GetAllForUserAsync(PaginationParams? pageParams);

    }
}
