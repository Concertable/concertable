using Concertable.Search.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Search.Application.Services;

internal class AutocompleteServiceFactory : IAutocompleteServiceFactory
{
    private readonly IServiceProvider serviceProvider;

    public AutocompleteServiceFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public IAutocompleteService Create(HeaderType? type)
        => serviceProvider.GetRequiredKeyedService<IAutocompleteService>(type);
}
