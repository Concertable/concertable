using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Contract.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Contract.Api")]
// Ride-along (Â§3.3): Concert.Infrastructure applies ContractEntityConfiguration on ConcertDbContext.
[assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Web.E2ETests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
