using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Responses;
using Infrastructure.Helpers;
using Application.DTOs;
using Infrastructure.Factories;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class ArtistRepository : HeaderRepository<Artist, ArtistHeaderDto>, IArtistRepository
    {

        public ArtistRepository(ApplicationDbContext context) : base(context) 
        {
        }

        protected override Expression<Func<Artist, ArtistHeaderDto>> Selector => a => new ArtistHeaderDto
        {
            Id = a.Id,
            Name = a.Name,
            ImageUrl = a.ImageUrl,
            County = a.User.County,
            Town = a.User.Town,
            Latitude = a.User.Latitude ?? 0,
            Longitude = a.User.Longitude ?? 0
        };

        protected override List<Expression<Func<Artist, bool>>> Filters(SearchParams searchParams)
        {
            var filters = new List<Expression<Func<Artist, bool>>>();


            if (searchParams.GenreIds != null && searchParams.GenreIds.Any())
            {
                filters.Add(e => e.ArtistGenres.Any(ag => searchParams.GenreIds.Contains(ag.GenreId)));
            }

            return filters;
        }

        public async Task<Artist?> GetByUserIdAsync(int id)
        {
            var query = context.Artists.AsQueryable();
            query = query.Where(v => v.UserId == id);

            return await query.FirstOrDefaultAsync();
        }
    }
}
