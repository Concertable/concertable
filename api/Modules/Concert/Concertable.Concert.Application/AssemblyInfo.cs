using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Concert.Api")]
[assembly: InternalsVisibleTo("Concertable.Concert.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Web.E2ETests")]
// TEMPORARY: Castle Core dynamic proxy IVT — needed by Concertable.Infrastructure.UnitTests / Concertable.Workers.UnitTests
// to mock internal interfaces (IConcertRepository, IContractLoader, IConcertWorkflowFactory, etc.).
// Retires when those unit tests migrate into per-module test projects (Concertable.Concert.UnitTests etc.)
// and stop mocking another module's internals.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
// TEMPORARY: legacy Concertable.Infrastructure still hosts Payment + Ticket services that inject Concert.Application
// internals (IConcertRepository, IOpportunityRepository, IContractLoader, ITicketPaymentStrategy). Retires when
// Payment Stage 1 extracts those services into Concertable.Payment.Infrastructure.
[assembly: InternalsVisibleTo("Concertable.Infrastructure")]
// TEMPORARY: Concertable.Workers (ConcertFinishedFunction) injects IConcertRepository + ICompletionExecutor.
// Retires when the function moves into Concert.Api or its own Concert-owned worker.
[assembly: InternalsVisibleTo("Concertable.Workers")]
// TEMPORARY: Concertable.Web (E2EEndpointExtensions injects ICompletionExecutor; ServiceCollectionExtensions
// keyed-registers ITicketPaymentStrategy impls). Retires when those move into Concert.Api / Payment.Infrastructure.
[assembly: InternalsVisibleTo("Concertable.Web")]
