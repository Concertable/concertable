using Concertable.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concertable.Application.Interfaces;

public interface IStripeEventRepository
{
    Task<StripeEventEntity?> GetEventByIdAsync(string eventId);
    Task AddEventAsync(StripeEventEntity stripeEvent);
    Task<bool> EventExistsAsync(string eventId);
}
