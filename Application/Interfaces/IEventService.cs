using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces
{
    public interface IEventService : IHeaderService<EventHeaderDto>
    {
        Task<EventDto> GetDetailsByIdAsync(int id);
        Task<EventDto> GetDetailsByApplicationIdAsync(int applicationId);
        Task<IEnumerable<EventDto>> GetUpcomingByVenueIdAsync(int id);
        Task<IEnumerable<EventDto>> GetUpcomingByArtistIdAsync(int id);
        Task<ListingApplicationPurchaseResponse> BookAsync(EventBookingParams bookingParams);
        Task<ListingApplicationPurchaseResponse> CompleteAsync(PurchaseCompleteDto purchaseCompleteDto);
    }
}
