using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStripeEventRepository
    {
        Task<StripeEvent?> GetEventByIdAsync(string eventId);
        Task AddEventAsync(StripeEvent stripeEvent);
        Task<bool> EventExistsAsync(string eventId);
    }
}
