using Application.Interfaces.Search;
using Core.Enums;
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

        public IHeaderService? Create(HeaderType type)
            => serviceProvider.GetKeyedService<IHeaderService>(type.ToString().ToLower());
    }
}
