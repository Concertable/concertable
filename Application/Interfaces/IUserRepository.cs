using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository : IBaseRepository<ApplicationUser>
    {
        Task<int> GetIdByApplicationIdAsync(int applicationId);
        Task<int> GetIdByEventIdAsync(int eventId);
    }
}
