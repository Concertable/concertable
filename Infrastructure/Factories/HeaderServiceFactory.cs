using Application.Interfaces.Search;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Factories
{
    public class HeaderServiceFactory : IHeaderServiceFactory
    {
        private readonly IServiceProvider serviceProvider;

        public HeaderServiceFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IHeaderService? Create(string type)
            => serviceProvider.GetKeyedService<IHeaderService>(type.ToLower());
    }
}
