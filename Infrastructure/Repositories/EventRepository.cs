using Core.Entities;
using Core.Interfaces;
using Core.Parameters;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Event>> GetHeadersAsync(SearchParams searchParams)
        {
            var query = context.Events.AsQueryable();
            query = query.Select(v => new Event
            {
                Id = v.Id,
                Name = v.Name
            });

            if (!string.IsNullOrWhiteSpace(searchParams?.Sort))
            {
                query = query.OrderBy(v => searchParams.Sort);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingByVenueIdAsync(int id)
        {
            var query = (from events in context.Events
                        join registers in context.Registers on events.RegisterId equals registers.Id
                        join listings in context.Listings on registers.ListingId equals listings.Id
                        join venues in context.Venues on listings.VenueId equals venues.Id
                        where venues.Id == id && listings.StartDate >= DateTime.Today
                        select events)
                        .Include(e => e.Register)
                        .ThenInclude(r => r.Listing);

            return await query.ToListAsync();
        }

        public void Remove(Event entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Event entity)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<Event>> IRepository<Event>.GetAllAsync()
        {
            throw new NotImplementedException();
        }
    }
}
