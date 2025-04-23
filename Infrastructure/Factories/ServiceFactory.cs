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
    public abstract class ServiceFactory
    {
        protected readonly IDictionary<string, Type> serviceTypes;
        protected readonly IServiceScopeFactory scopeFactory;

        protected ServiceFactory(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
            serviceTypes = new Dictionary<string, Type>();
        }
    }
}
