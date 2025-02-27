using Application.Interfaces;
using Infrastructure.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Factories
{
    public abstract class ServiceFactory : ScopedServiceManager
    {
        protected readonly IDictionary<string, Type> serviceTypes;

        public ServiceFactory(IServiceProvider serviceProvider) : base(serviceProvider) 
        {
            serviceTypes = new Dictionary<string, Type>();
        }
    }
}
