using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Payment.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Payment.Api")]
// TEMPORARY: legacy hosts still consume Payment internals until Steps 7/8/12 retire them.
// Concertable.Infrastructure entry retired Step 10 (legacy payment service files all deleted).
[assembly: InternalsVisibleTo("Concertable.Web")]
[assembly: InternalsVisibleTo("Concertable.Workers")]
[assembly: InternalsVisibleTo("Concertable.Web.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Web.E2ETests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
// Concert.Application hosts ITicketService (Concert orchestrates ticket purchase per Option B);
// signature references Payment-internal DTOs/Responses (TicketPaymentResponse, PurchaseCompleteDto).
[assembly: InternalsVisibleTo("Concertable.Concert.Application")]
// Concert.Infrastructure uses IStripeValidator + IStripeValidationFactory in
// OpportunityService/OpportunityApplicationService for pre-create/pre-apply Stripe eligibility checks.
// TEMPORARY until eligibility routes through a Payment.Contracts facade.
[assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]
// Concert.Api hosts TicketController (Concert orchestrates ticket purchase per Option B);
// signature returns Payment-internal TicketPaymentResponse.
[assembly: InternalsVisibleTo("Concertable.Concert.Api")]
