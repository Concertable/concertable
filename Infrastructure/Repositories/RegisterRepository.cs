using Core.Entities;
using Application.Interfaces;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RegisterRepository : Repository<Register>, IRegisterRepository
    {
        public RegisterRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Register>> GetAllForListingIdAsync(int listingId)
        {
            var query = context.Registers.AsQueryable();
            query = query.Where(v => v.ListingId == listingId);

            return await query.ToListAsync();
        }
    }
}
