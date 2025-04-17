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
using NetTopologySuite.Geometries;

namespace Infrastructure.Repositories
{
    public class VenueRepository : HeaderRepository<Venue, VenueHeaderDto>, IVenueRepository
    {
        public VenueRepository(
            ApplicationDbContext context,
            IGeometryService geometryService
            ) : base(context, geometryService) 
        {
        }

        protected override Expression<Func<Venue, VenueHeaderDto>> Selector => v => new VenueHeaderDto
        {
            Id = v.Id,
            Name = v.Name,
            ImageUrl = v.ImageUrl,
            County = v.User.County,
            Town = v.User.Town,
            Latitude = v.User.Location.Y,
            Longitude = v.User.Location.X
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

        protected override IQueryable<Venue> FilterByRadius(IQueryable<Venue> query, double latitude, double longitude, double radiusKm)
        {
            var center = geometryService.CreatePoint(latitude, longitude);

            query = query.Where(e =>
                e.User.Location != null &&
                e.User.Location.Distance(center) <= (radiusKm * 1000));

            return query;
        }
    }
}
