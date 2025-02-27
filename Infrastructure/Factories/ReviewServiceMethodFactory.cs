using Application.DTOs;
using Application.Interfaces;
using Application.Responses;
using Core.Parameters;
using Infrastructure.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Infrastructure.Factories
{
    public class ReviewServiceMethodFactory : ScopedServiceManager, IReviewServiceMethodFactory
    {
        private readonly IServiceScopeFactory scopeFactory;
        private IServiceScope scope;
        private IReviewService reviewService;
        private readonly IDictionary<string, Func<int, PaginationParams, Task<PaginationResponse<ReviewDto>>>> serviceMethods;

        public ReviewServiceMethodFactory(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            // The dictionary is initialized, but service resolution happens later
            serviceMethods = new Dictionary<string, Func<int, PaginationParams, Task<PaginationResponse<ReviewDto>>>>()
            {
                { "venue", (id, pagination) => reviewService.GetByVenueIdAsync(id, pagination) },
                { "artist", (id, pagination) => reviewService.GetByArtistIdAsync(id, pagination) },
                { "event", (id, pagination) => reviewService.GetByEventIdAsync(id, pagination) }
            };
        }

        public override void CreateScope()
        {
            base.DisposeScope();
            reviewService = scope.ServiceProvider.GetRequiredService<IReviewService>();
        }

        public override void DisposeScope()
        {
            base.DisposeScope();
            reviewService = null;
        }

        public Func<int, PaginationParams, Task<PaginationResponse<ReviewDto>>> GetMethod(string entityType)
        {
            if (reviewService == null)
                throw new InvalidOperationException("Scope has not been created. Call CreateScope() before using GetMethod().");

            if (serviceMethods.TryGetValue(entityType, out var method))
                return method;

            throw new ArgumentOutOfRangeException($"No service found for review type '{entityType}'.");
        }
    }
}
