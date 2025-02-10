using Application.Interfaces;
using Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IUserService userService;

        public ManagerService(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<int> GetIdByEventIdAsync(int id)
        {
            return await userService.GetIdByEventIdAsync(id);
        }

        public async Task<int> GetIdByApplicationIdAsync(int id)
        {
            return await userService.GetIdByApplicationIdAsync(id);
        }
    }
}
