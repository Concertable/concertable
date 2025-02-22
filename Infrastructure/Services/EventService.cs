using AutoMapper;
using Core.Entities;
using Application.Interfaces;
using Core.Parameters;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Core.Responses;
using System.Diagnostics;

namespace Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository eventRepository;
        private readonly IReviewService reviewService;
        private readonly ILocationService locationService;
        private readonly IListingApplicationService applicationService;
        private readonly IMapper mapper;

        public EventService(
            IEventRepository eventRepository, 
            IReviewService reviewService, 
            ILocationService locationService,
            IListingApplicationService applicationService, 
            IMapper mapper)
        {
            this.eventRepository = eventRepository;
            this.reviewService = reviewService;
            this.locationService = locationService;
            this.applicationService = applicationService;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<EventDto>> GetUpcomingByVenueIdAsync(int id)
        {
            var events = await eventRepository.GetUpcomingByVenueIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<PaginationResponse<EventHeaderDto>> GetHeadersAsync(SearchParams searchParams)
        {
            var headers = await eventRepository.GetRawHeadersAsync(searchParams);

            await reviewService.AddAverageRatingsAsync(headers.Data);

            var locationHeaders = locationService.FilterAndSortByNearest(searchParams, headers.Data);

            return new PaginationResponse<EventHeaderDto>(
                locationHeaders,
                headers.TotalCount,
                headers.PageNumber,
                headers.PageSize);
        }

        public async Task<IEnumerable<EventDto>> GetUpcomingByArtistIdAsync(int id)
        {
            var events = await eventRepository.GetUpcomingByArtistIdAsync(id);
            return mapper.Map<IEnumerable<EventDto>>(events);
        }

        public async Task<EventDto> GetDetailsByIdAsync(int id)
        {
            var eventEntity = await eventRepository.GetByIdAsync(id);

            return mapper.Map<EventDto>(eventEntity);
        }

        public async Task<EventDto> CreateFromApplicationIdAsync(int id)
        {
            var (artistDto, venueDto) = await applicationService.GetArtistAndVenueByIdAsync(id);

            var eventEntity = new Event
            {
                ApplicationId = id,
                Name = $"{artistDto.Name} performing at {venueDto.Name}",
                Price = 0,
                TotalTickets = 0,
                AvailableTickets = 0,
                Posted = false
            };

            await eventRepository.AddAsync(eventEntity);
            await eventRepository.SaveChangesAsync();

            return mapper.Map<EventDto>(eventEntity);
        }
    }
}
