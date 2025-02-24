using Application.Interfaces;
using Core.Entities.Identity;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<ApplicationUser>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<ApplicationUser> GetByApplicationIdAsync(int applicationId)
        {
            var query = context.ListingApplications
                .Where(a => a.Id == applicationId)
                .Select(a => a.Artist.User);

            return await query.FirstAsync();
        }

        public async Task<ApplicationUser> GetByEventIdAsync(int eventId)
        {
            var query = context.Events
                 .Where(e => e.Id == eventId)
                 .Select(e => e.Application.Artist.User);

            return await query.FirstAsync();
        }

        public async Task<int> GetIdByApplicationIdAsync(int applicationId)
        {
            var query = context.ListingApplications
                .Where(a => a.Id == applicationId)
                .Select(a => a.Artist.UserId);

            return await query.FirstAsync();
        }

        public async Task<int> GetIdByEventIdAsync(int eventId)
        {
            var query = context.Events
                .Where(e => e.Id == eventId)
                .Select(e => e.Application.Artist.UserId);

            return await query.FirstAsync();
        }
    }
}
