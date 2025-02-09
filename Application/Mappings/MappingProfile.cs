using Application.DTOs;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Core.Responses;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            //Venue
            CreateMap<Venue, VenueDto>();
            CreateMap<VenueDto, Venue>();
            CreateMap<Venue, VenueHeaderDto>();
            CreateMap<VenueHeaderDto, Venue>();
            CreateMap<Venue, VenueDto>()
            .ForMember(
                dest => dest.Coordinates,
                opt => opt.MapFrom(src => new CoordinatesDto
                {
                    Latitude = src.Latitude,
                    Longitude = src.Longitude
                })
            );
            //Artist
            CreateMap<Artist, ArtistDto>();
            CreateMap<ArtistDto, Artist>();
            CreateMap<Artist, ArtistHeaderDto>();
            CreateMap<ArtistHeaderDto, Artist>();
            //Listing
            CreateMap<Listing, ListingDto>();
            CreateMap<ListingDto, Listing>();
            CreateMap<Listing, ListingDto>()
                .ForMember(
                    dest => dest.Genres,
                    opts => opts.MapFrom( //Map from one table to another
                        src => src.ListingGenres
                            .Select(g => g.Genre.Name)
                    )
                );

            //Events
            CreateMap<Event, EventDto>();
            CreateMap<EventDto, Event>();
            CreateMap<Event, EventHeaderDto>();
            CreateMap<EventHeaderDto, Event>();
            CreateMap<Event, EventDto>()
                .ForMember(
                    dest => dest.StartDate,
                    opt => opt.MapFrom(src => src.Application.Listing.StartDate)
                )
                .ForMember(
                    dest => dest.EndDate,
                    opt => opt.MapFrom(src => src.Application.Listing.EndDate)
                );

            //Messages
            CreateMap<Message, MessageDto>();
            CreateMap<MessageDto, Message>();

            //MessageSummary

            //Purchase
            CreateMap<Purchase, PurchaseDto>();
            CreateMap<PurchaseDto, Purchase>();

        }
    }
}
