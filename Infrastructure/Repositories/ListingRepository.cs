using Core.Entities;
using Application.Interfaces;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ListingRepository : Repository<Listing>, IListingRepository
{
    private readonly TimeProvider timeProvider;

    public ListingRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public async Task<IEnumerable<Listing>> GetActiveByVenueIdAsync(int id)
    {
        var query = context.Listings
            .Where(l => l.VenueId == id && l.StartDate >= timeProvider.GetUtcNow())
            .Where(l => !context.Concerts.Any(e => e.ApplicationId ==
                context.ListingApplications
                    .Where(la => la.ListingId == l.Id)
                    .Select(la => la.Id)
                    .FirstOrDefault()))
            .Include(l => l.ListingGenres)
            .ThenInclude(lg => lg.Genre)
            .OrderBy(l => l.StartDate);

        return await query.ToListAsync();
    }

    public async Task<User?> GetOwnerByIdAsync(int listingId)
    {
        return await context.Users
            .OfType<VenueManager>()
            .Where(vm => vm.Venue != null && vm.Venue.Listings.Any(l => l.Id == listingId))
            .FirstOrDefaultAsync();
    }

    public async new Task<Listing?> GetByIdAsync(int id)
    {
        return await context.Listings
            .Where(l => l.Id == id)
            .Include(l => l.ListingGenres)
                .ThenInclude(lg => lg.Genre)
            .FirstOrDefaultAsync();
    }

    public async Task<Listing?> GetWithVenueByIdAsync(int id)
    {
        return await context.Listings
            .Where(l => l.Id == id)
            .Include(l => l.Venue)
            .FirstOrDefaultAsync();
    }

    public async Task<Listing?> GetByApplicationIdAsync(int id)
    {
        return await context.Listings
            .Include(l => l.ListingGenres)
                .ThenInclude(lg => lg.Genre)
            .Include(l => l.Venue)
                .ThenInclude(v => v.User)
            .Where(l => l.Applications.Any(a => a.Id == id))
            .FirstOrDefaultAsync();
    }
}
