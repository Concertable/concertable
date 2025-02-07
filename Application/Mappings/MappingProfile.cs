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
            //Artist
            CreateMap<Artist, ArtistDto>();
            CreateMap<ArtistDto, Artist>();
            CreateMap<Artist, ArtistHeaderDto>();
            CreateMap<ArtistHeaderDto, Artist>();
            //Listing
            CreateMap<Listing, ListingDto>();
            CreateMap<ListingDto, Listing>();
            //Events
            CreateMap<Event, EventDto>();
            CreateMap<EventDto, Event>();
            CreateMap<Event, EventHeaderDto>();
            CreateMap<EventHeaderDto, Event>();
        }
    }
}
