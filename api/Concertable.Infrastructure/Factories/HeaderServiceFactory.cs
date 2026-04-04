using Concertable.Application.Interfaces.Search;
using Concertable.Core.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Factories;

public class HeaderServiceFactory : IHeaderServiceFactory
{
    private readonly IServiceProvider serviceProvider;

    public HeaderServiceFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IHeaderService Create(HeaderType type)
        => serviceProvider.GetRequiredKeyedService<IHeaderService>(type);
}
