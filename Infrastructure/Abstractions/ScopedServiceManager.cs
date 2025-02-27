using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Abstractions
{
    public abstract class ScopedServiceManager : IScopeDisposable
    {
        protected readonly IServiceProvider serviceProvider;
        protected IServiceScope scope;

        public ScopedServiceManager(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public virtual void CreateScope()
        {
            DisposeScope();
            scope = serviceProvider.CreateScope();
        }

        public virtual void DisposeScope()

        {
            scope?.Dispose();
            scope = null;
        }

        public void Dispose()
        {
            DisposeScope();
        }
    }
}
