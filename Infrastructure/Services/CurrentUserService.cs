using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entities.Identity;
using Core.Exceptions;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;

        public static readonly Dictionary<string, string> BaseUrls = new()
        {
            { "Admin", "/admin" },
            { "Customer", "/user" },
            { "VenueManager", "/venue" },
            { "ArtistManager", "/artist" },
        };

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor, 
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.mapper = mapper;
        }

        public async Task<UserDto> GetAsync()
        {
            var user = await GetEntityAsync();
            var role = await GetFirstRoleAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = role,
                County = user.County,
                Town = user.Town,
                Latitude = user.Latitude,
                Longitude = user.Longitude,
                BaseUrl = RoleRoutes.BaseUrls[role]
            };
        }

        public async Task<UserDto?> GetOrDefaultAsync()
        {
            try
            {
                var user = await GetEntityAsync();
                var role = await GetFirstRoleAsync(user);

                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = role,
                    County = user.County,
                    Town = user.Town,
                    Latitude = user.Latitude,
                    Longitude = user.Longitude,
                    BaseUrl = RoleRoutes.BaseUrls[role]
                };
            }
            catch (UnauthorizedException)
            {
                return null;
            }
        }

        public async Task<int> GetIdAsync()
        {
            return (await GetAsync()).Id;
        }

        public async Task<string> GetFirstRoleAsync()
        {
            var principal = httpContextAccessor.HttpContext?.User;
            var user = await userManager.GetUserAsync(principal);
            var roles = await userManager.GetRolesAsync(user);

            if (!roles.Any())
                throw new BadRequestException("User has no roles assigned.");

            return roles.First();
        }

        public async Task<string> GetFirstRoleAsync(ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (!roles.Any())
                throw new BadRequestException("User has no roles assigned.");

            return roles.First();
        }

        public async Task<ApplicationUser> GetEntityAsync()
        {
            var principal = httpContextAccessor.HttpContext?.User;
            var user = await userManager.GetUserAsync(principal);
            if (user is null) throw new UnauthorizedException("User not authenticated");

            return user;
        }
    }
}
