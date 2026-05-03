using Concertable.User.Contracts;
using Concertable.User.Contracts.Events;
using Concertable.Payment.Application.Interfaces;
using Concertable.Shared;

namespace Concertable.Payment.Infrastructure.Handlers;

internal class UserRegisteredHandler(IStripeAccountClient stripeAccountClient)
    : IIntegrationEventHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent e, CancellationToken ct = default)
    {
        if (e.Role is Role.Admin)
            return;

        await stripeAccountClient.ProvisionCustomerAsync(e.UserId, e.Email, ct);

        if (e.Role is Role.ArtistManager or Role.VenueManager)
            await stripeAccountClient.ProvisionConnectAccountAsync(e.UserId, e.Email, ct);
    }
}
