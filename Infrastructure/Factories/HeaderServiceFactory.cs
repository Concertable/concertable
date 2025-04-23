using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

namespace Infrastructure.Factories
{
    public class HeaderServiceFactory : IHeaderServiceFactory
    {
        private readonly IHttpContextAccessor httpContext;
        private readonly IDictionary<string, Type> serviceTypes
          = new Dictionary<string, Type>();

        public HeaderServiceFactory(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContext)
        {
            this.httpContext = httpContext;
            serviceTypes.Add("venue", typeof(IHeaderService<VenueHeaderDto>));
            serviceTypes.Add("artist", typeof(IHeaderService<ArtistHeaderDto>));
            serviceTypes.Add("event", typeof(IHeaderService<EventHeaderDto>));
        }

        public IHeaderService<TDto> GetService<TDto>(string entityType) where TDto : HeaderDto
        {
            if(!serviceTypes.TryGetValue(entityType, out var svcType))
            throw new ArgumentOutOfRangeException(
                nameof(entityType),
                $"No service for '{entityType}'.");

            var ctx = httpContext.HttpContext
                      ?? throw new InvalidOperationException("No active HttpContext");

            // Resolve from the current request scope
            return (IHeaderService<TDto>)ctx
                .RequestServices
                .GetRequiredService(svcType);
        }
    }
}
