using Application.DTOs;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class HeaderServiceMapper : IHeaderServiceMapper
    {
        private readonly IHeaderServiceFactory factory;

        public HeaderServiceMapper(IHeaderServiceFactory factory)
        {
            this.factory = factory;
        }

        public IHeaderService<TDto> Resolve<TDto>(string headerType) where TDto : HeaderDto
        {
            // Debugging: Log the types for reference
            Debug.WriteLine($"HeaderType: {headerType}, Expected Type: {typeof(TDto).Name}");

            return headerType.ToLower() switch
            {
                "venue" =>
                    (IHeaderService<TDto>)factory.GetService<VenueHeaderDto>(headerType),

                "artist" =>
                    (IHeaderService<TDto>)factory.GetService<ArtistHeaderDto>(headerType),

                "event" =>
                    (IHeaderService<TDto>)factory.GetService<EventHeaderDto>(headerType),

                _ => throw new ArgumentException($"Unsupported header type '{headerType}' for DTO {typeof(TDto).Name}")
            };
        }

    }

}
