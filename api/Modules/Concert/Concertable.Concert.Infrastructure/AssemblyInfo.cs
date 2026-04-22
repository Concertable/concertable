using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Concert.Api")]
[assembly: InternalsVisibleTo("Concertable.Web.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
// TEMPORARY: Concertable.Workers re-registers Concert dispatchers/strategies until Step 12 collapses to AddConcertModule().
[assembly: InternalsVisibleTo("Concertable.Workers")]
// TEMPORARY: Concertable.Web (DevController + E2EEndpointExtensions + ServiceCollectionExtensions DI registrations)
// continues to inject Concert internals until Steps 9 (Concert.Api) and 11/12 (AddConcertModule) land.
[assembly: InternalsVisibleTo("Concertable.Web")]
// TEMPORARY: legacy Concertable.Infrastructure still hosts Payment services that need to inject IConcertBookingRepository
// for the read-model UserId hop. Removed when Payment Stage 1 extracts those services.
[assembly: InternalsVisibleTo("Concertable.Infrastructure")]
