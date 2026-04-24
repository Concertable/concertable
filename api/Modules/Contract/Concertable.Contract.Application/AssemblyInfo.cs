using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Contract.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Contract.Api")]
// Ride-along (§3.3): Concert.Infrastructure applies ContractEntityConfiguration on ConcertDbContext.
[assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Web.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Web.E2ETests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
// TEMPORARY: legacy Concertable.Application / Concertable.Infrastructure still hold consumers that touch Contract internals.
// Retires in Step 11 once Concert + Payment fully consume IContractModule.
[assembly: InternalsVisibleTo("Concertable.Application")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Workers")]
[assembly: InternalsVisibleTo("Concertable.Web")]
