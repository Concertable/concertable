using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Payment;

public interface IUserPaymentService
{
    Task<PaymentResponse> PayVenueManagerByConcertIdAsync(int concertId, int quantity, string paymentMethodId, decimal price);
    Task<PaymentResponse> PayArtistManagerByApplicationIdAsync(int applicationId, string paymentMethodId);
}
