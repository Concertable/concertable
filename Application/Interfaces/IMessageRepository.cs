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
    public interface IMessageRepository : IRepository<Message>
    {
        Task<PaginationResponse<Message>> GetAllForUserAsync(int id, PaginationParams? pageParams);
        Task<int> GetUnreadCountForUserAsync(int id);
    }
}
