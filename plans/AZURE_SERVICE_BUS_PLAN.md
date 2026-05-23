# Azure Service Bus Transport — Parallel Agent Spec

Self-contained spec for a parallel agent to build `Concertable.Messaging.AzureServiceBus` — the production-broker `IBusTransport` adapter. Designed to be worked on in **isolation** without colliding with concurrent work on `Concertable.DataAccess` extraction.

## Context (read first)

The repo is migrating from a modular monolith to microservices. `Concertable.Messaging` (at `api/Concertable.Messaging/`) was extracted as a shared lib at commit `517201db` on branch `Refactor/Microservices`. It defines:

- **`IBus`** (publisher API) — `PublishAsync<TEvent>` for events, `SendAsync<TCommand>` for commands.
- **`IBusTransport`** (delivery seam) — broker-agnostic. Same two methods plus a `MessageEnvelope` carrying `MessageId`, `MessageType`, `OccurredAtUtc`, `CorrelationId`.
- **`InMemoryBusTransport`** — Step 8 default; per-service in-proc DI fanout.
- **`IIntegrationEvent`** / **`IIntegrationCommand`** marker interfaces.
- **`IIntegrationEventHandler<T>`** (events: 0..N handlers, fan-out) / **`IIntegrationCommandHandler<T>`** (commands: exactly one handler).

Files to read before starting:
- `api/Concertable.Messaging/Concertable.Messaging.Contracts/IBus.cs`
- `api/Concertable.Messaging/Concertable.Messaging.Contracts/IBusTransport.cs`
- `api/Concertable.Messaging/Concertable.Messaging.Contracts/MessageEnvelope.cs`
- `api/Concertable.Messaging/Concertable.Messaging.Contracts/IIntegrationEvent.cs` / `IIntegrationCommand.cs` / handlers
- `api/Concertable.Messaging/Concertable.Messaging.Infrastructure/InMemoryBusTransport.cs` (reference impl — same shape, swap broker)
- `MICROSERVICE_STEPS.md` Phase 4 Step 14 ("Switch transport to Azure Service Bus") — the *what*
- `MODULAR_MONOLITH_RULES.md` — Clean Architecture layering convention applied here

## Architectural decisions already locked

1. **ASB SDK + our own abstraction.** Not MassTransit. Reasons: MassTransit v9 went partially commercial; this is a learning project; abstraction shape is broker-agnostic by design (already proven with in-memory transport).
2. **Bus must be transport-agnostic from day one.** The new lib implements `IBusTransport` from `Concertable.Messaging.Contracts`. Don't introduce new abstractions in `Concertable.Messaging.Contracts` — that interface is locked.
3. **Topics for events, queues for commands.**
   - `IBusTransport.PublishAsync<TEvent>` → ASB topic, one subscription per consuming service.
   - `IBusTransport.SendAsync<TCommand>` → ASB queue, exactly one consumer.
4. **One ASB namespace, many topics + queues.** Naming: topic per event type (`event.concertchangedevent`), queue per command type (`command.refundticketcommand`). Lowercased full type name with prefix to disambiguate.
5. **Envelope carried via ASB `ApplicationProperties`** — `MessageId`, `MessageType` (the `T` full name), `OccurredAtUtc`, `CorrelationId`. Body is the serialized event/command (System.Text.Json).
6. **Receiver-side dispatch via DI handler resolution** — same pattern as `InMemoryBusTransport`: read the `MessageType` header, resolve type via a registry, deserialize, fetch `IIntegrationEventHandler<T>` instances from DI, invoke.

## Layout

```
api/Concertable.Messaging/
  Concertable.Messaging.AzureServiceBus/
    Concertable.Messaging.AzureServiceBus.csproj
    AzureServiceBusTransport.cs              ← IBusTransport impl (sender side)
    AzureServiceBusReceiver.cs               ← IHostedService (receiver side)
    MessageTypeRegistry.cs                   ← Type T ↔ string mapping
    MessageSerializer.cs                     ← System.Text.Json wrapper
    Options/
      AzureServiceBusOptions.cs              ← Connection string, topic/queue naming policy, service name
    Extensions/
      ServiceCollectionExtensions.cs         ← AddAzureServiceBusTransport(...)
      EventRegistrationExtensions.cs         ← Fluent API for registering subscribed event/command types
```

Single csproj. Lives **inside** `api/Concertable.Messaging/` (sibling to `.Contracts/.Domain/.Application/.Infrastructure/`) because it's logically a transport adapter for the Messaging family.

Namespace: `Concertable.Messaging.AzureServiceBus`.

## csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Azure.Messaging.ServiceBus" Version="7.18.2" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.3" />
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" Version="10.0.3" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.3" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="10.0.3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Concertable.Messaging.Contracts\Concertable.Messaging.Contracts.csproj" />
  </ItemGroup>
</Project>
```

(Use the latest Azure.Messaging.ServiceBus stable version — `7.18.2` shown is illustrative; check NuGet.)

## Sender side — `AzureServiceBusTransport`

```csharp
internal sealed class AzureServiceBusTransport : IBusTransport, IAsyncDisposable
{
    private readonly ServiceBusClient client;
    private readonly AzureServiceBusOptions options;
    private readonly MessageSerializer serializer;
    private readonly ConcurrentDictionary<string, ServiceBusSender> senders = new();

