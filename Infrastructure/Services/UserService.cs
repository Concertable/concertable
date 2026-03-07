using Application.DTOs;
using Application.Interfaces;
using Application.Mappers;
using Common.Helpers;
using Core.Entities.Identity;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepsitory;
        private readonly ICurrentUserService currentUserService;
        private readonly IGeocodingService geocodingService;

        public UserService(
            IUserRepository userRepsitory,
            ICurrentUserService currentUserService,
            IGeocodingService geocodingService)
        {
            this.userRepsitory = userRepsitory;
            this.currentUserService = currentUserService;
            this.geocodingService = geocodingService;
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

            return user.ToDto();
        }
    }
}
