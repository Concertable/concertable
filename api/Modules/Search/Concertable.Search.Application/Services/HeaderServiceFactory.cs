using Concertable.Core.Enums;
using Concertable.Search.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Search.Application.Services;

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
