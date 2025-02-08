using Application.DTOs;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        public Task Register(RegisterDto registerDto);

        public Task Logout();

        public Task<ApplicationUser?> GetCurrentUserAsync();

        public Task<string> GetFirstUserRole(ApplicationUser user);
    }
}
