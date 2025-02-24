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
    public interface IMessageRepository : IRepository<Message>
    {
        Task<PaginationResponse<Message>> GetByUserIdAsync(int id, PaginationParams? pageParams);
        Task<int> GetUnreadCountByUserIdAsync(int id);
    }
}
