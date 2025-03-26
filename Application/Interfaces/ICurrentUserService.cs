using Application.DTOs;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICurrentUserService
    {
        Task<UserDto> GetAsync();
        Task<ApplicationUser> GetEntityAsync();
        Task<int> GetIdAsync();
        Task<string> GetFirstRoleAsync();
    }
}
