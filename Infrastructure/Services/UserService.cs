using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Common.Helpers;
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
        private readonly ICurrentUserService currentUserService;
        private readonly IGeocodingService geocodingService;
        private readonly IMapper mapper;

        public UserService(
            IUserRepository userRepsitory, 
            ICurrentUserService currentUserService,
            IGeocodingService geocodingService,
            IMapper mapper)
        {
            this.userRepsitory = userRepsitory;
            this.currentUserService = currentUserService;
            this.geocodingService = geocodingService;
            this.mapper = mapper;
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

        public async Task<UserDto> UpdateLocationAsync(double latitude, double longitude)
        {
            var user = await currentUserService.GetEntityAsync();

            user.Location = LocationHelper.CreatePoint(latitude, longitude);

            var locationDto = await geocodingService.GetLocationAsync(latitude, longitude);

            user.County = locationDto.County;
            user.Town = locationDto.Town;

            userRepsitory.Update(user);
            await userRepsitory.SaveChangesAsync();

            return mapper.Map<UserDto>(user);
        }

    }
}
