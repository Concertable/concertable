using Application.Interfaces.Search;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Factories
{
    public class SearchServiceFactory : ISearchServiceFactory
    {
        private readonly IServiceProvider serviceProvider;

        public SearchServiceFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public ISearchService? Create(string type)
            => serviceProvider.GetKeyedService<ISearchService>(type.ToLower());
    }
}
