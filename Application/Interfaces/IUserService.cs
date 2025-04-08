using Application.DTOs;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser> GetByApplicationIdAsync(int applicationId);
        Task<ApplicationUser> GetByEventIdAsync(int id);
        Task<int> GetIdByApplicationIdAsync(int id);
        Task<int> GetIdByEventIdAsync(int id);
        Task<UserDto> UpdateLocationAsync(double latitude, double longitude);
    }
}
