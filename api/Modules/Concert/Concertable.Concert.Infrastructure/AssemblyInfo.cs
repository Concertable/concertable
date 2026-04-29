using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Concert.Api")]
[assembly: InternalsVisibleTo("Concertable.Concert.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Concert.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
// TEMPORARY: Concertable.Web injects internal WebhookService (Concert.Infrastructure.Services.Webhook).
// Retires when Webhook routing moves into Concert.Api or a Payment-owned host.
[assembly: InternalsVisibleTo("Concertable.Web")]
