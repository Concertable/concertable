using Concertable.Auth.Contracts;
using Concertable.Auth.Contracts.Events;
using Concertable.Customer.User.Infrastructure.Data;
using Concertable.Messaging.Contracts;

namespace Concertable.Customer.User.Infrastructure.Events;

internal sealed class UserCreationHandler : IIntegrationEventHandler<CredentialRegisteredEvent>
{
    private readonly UserDbContext context;

    public UserCreationHandler(UserDbContext context)
    {
        this.context = context;
    }

    public async Task HandleAsync(CredentialRegisteredEvent e, MessageEnvelope envelope, CancellationToken ct = default)
    {
        // Customer's own classification of which interactive clients are its own — Auth's InteractiveClient
        // is identity-only and carries no business-party opinion.
        if (InteractiveClients.Find(e.ClientId) is not { Client: InteractiveClient.CustomerBrowser or InteractiveClient.CustomerMobile })
            return;

        if (await context.IsInboxMessageProcessedAsync(envelope.MessageId, nameof(UserCreationHandler), ct))
            return;

        context.AddInboxMessage(envelope, nameof(UserCreationHandler));

        context.Users.Add(UserEntity.FromRegistration(e.UserId, e.Email));
        await context.SaveChangesAsync(ct);
    }
}