    public AzureServiceBusTransport(ServiceBusClient client, IOptions<AzureServiceBusOptions> options, MessageSerializer serializer)
    {
        this.client = client;
        this.options = options.Value;
        this.serializer = serializer;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, MessageEnvelope envelope, CancellationToken ct = default)
        where TEvent : IIntegrationEvent
    {
        var topic = options.TopicNameFor(typeof(TEvent));
        var sender = senders.GetOrAdd(topic, name => client.CreateSender(name));
        await sender.SendMessageAsync(BuildMessage(@event, envelope), ct);
    }

    public async Task SendAsync<TCommand>(TCommand command, MessageEnvelope envelope, CancellationToken ct = default)
        where TCommand : IIntegrationCommand
    {
        var queue = options.QueueNameFor(typeof(TCommand));
        var sender = senders.GetOrAdd(queue, name => client.CreateSender(name));
        await sender.SendMessageAsync(BuildMessage(command, envelope), ct);
    }

    private ServiceBusMessage BuildMessage<T>(T payload, MessageEnvelope envelope)
    {
        var msg = new ServiceBusMessage(serializer.Serialize(payload))
        {
            MessageId = envelope.MessageId.ToString(),
            ContentType = "application/json",
        };
        msg.ApplicationProperties["MessageType"] = envelope.MessageType;
        msg.ApplicationProperties["OccurredAtUtc"] = envelope.OccurredAtUtc.ToString("O");
        if (envelope.CorrelationId is not null)
            msg.CorrelationId = envelope.CorrelationId;
        return msg;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in senders.Values)
            await sender.DisposeAsync();
        await client.DisposeAsync();
    }
}
```

## Receiver side — `AzureServiceBusReceiver`

`IHostedService` (background) that:
1. On start: for each registered event type (`MessageTypeRegistry`), creates a `ServiceBusProcessor` against `<topic>/<service-subscription>` and starts processing.
2. For each registered command type, creates a `ServiceBusProcessor` against `<queue>` and starts.
3. On each incoming message: reads `MessageType` from `ApplicationProperties`, resolves CLR type via registry, deserializes JSON, resolves handler(s) from DI scope, invokes.
4. Idempotency / inbox-state is **out of scope** for this lib — that's a separate Step 10 concern. Document it.

Sketch:

```csharp
internal sealed class AzureServiceBusReceiver : BackgroundService
{
    private readonly ServiceBusClient client;
    private readonly AzureServiceBusOptions options;
    private readonly MessageTypeRegistry registry;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly MessageSerializer serializer;
    private readonly ILogger<AzureServiceBusReceiver> logger;
    private readonly List<ServiceBusProcessor> processors = new();

    // constructor + ExecuteAsync that wires processors

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var eventType in registry.RegisteredEventTypes)
        {
            var topic = options.TopicNameFor(eventType);
            var processor = client.CreateProcessor(topic, options.ServiceName);  // subscription name = service name
            processor.ProcessMessageAsync += args => HandleEventAsync(args, eventType);
            processor.ProcessErrorAsync += args => HandleErrorAsync(args);
            processors.Add(processor);
            await processor.StartProcessingAsync(stoppingToken);
        }

        foreach (var commandType in registry.RegisteredCommandTypes)
        {
            var queue = options.QueueNameFor(commandType);
            var processor = client.CreateProcessor(queue);
            processor.ProcessMessageAsync += args => HandleCommandAsync(args, commandType);
            processor.ProcessErrorAsync += args => HandleErrorAsync(args);
            processors.Add(processor);
            await processor.StartProcessingAsync(stoppingToken);
        }

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (TaskCanceledException) { }

        foreach (var p in processors) await p.DisposeAsync();
    }

    private async Task HandleEventAsync(ProcessMessageEventArgs args, Type eventType)
    {
        try
        {
            var @event = serializer.Deserialize(args.Message.Body, eventType);
            using var scope = scopeFactory.CreateScope();
            var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
            foreach (var handler in scope.ServiceProvider.GetServices(handlerType))
            {
                var method = handlerType.GetMethod(nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync))!;
                await (Task)method.Invoke(handler, [@event, args.CancellationToken])!;
            }
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed processing event {MessageType}", args.Message.ApplicationProperties.GetValueOrDefault("MessageType"));
            await args.AbandonMessageAsync(args.Message);
            // ASB built-in retry takes over; dead-letter handling via ASB queue config
        }
    }

    // HandleCommandAsync — same shape but single handler (throw if zero or multiple)
}
```

## Type registry — `MessageTypeRegistry`

```csharp
public sealed class MessageTypeRegistry
{
    private readonly Dictionary<string, Type> events = new();
    private readonly Dictionary<string, Type> commands = new();

    public IEnumerable<Type> RegisteredEventTypes => events.Values;
    public IEnumerable<Type> RegisteredCommandTypes => commands.Values;

    public Type ResolveEvent(string messageType) => events[messageType];
    public Type ResolveCommand(string messageType) => commands[messageType];

