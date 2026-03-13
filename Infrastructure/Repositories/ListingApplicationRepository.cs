using Core.Entities;
using Application.Interfaces;
using Infrastructure.Data.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

namespace Infrastructure.Repositories;

public class ListingApplicationRepository : Repository<ListingApplication>, IListingApplicationRepository
{
    private readonly TimeProvider timeProvider;

    public ListingApplicationRepository(ApplicationDbContext context, TimeProvider timeProvider) : base(context)
    {
        this.timeProvider = timeProvider;
    }

    public async Task<IEnumerable<ListingApplication>> GetByListingIdAsync(int id)
    {
        var query = context.ListingApplications
        .Where(la => la.ListingId == id)
        .Include(la => la.Artist)
            .ThenInclude(a => a.User)
        .Include(la => la.Artist)
            .ThenInclude(a => a.ArtistGenres)
                .ThenInclude(ag => ag.Genre);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<ListingApplication>> GetPendingByArtistIdAsync(int artistId)
    {
        var query = context.ListingApplications
        .Include(a => a.Listing)
            .ThenInclude(l => l.Venue)
        .Where(a =>
            a.ArtistId == artistId &&
            !context.Concerts.Any(e => e.ApplicationId == a.Id) &&
            a.Listing.StartDate > timeProvider.GetUtcNow());

        return await query.ToListAsync();
    }

    public async Task<(Artist, Venue)?> GetArtistAndVenueByIdAsync(int id)
    {
        var query = await context.ListingApplications
            .Where(la => la.Id == id)
            .Include(la => la.Artist)
                .ThenInclude(a => a.User)
            .Include(la => la.Artist)
                .ThenInclude(a => a.ArtistGenres)
            .Include(la => la.Listing)
                .ThenInclude(l => l.Venue)
                    .ThenInclude(v => v.User)
            .Include(la => la.Listing.ListingGenres)
            .FirstOrDefaultAsync();

        if (query is null) return null;
        return (query.Artist, query.Listing.Venue);
    }



    public async Task<decimal?> GetListingPayByIdAsync(int id)
    {
        var query = context.ListingApplications
            .Where(la => la.Id == id)
            .Select(la => (decimal?)la.Listing.Pay);

        return await query.FirstOrDefaultAsync();
    }

    public async new Task<ListingApplication?> GetByIdAsync(int id)
    {
        var query = context.ListingApplications
            .Where(la => la.Id == id)
            .Include(la => la.Artist)
                .ThenInclude(la => la.ArtistGenres)
                    .ThenInclude(la => la.Genre)
            .Include(la => la.Artist.User)
            .Include(la => la.Listing);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ListingApplication>> GetRecentDeniedByArtistIdAsync(int artistId)
    {
        var query = context.ListingApplications
            .Include(a => a.Listing)
                .ThenInclude(l => l.Venue)
            .Where(a =>
                a.ArtistId == artistId &&
                context.Concerts.Any(e =>
                    e.Application.ListingId == a.ListingId &&
                    e.ApplicationId != a.Id)) // someone else was accepted
            .OrderByDescending(a => a.Listing.EndDate)
            .Take(5);

        return await query.ToListAsync();
    }
}
