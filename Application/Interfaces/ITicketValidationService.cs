using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITicketValidationService
    {
        /// <summary>
        /// Prevents the user from purchasing a ticket if:
        /// The event doesnt exist
        /// The event has passed
        /// The event hasn't been posted
        /// </summary>
        Task<ValidationResponse> CanPurchaseTicketAsync(int eventId);
    }
}
