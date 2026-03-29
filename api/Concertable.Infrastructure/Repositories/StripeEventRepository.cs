using Concertable.Application.Interfaces;
using Concertable.Infrastructure.Data;
using Concertable.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Infrastructure.Repositories;

public class StripeEventRepository : IStripeEventRepository
{
    private readonly ApplicationDbContext context;

    public StripeEventRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<StripeEventEntity?> GetEventByIdAsync(string eventId)
    {
        return await context.StripeEvents
            .FirstOrDefaultAsync(e => e.EventId == eventId);
    }

    public async Task AddEventAsync(StripeEventEntity stripeEvent)
    {
        await context.StripeEvents.AddAsync(stripeEvent);
        await context.SaveChangesAsync();
    }

    public async Task<bool> EventExistsAsync(string eventId)
    {
        return await context.StripeEvents
            .AnyAsync(e => e.EventId == eventId);
    }
}
