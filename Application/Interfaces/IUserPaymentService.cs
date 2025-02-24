using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserPaymentService
    {
        Task<PaymentResponse> PayVenueManagerByEventIdAsync(int eventId, string paymentMethodId);
        Task<PaymentResponse> PayArtistManagerByApplicationIdAsync(int applicationId, string paymentMethodId);
    }
}
