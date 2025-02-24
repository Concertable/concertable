using Application.DTOs;
using Application.Interfaces;
using Core.Entities.Identity;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepsitory;

        public UserService(IUserRepository userRepsitory)
        {
            this.userRepsitory = userRepsitory;
        }

        public async Task<int> GetIdByApplicationIdAsync(int applicationId)
        {
            return await userRepsitory.GetIdByApplicationIdAsync(applicationId);
        }

        public async Task<ApplicationUser> GetByApplicationIdAsync(int applicationId)
        {
            return await userRepsitory.GetByApplicationIdAsync(applicationId);
        }

        public async Task<int> GetIdByEventIdAsync(int eventId)
        {
            return await userRepsitory.GetIdByEventIdAsync(eventId);
        }

        public async Task<ApplicationUser> GetByEventIdAsync(int eventId)
        {
            return await userRepsitory.GetByEventIdAsync(eventId);
        }
    }
}
