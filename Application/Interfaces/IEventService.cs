using Core.Entities;
using Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Core.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces
{
    public interface IEventService
    {
        Task<EventDto> GetDetailsByIdAsync(int id);
        Task<IEnumerable<EventDto>> GetUpcomingByVenueIdAsync(int id);
        Task<IEnumerable<EventDto>> GetUpcomingByArtistIdAsync(int id);
        Task<PaginationResponse<EventHeaderDto>> GetHeadersAsync(SearchParams searchParams);
        Task<EventDto> CreateFromApplicationIdAsync(int id);
    }
}
