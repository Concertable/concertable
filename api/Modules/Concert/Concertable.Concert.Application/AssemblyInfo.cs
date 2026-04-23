using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Concert.Api")]
[assembly: InternalsVisibleTo("Concertable.Web.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Web.E2ETests")]
// TEMPORARY: Castle Core dynamic proxy IVT — needed by Concertable.Infrastructure.UnitTests / Concertable.Workers.UnitTests
// to mock internal interfaces (IConcertRepository, IContractStrategyResolver, etc.). Retires when those unit tests
// migrate into per-module test projects (Concertable.Concert.UnitTests etc.) and stop mocking another module's internals.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
// TEMPORARY: legacy Concertable.Infrastructure still hosts Concert service impls until Step 7 moves them.
// TEMPORARY: Concertable.Workers re-registers Concert dispatchers/strategies until Step 12 collapses to AddConcertModule().
[assembly: InternalsVisibleTo("Concertable.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Workers")]
// TEMPORARY: DevController + E2EEndpointExtensions inject IAcceptDispatcher/IFinishedDispatcher
// directly. Remove when those routes move into Concert.Api. See CONCERT_MODULE_REFACTOR.md §Stage 0.
[assembly: InternalsVisibleTo("Concertable.Web")]
