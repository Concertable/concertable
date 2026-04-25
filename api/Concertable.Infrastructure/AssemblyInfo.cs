using System.Runtime.CompilerServices;

// TEMPORARY: Concert.Infrastructure (during the modular-monolith extraction transition) consumes
// internal helpers (QueryableConcertMappers, QueryableReviewMappers, IBackgroundTaskQueue, etc.)
// from this legacy assembly while the cross-extraction-boundary refs persist (payment + ratings).
// Removed when Payment Stage 1 + the rating-pipeline rewrite (MM_NORTH_STAR §5) collapse the
// remaining temp refs.
[assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Workers")]
[assembly: InternalsVisibleTo("Concertable.Web")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Web.IntegrationTests")]
// TEMPORARY: Castle Core dynamic proxy IVT — Concertable.Infrastructure.UnitTests mocks internal
// `ITicketPaymentStrategyFactory` (Concertable.Infrastructure.Factories). Retires when payment
// internals migrate into Concertable.Payment.Infrastructure under Payment Stage 1.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
