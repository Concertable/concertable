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
using Application.Responses;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using Common.Helpers;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => LocationHelper.GetLatitude(src.Location)))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => LocationHelper.GetLongitude(src.Location)));
            CreateMap<UserDto, ApplicationUser>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => LocationHelper.CreatePoint(src.Latitude, src.Longitude)));

            CreateMap<Genre, GenreDto>().ReverseMap();

            //Venue
            CreateMap<Venue, VenueHeaderDto>();
            CreateMap<VenueHeaderDto, Venue>();
            CreateMap<Venue, VenueDto>()
            .ForMember(
                dest => dest.Latitude,
                opt => opt.MapFrom(src => src.User.Location != null ? src.User.Location.Y : 0)) // Extract Latitude from Location
            .ForMember(
                dest => dest.Longitude,
                opt => opt.MapFrom(src => src.User.Location != null ? src.User.Location.X : 0)) // Extract Longitude from Location
            .ForMember(
                dest => dest.County,
                opt => opt.MapFrom(src => src.User.County))
            .ForMember(
                dest => dest.Town,
                opt => opt.MapFrom(src => src.User.Town))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.User.Email))
            .ReverseMap();

            //Artist
            CreateMap<Artist, ArtistDto>()
            .ForMember(dest => dest.Genres, opt =>
                opt.MapFrom(src => src.ArtistGenres
                    .Select(ag => ag.Genre))) // Let AutoMapper map Genre → GenreDto
            .ForMember(dest => dest.County,
                opt => opt.MapFrom(src => src.User.County))
            .ForMember(dest => dest.Town,
                opt => opt.MapFrom(src => src.User.Town))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.User.Email));
            CreateMap<CreateVenueDto, Venue>();

            CreateMap<ArtistDto, Artist>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
            CreateMap<Artist, ArtistHeaderDto>();
            CreateMap<ArtistHeaderDto, Artist>();
            CreateMap<CreateArtistDto, Artist>();
            //Listing
            CreateMap<Listing, ListingDto>()
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(
                src => src.ListingGenres.Select(lg => new GenreDto
                {
                    Id = lg.Genre.Id,
                    Name = lg.Genre.Name
                })))
            .ReverseMap();


            //Events
            CreateMap<Event, EventHeaderDto>();
            CreateMap<EventHeaderDto, Event>();
            CreateMap<EventDto, EventHeaderDto>();
            CreateMap<EventHeaderDto, EventDto>();
            CreateMap<Event, EventDto>()
            .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom(src => src.Application.Listing.StartDate))
            .ForMember(dest => dest.EndDate,
                opt => opt.MapFrom(src => src.Application.Listing.EndDate))
            .ForMember(dest => dest.Venue,
                opt => opt.MapFrom(src => src.Application.Listing.Venue))
            .ForMember(dest => dest.Artist,
                opt => opt.MapFrom(src => src.Application.Artist))
            .ForMember(dest => dest.Genres,
                opt => opt.MapFrom(src => src.EventGenres.Select(eg => eg.Genre)))
            .ReverseMap();



            //Messages
            CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.FromUser, opt => opt.MapFrom(src => src.FromUser))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src =>
                !string.IsNullOrEmpty(src.Action) && src.ActionId.HasValue
                    ? new ActionDto
                    {
                        Name = src.Action,
                        Id = src.ActionId.Value
                    }
                    : null
            ))
            .ReverseMap();


            //Purchase
            CreateMap<Purchase, PurchaseDto>();
            CreateMap<PurchaseDto, Purchase>();

            //ListingApplications
            CreateMap<ListingApplication, ListingApplicationDto>()
            .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Artist))
            .ForMember(dest => dest.Listing, opt => opt.MapFrom(src => src.Listing));

            //Reviews
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.Email, opt =>  opt.MapFrom(src => src.Ticket.User.Email));
            CreateMap<ReviewDto, Review>();

            CreateMap<CreatePreferenceDto, Preference>();
            CreateMap<Preference, PreferenceDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.GenrePreferences
                    .Select(gp => gp.Genre)));
            CreateMap<PreferenceDto, Preference>();

            CreateMap<Ticket, TicketDto>();
            CreateMap<TicketDto, Ticket>();
        }
    }
}
