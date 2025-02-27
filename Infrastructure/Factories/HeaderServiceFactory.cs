using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;

namespace Infrastructure.Factories
{
    public class HeaderServiceFactory : ServiceFactory, IHeaderServiceFactory
    {

        public HeaderServiceFactory(IServiceProvider serviceProvider) : base(serviceProvider) 
        {
            serviceTypes.Add("venue", typeof(IHeaderService<VenueHeaderDto>));
            serviceTypes.Add("artist", typeof(IHeaderService<ArtistHeaderDto>));
            serviceTypes.Add("event", typeof(IHeaderService<EventHeaderDto>));
        }

        public IHeaderService<TDto> GetService<TDto>(string entityType) where TDto : HeaderDto
        {
            if (serviceTypes.TryGetValue(entityType, out var serviceType))
                return (IHeaderService<TDto>)scope.ServiceProvider.GetRequiredService(serviceType);

            throw new ArgumentOutOfRangeException($"No service found for header type '{entityType}'.");
        }
    }
}
