using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Concertable.Payment.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Payment.Api")]
// TEMPORARY: legacy hosts still consume Payment internals until Steps 7/8/12 retire them.
[assembly: InternalsVisibleTo("Concertable.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Web")]
[assembly: InternalsVisibleTo("Concertable.Workers")]
[assembly: InternalsVisibleTo("Concertable.Web.IntegrationTests")]
[assembly: InternalsVisibleTo("Concertable.Web.E2ETests")]
[assembly: InternalsVisibleTo("Concertable.Infrastructure.UnitTests")]
[assembly: InternalsVisibleTo("Concertable.Workers.UnitTests")]
// TEMPORARY: Concert.Infrastructure currently hosts the webhook handlers (carryover from
// Concert extraction). They move to Payment.Infrastructure in Step 7 and this IVT retires.
[assembly: InternalsVisibleTo("Concertable.Concert.Infrastructure")]
// Concert.Application hosts ITicketService (Concert orchestrates ticket purchase per Option B);
// signature references Payment-internal DTOs/Responses (TicketPaymentResponse, PurchaseCompleteDto).
[assembly: InternalsVisibleTo("Concertable.Concert.Application")]
// TEMPORARY: Identity.Infrastructure registers consume IStripeAccountService directly to write
// UserEntity.StripeCustomerId / ManagerEntity.StripeAccountId during signup. Step 7 inverts this
// (Payment subscribes to UserCreated event → writes PayoutAccountEntity); IVT retires then.
[assembly: InternalsVisibleTo("Concertable.Identity.Infrastructure")]
// Concert.Api hosts TicketController (Concert orchestrates ticket purchase per Option B);
// signature returns Payment-internal TicketPaymentResponse.
[assembly: InternalsVisibleTo("Concertable.Concert.Api")]
