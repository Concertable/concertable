using Concertable.Application.Interfaces;
using Concertable.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Factories;

public class ReviewServiceFactory : IReviewServiceFactory
{
    private readonly IServiceProvider serviceProvider;

    public ReviewServiceFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IReviewService Create(ReviewType type)
        => serviceProvider.GetRequiredKeyedService<IReviewService>(type);
}
