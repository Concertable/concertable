using Concertable.Application.Interfaces;
using Concertable.Core.Entities;
using Concertable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Concertable.Infrastructure.Repositories;

public class ConcertBookingRepository : IConcertBookingRepository
{
    private readonly ApplicationDbContext context;

    public ConcertBookingRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(ConcertBookingEntity booking)
    {
        await context.ConcertBookings.AddAsync(booking);
    }

    public async Task<ConcertBookingEntity?> GetByApplicationIdAsync(int applicationId)
    {
        return await context.ConcertBookings
            .FirstOrDefaultAsync(b => b.ApplicationId == applicationId);
    }

    public async Task<ConcertBookingEntity?> GetByConcertIdAsync(int concertId)
    {
        return await context.ConcertBookings
            .FirstOrDefaultAsync(b => b.Concert.Id == concertId);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
