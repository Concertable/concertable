using Core.Entities;
using Application.Interfaces;
using Infrastructure.Repositories;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Helpers;
using Application.Responses;
using Core.Entities.Identity;
using Application.DTOs;
using System.Linq.Expressions;
using Infrastructure.Factories;

namespace Infrastructure.Repositories
{
    public class VenueRepository : HeaderRepository<Venue, VenueHeaderDto>, IVenueRepository
    {
        public VenueRepository(
            ApplicationDbContext context) : base(context) 
        {
        }

        protected override Expression<Func<Venue, VenueHeaderDto>> Selector => v => new VenueHeaderDto
        {
            Id = v.Id,
            Name = v.Name,
            ImageUrl = v.ImageUrl,
            County = v.User.County,
            Town = v.User.Town,
            Latitude = v.User.Latitude ?? 0,
            Longitude = v.User.Longitude ?? 0
        };

        public async Task<Venue> GetByIdAsync(int id)
        {
            var query = context.Venues
                .Where(v => v.Id == id)
                .Include(v => v.User);

            return await query.FirstAsync();
        } 

        public async Task<Venue?> GetByUserIdAsync(int id)
        {
            var query = context.Venues
                .Where(v => v.UserId == id)
                .Include(v => v.User);

            return await query.FirstOrDefaultAsync();
        }
    }
}
