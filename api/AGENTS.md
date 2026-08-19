# Concertable — backend (`api/`)

The .NET app. **No standard lives in this repo.** The generic .NET rules are load-on-demand skills
(`csharp-style`, `csharp-naming`, `comments`, `dependency-injection`, `logging`, `validation`,
`persistence`, `multitenancy`, `domain-events`, `keyed-strategies`, `module-structure`, `http-api`,
`microservice-boundaries`, `proto`, `seeding`, `result-carriers`, `result-errors`, `result-terminals`,
`unit-testing`, `integration-testing`, `e2e-scenarios`), and what is true of *this* system is their
`concertable-` counterparts (`concertable-persistence`, `concertable-seeding`,
`concertable-microservice-boundaries`, `concertable-packages`, `concertable-http-clients`,
`concertable-integration-testing`, and the rest). The task you are doing is the trigger to load the
matching pair; `.agents/skill-routes.json` maps path to skill, and the write-time hook enforces it.

Below is only the floor — the handful of rules whose violation is silent, expensive, and not worth
waiting for a skill invocation to catch.

## These are microservices — read [`ARCHITECTURE.md`](./ARCHITECTURE.md) before crossing a service boundary

The monorepo is a convenience only. Each service is independently owned and will split into its own repo
with its own developers. Design every change as if that split already happened: *would this still work if
this service lived alone?*

- **Adapter services — `Auth`, `Payment`.** Present in every host. A data service MAY call them
  synchronously and MAY `WaitFor` them at startup.
- **Data services — `B2B`, `Customer`, `Search`.** They must NEVER depend on each other's runtime. **B2B
  and Customer require Payment + Auth, but never each other.** A data service `WaitFor`-ing another data
  service is the bug to never introduce.

The roster, the surfaces each service exposes, and the simulator pattern that makes standalone hosts work
are the `concertable-microservice-boundaries` skill.

## STOP — a seeder may only write what production writes directly

**If production only creates this data as a *reaction* — an event, an outbox message, a handler, a
webhook — the seeder must drive that same trigger, not bypass it and write the row.** If the table is
empty at seed time and production never writes it directly, the fix is **always** to make the trigger
fire, never `context.X.AddRange(...)`.

Quick check before writing a seeder body: open the entity's repository, service or handler. If the only
production code calling `.Add`/`.AddRange` on this DbSet is inside a handler reacting to an event, your
seeder is not allowed to write it either.

**Which tables that means here is the `concertable-seeding` skill — read it in full before writing or
changing any `IDevSeeder` or `ITestSeeder`.** This mistake has cost real time, multiple times.

## Shared code is the intersection, never the union

`Concertable.Kernel` and `Concertable.Contracts` (and any `Concertable.Shared.*` lib) are consumed by
**every** service. Code there MUST be audience-agnostic. **Never put a B2B-only or Customer-only concept
onto a shared type** — not a property, not a method, not an enum case, not a claim accessor.

The litmus test: **if a member is only ever populated or meaningful for one audience — and is dead weight
for another — it does not belong in shared code.** That a shared *container* (e.g. `ICurrentUser`)
legitimately lives in `Kernel` does NOT license adding audience-specific *members* to it.

When a shared adapter needs an audience-specific value, either the **caller** resolves it and passes it in,
or it lives in a **separate abstraction only the services with the concept depend on** (e.g. an
`ITenantContext`, declared beside `ICurrentUser` so the shared save-interceptor can read it, but
implemented and depended on by B2B alone).

Anti-pattern, do not reintroduce: a tenant/owner key on the shared `ICurrentUser` — an `Owner` member and
`GetOwnerId()` extension once lived in `Kernel` for exactly this, and were removed. As resolved: Payment
reads the opaque `owner` claim at its own HTTP boundary
(`Concertable.Payment.Api.Identity.ICurrentPayoutOwner`, fail-closed). **Customer** mints `owner` and calls
those endpoints directly. **B2B no longer mints `owner`**: its `Tenant.Api/StripeAccountController` fronts
Payment's payout operations over gRPC, passing the active tenant id explicitly from B2B's request-scoped
`ITenantContext`, never the shared identity type.

## Logging is source-generated

Never call `logger.LogInformation/LogWarning/LogError` with an inline template — add a `[LoggerMessage]`
method to the project's `Log.cs`. `CA1848` is an error, so the inline form fails the build. Details are the
`logging` skill.

## Migrations are never additive, and never a cost to weigh

Run `./initial-migrations.ps1` from `api/` when the model changes. There is no production data, so the
re-scaffold is free — never raise "but this needs a migration" as an argument against a change. Full rule:
the `concertable-migrations` skill.
