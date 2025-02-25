using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;

namespace Infrastructure.Factories
{
    public class HeaderServiceFactory : IHeaderServiceFactory, IDisposable
    {
        private readonly IServiceProvider serviceProvider;
        private IServiceScope scope;
        private readonly Dictionary<string, Type> serviceTypes;

        public HeaderServiceFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;

            this.serviceTypes = new Dictionary<string, Type>()
            {
                { "venue", typeof(IHeaderService<VenueHeaderDto>) },
                { "artist", typeof(IHeaderService<ArtistHeaderDto>) },
                { "event", typeof(IHeaderService<EventHeaderDto>) }
            };
        }

        public void CreateScope()
        {
            DisposeScope(); 
            scope = serviceProvider.CreateScope();
        }

        public void DisposeScope()
        {
            scope?.Dispose();
            scope = null;
        }

        public IHeaderService<TDto> GetService<TDto>(string entityType) where TDto : HeaderDto
        {
            if (!serviceTypes.TryGetValue(entityType, out var serviceType))
                throw new ArgumentException($"No service found for entity type '{entityType}'.");

            return (IHeaderService<TDto>)scope.ServiceProvider.GetRequiredService(serviceType);
        }

        public void Dispose()
        {
            DisposeScope();
        }
    }
}
