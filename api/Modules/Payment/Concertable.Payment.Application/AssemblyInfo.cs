using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Payment.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Payment.Api")]
// TEMPORARY: legacy hosts still consume Payment internals until Steps 7/8/12 retire them.
// Concertable.Infrastructure entry retired Step 10 (legacy payment service files all deleted).
[assembly: InternalsVisibleTo("Concertable.Web")]
[assembly: InternalsVisibleTo("Concertable.Workers")]
[assembly: InternalsVisibleTo("Concertable.IntegrationTests.Common")]
[assembly: InternalsVisibleTo("Concertable.Payment.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.E2ETests.Api")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
// Concert.Application hosts ITicketService; signature references Payment-internal DTOs/Responses
// (TicketPaymentResponse, PurchaseCompleteDto).
[assembly: InternalsVisibleTo("Concertable.Concert.Application")]
// Concert.Infrastructure uses IStripeValidator + IStripeValidationFactory in
// OpportunityService/ApplicationService for pre-create/pre-apply Stripe eligibility checks.
// Also uses PurchaseCompleteDto in PaymentSucceededEventHandler.
// TEMPORARY until eligibility routes through a Payment.Contracts facade.
[assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]
// Concert.Api hosts TicketController; signature returns Payment-internal TicketPaymentResponse.
[assembly: InternalsVisibleTo("Concertable.Concert.Api")]
// Concert integration tests deserialize TicketPaymentResponse + reference ITransaction via fixture round-trips.
[assembly: InternalsVisibleTo("Concertable.Concert.IntegrationTests")]
// Customer.Ticket extraction (Phase 1): ITicketService/Controller signatures still reference Payment-internal
// TicketPaymentResponse + PurchaseCompleteDto. Retires when those types become Customer.Ticket-owned or
// the ticket payment flow rewires through bus events.
[assembly: InternalsVisibleTo("Concertable.Customer.Ticket.Application")]
[assembly: InternalsVisibleTo("Concertable.Customer.Ticket.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Customer.Ticket.Api")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