    public void RegisterEvent<TEvent>() where TEvent : IIntegrationEvent =>
        events[typeof(TEvent).FullName!] = typeof(TEvent);

    public void RegisterCommand<TCommand>() where TCommand : IIntegrationCommand =>
        commands[typeof(TCommand).FullName!] = typeof(TCommand);
}
```

## Options

```csharp
public sealed class AzureServiceBusOptions
{
    public required string ConnectionString { get; init; }
    public required string ServiceName { get; init; }  // used as subscription name
    public string EventTopicPrefix { get; init; } = "event.";
    public string CommandQueuePrefix { get; init; } = "command.";

    public string TopicNameFor(Type eventType) =>
        EventTopicPrefix + eventType.Name.ToLowerInvariant();

    public string QueueNameFor(Type commandType) =>
        CommandQueuePrefix + commandType.Name.ToLowerInvariant();
}
```

## DI extension

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureServiceBusTransport(
        this IServiceCollection services,
        Action<AzureServiceBusOptions> configure,
        Action<MessageTypeRegistry> register)
    {
        services.Configure(configure);
        var registry = new MessageTypeRegistry();
        register(registry);
        services.AddSingleton(registry);

        services.AddSingleton<ServiceBusClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<AzureServiceBusOptions>>().Value;
            return new ServiceBusClient(opts.ConnectionString);
        });
        services.AddSingleton<MessageSerializer>();
        services.AddSingleton<IBusTransport, AzureServiceBusTransport>();
        services.AddHostedService<AzureServiceBusReceiver>();

        return services;
    }
}
```

Caller usage (illustrative — DO NOT wire into any composition root as part of this agent's work):

```csharp
services.AddAzureServiceBusTransport(
    configure: opts =>
    {
        opts.ConnectionString = configuration.GetConnectionString("ServiceBus")!;
        opts.ServiceName = "b2b";  // or "customer", etc.
    },
    register: registry =>
    {
        registry.RegisterEvent<ConcertChangedEvent>();
        registry.RegisterEvent<ReviewSubmittedEvent>();
        registry.RegisterCommand<RefundTicketCommand>();
    });
```

## Scope boundaries — what NOT to touch

This agent's work is **strictly additive**. Do not:

- Touch any composition root (`Concertable.Web/Program.cs`, `Concertable.Workers/ServiceCollectionExtensions.cs`, `Concertable.Customer/Concertable.Customer.Web/Program.cs`). They keep `InMemoryBusTransport` for now. The ASB lib just sits in the tree, ready to be wired at Phase 4 Step 14 (production cutover).
- Touch `Concertable.Messaging.Contracts` — `IBus`/`IBusTransport`/`MessageEnvelope`/`IIntegrationEvent`/`IIntegrationCommand` are locked.
- Touch `Concertable.Messaging.Infrastructure` (the in-memory transport stays as-is).
- Touch any file outside `api/Concertable.Messaging/Concertable.Messaging.AzureServiceBus/`, **with one exception**: adding the new csproj to `Concertable.sln` via `dotnet sln add`. If a merge conflict on sln happens, that's fine — easy to resolve.
- Add the lib to any other csproj's ProjectReferences.
- Introduce outbox/inbox/idempotency concerns — those are Steps 9 and 10. This agent does pure transport.

## What to deliver

1. The 8 files above (csproj + 7 source files) under `api/Concertable.Messaging/Concertable.Messaging.AzureServiceBus/`.
2. `dotnet sln add` the new csproj.
3. `dotnet build api/Concertable.sln` — must compile clean.
4. A unit test project at `api/Concertable.Messaging/Tests/Concertable.Messaging.AzureServiceBus.UnitTests/` exercising:
   - `MessageTypeRegistry` registration + resolution
   - `AzureServiceBusOptions` topic/queue naming
   - `MessageSerializer` round-trip on a fake `IIntegrationEvent` record
   - `AzureServiceBusTransport.PublishAsync` builds the right `ServiceBusMessage` (mock `ServiceBusSender` if feasible — Azure.Messaging.ServiceBus 7.x makes this awkward; use `ServiceBusModelFactory` where available, or skip if too painful)
5. One commit on its own branch (e.g. `Feature/AzureServiceBusTransport`) ready for review. Commit message follows repo style — no Co-Authored-By trailer.

## Open items the agent decides

- **Receive concurrency / prefetch / max-auto-renew-lock-duration** — pick sensible defaults; expose via `AzureServiceBusOptions` if useful.
- **Dead-letter handling** — rely on ASB's built-in DLQ after max-delivery-count; document.
- **Schema / topic provisioning** — assume topics/queues already exist (operator-provisioned). Don't try to create them at startup. Document this as a precondition.
- **Sessions / ordering** — out of scope. Topics + queues are unordered. If ordering becomes a concern later, separate piece of work.

## Sign-off criteria

- Build green on `Concertable.sln`.
- New csproj in sln.
- Unit tests pass (or cleanly documented as "skipped due to ServiceBusSender mocking limitations").
- No edits outside the agent's defined scope.
- Branch pushed (or worktree merged) for review.
