using Concertable.Application.Interfaces;
using Concertable.IntegrationTests.Common.Mocks;
using Concertable.Payment.Application.Interfaces.Webhook;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Concertable.IntegrationTests.Common;

public class TestClientOptions
{
    public Action<IConfigurationBuilder>? Configure { get; set; }
    public Action<IServiceCollection>? Services { get; set; }

    public TestClientOptions UseFailingStripe()
    {
        Services += services => services.Replace(ServiceDescriptor.Singleton<IStripeClient, MockStripeClientFail>());
        return this;
    }

    public TestClientOptions UseFailingPayment()
    {
        Services += services => services.Replace(ServiceDescriptor.Singleton<IStripePaymentClient, MockStripePaymentClientFail>());
        return this;
    }

    public TestClientOptions UseFailingGeocoding()
    {
        Services += services => services.Replace(ServiceDescriptor.Scoped<IGeocodingService, MockGeocodingServiceFail>());
        return this;
    }
}
