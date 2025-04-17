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
            CreateMap<Venue, VenueHeaderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.County, opt => opt.MapFrom(src => src.User.County))
                .ForMember(dest => dest.Town, opt => opt.MapFrom(src => src.User.Town))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.User.Location != null ? src.User.Location.Y : (double?)null))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.User.Location != null ? src.User.Location.X : (double?)null));
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
            CreateMap<VenueDto, VenueHeaderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.County, opt => opt.MapFrom(src => src.County))
                .ForMember(dest => dest.Town, opt => opt.MapFrom(src => src.Town))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => (double?)src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => (double?)src.Longitude));


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
            CreateMap<Artist, ArtistHeaderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.County, opt => opt.MapFrom(src => src.User.County))
                .ForMember(dest => dest.Town, opt => opt.MapFrom(src => src.User.Town))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.User.Location != null ? src.User.Location.Y : (double?)null))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.User.Location != null ? src.User.Location.X : (double?)null));
            CreateMap<ArtistHeaderDto, Artist>();
            CreateMap<ArtistDto, ArtistHeaderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.County, opt => opt.MapFrom(src => src.County))
                .ForMember(dest => dest.Town, opt => opt.MapFrom(src => src.Town))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => (double?)src.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => (double?)src.Longitude));

            CreateMap<CreateArtistDto, Artist>();
            //Listing
            CreateMap<Listing, ListingDto>()
            .ForMember(dest => dest.Genres, opt => opt.MapFrom(
                src => src.ListingGenres.Select(lg => new GenreDto
                {
                    Id = lg.Genre.Id,
                    Name = lg.Genre.Name
                })));
            CreateMap<ListingDto, Listing>()
            .ForMember(dest => dest.ListingGenres, opt => opt.MapFrom(src =>
                src.Genres.Select(g => new ListingGenre { GenreId = g.Id }).ToList()));

            //Events
            CreateMap<Event, EventHeaderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Application.Artist.ImageUrl))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Application.Listing.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Application.Listing.EndDate))
                .ForMember(dest => dest.County, opt => opt.MapFrom(src => src.Application.Listing.Venue.User.County))
                .ForMember(dest => dest.Town, opt => opt.MapFrom(src => src.Application.Listing.Venue.User.Town))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Application.Listing.Venue.User.Location != null ? src.Application.Listing.Venue.User.Location.Y : (double?)null))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Application.Listing.Venue.User.Location != null ? src.Application.Listing.Venue.User.Location.X : (double?)null))
                .ForMember(dest => dest.DatePosted, opt => opt.MapFrom(src => src.DatePosted));
            CreateMap<EventDto, EventHeaderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Artist.ImageUrl))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.County, opt => opt.MapFrom(src => src.Venue.County))
                .ForMember(dest => dest.Town, opt => opt.MapFrom(src => src.Venue.Town))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Venue.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Venue.Longitude))
                .ForMember(dest => dest.DatePosted, opt => opt.MapFrom(src => src.DatePosted));
            CreateMap<EventHeaderDto, Event>();
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
            CreateMap<Transaction, TransactionDto>();
            CreateMap<TransactionDto, Transaction>();

            //ListingApplications
            CreateMap<ListingApplication, ListingApplicationDto>()
                .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Artist))
                .ForMember(dest => dest.Listing, opt => opt.MapFrom(src => src.Listing));

            CreateMap<ListingApplication, ArtistListingApplicationDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Artist))
                .ForMember(dest => dest.ListingWithVenue, opt => opt.MapFrom(src => src.Listing));

            CreateMap<Listing, ListingWithVenueDto>()
                .ForMember(dest => dest.Listing, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Venue, opt => opt.MapFrom(src => src.Venue));

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
