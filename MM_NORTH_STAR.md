# Modular Monolith — North Star

The target architecture. Not the current state. Not the next PR. The shape the codebase is aimed at,
used to judge whether a given change moves us closer or further away.

Everything in this doc is aspirational until the relevant module extraction gets here. Per-module
plans (`IDENTITY_MODULE_REFACTOR.md`, `ARTIST_MODULE_REFACTOR.md`, etc.) describe how each step
moves toward this target. `IDENTITY_COMPLETION.md` records the known gaps.

---

## The Rule

> **Zero cross-module runtime queries.** Every module reads only from its own `DbContext`. Data
> needed from another module is either (a) duplicated locally as a read projection, kept current
> via domain events, or (b) requested via a command that the owning module executes.

**Search is the structural exception.** Search owns no write tables; its `SearchDbContext` is
a pure read layer consisting entirely of `ExcludeFromMigrations` mappings over the source
modules' tables. No events, no duplication — because there's no owned side to populate from.

Everything else is a corollary.

---

## Corollaries

### 1. No `IReadDbContext` on hot paths.
`IReadDbContext` exists as a transitional shim during extraction. In the north-star state it is
deleted. No Concert code reaches into Artist. No Payment code reaches into Identity. No
Notification code reaches into anything.

**Current legitimate use — Search.** Until Search gets its own `SearchDbContext` backed by a
module-owned projection (populated from `ArtistCreated` / `VenueCreated` / `ConcertScheduled` /
etc.), it needs `IReadDbContext` to query across modules for search indexing. That's the final
consumer. Once Search has its projection, `IReadDbContext` and the `Concertable.Data.Application`
project go away entirely.

### 2. No `.Include(x => x.OtherModuleEntity)`.
Navigation properties across module boundaries don't exist. FKs are plain primitives (`Guid UserId`,
`int ArtistId`). EF in each module's context is unaware that the target entity exists. The DB
enforces referential integrity; EF does not traverse it.

### 3. No cross-module service-to-service data fetching.
`IArtistModule.GetManagerAsync(userId)` in Identity — that kind of contract — is **allowed only for
administrative/UI-triggered flows**, not for module internal workflows. A Concert workflow that
needs artist data gets it from Concert's own projection, not from an Artist facade call.

### 4. Contracts carry commands, not queries.
Module `Contracts` projects expose:
- **Events** raised by the module (consumable by others via handlers).
- **Commands** that the module accepts ("pay this user this amount").

They do **not** expose "get me X about Y" query methods in the north-star state. Queries live
inside the module that owns the data, answered from its own store.

### 5. Each module owns its own read projections.

> **TL;DR.** Pure-consumer modules (Search — no writes, no workflows) use `ExcludeFromMigrations`
> mappings over source tables. Every other consumer (Concert, Payment, Notification, ...) owns
> a physical projection table populated by integration events. The deciding factor is whether
> the module has write paths that need a transactionally consistent local view.

If a consumer module needs `Email`, `StripeAccountId`, `Avatar`, `DisplayName`, `Name`, etc.
from another module, **the consumer owns its own table** carrying those fields, populated from
the source module's integration events via an in-module handler. Storage is cheap; boundaries
are not.

- **Owned physical table** in the consumer's schema — the consumer's DbContext creates +
  migrates it (NOT `ExcludeFromMigrations`). Concert's `ArtistReadModel` is a real owned
  table, not a view over Artist's table.
- **Populated by event handler**, never by direct cross-module writes. Source module raises
  `XCreated`/`XUpdated` integration events; consumer module's handler upserts its local
  projection table.
- **Idempotent upserts** — events can redeliver.
- **Restricted per consumer.** A projection carries only the fields the consuming module
  actually needs — never the full foreign aggregate. When the same source entity is needed by
  two different consumers with different concerns, each consumer gets its own projection table,
  shaped to its own view.

Canonical example — reviews:

- `ReviewEntity` lives in Concert. `ReviewSubmittedEvent` carries `ArtistId`, `VenueId`, `Stars`,
  `Comment`, `CreatedAt`, `ReviewerUserId`.
- **Artist** maintains `ArtistReviewProjection` (`ReviewId`, `ArtistId`, `Stars`, `Comment`,
  `CreatedAt`, `ReviewerUserId`) — **no `VenueId`**.
- **Venue** maintains `VenueReviewProjection` (`ReviewId`, `VenueId`, `Stars`, `Comment`,
  `CreatedAt`, `ReviewerUserId`) — **no `ArtistId`**.
- Each handler writes only the slice its module is allowed to see. Artist never learns which
  venue a review is attached to; Venue never learns which artist.

The same principle covers **aggregates-over-that-data**: `ArtistRatingProjection` and
`VenueRatingProjection` are two separate tables, each owned by the module whose display it
drives, each fed from the same `ReviewSubmittedEvent` by an in-module handler.

#### Search is the exception — `ExcludeFromMigrations`, not owned projections

Search owns **zero write tables**. `SearchDbContext` is entirely a collection of
`ExcludeFromMigrations` mappings over the source modules' own tables:

```csharp
// In Search.Domain — Search's restricted view of Artist's rows
public class ArtistSearchModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Avatar { get; set; }
    public Point? Location { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
}

// In SearchDbContext
modelBuilder.Entity<ArtistSearchModel>(e =>
{
    e.ToTable("Artists", t => t.ExcludeFromMigrations());
    e.HasKey(a => a.Id);
});
```

Because Search has no owned side, **nothing is duplicated** — its mappings are just restricted
C# views of rows that other modules own. No events, no handlers, no backfill. This shape works
for Search specifically because Search is a pure-consumer module: no writes, no workflow
ownership, nothing to keep consistent with its own transactional state.

Every other module (Concert, Identity, Payment, Notification, etc.) owns its own projection
tables populated by events — because they *do* have write paths that need a transactionally
consistent local view.

**Applies only to module-owned facts.** See §6 below for shared reference data — that's a
different category and does *not* get duplicated.
### 6. Shared reference data lives in one place and is read via FK.
Reference data that no module owns writes for — `GenreEntity`, and any future `CountryEntity` /
`CurrencyEntity` / similar static vocabulary — lives in a dedicated **`SharedDbContext`** owned by
nobody. Every module FKs into its tables directly. No duplication, no event-sync ceremony. The
`SharedDbContext` has **zero outbound FKs** (reference data is structurally terminal), so it
migrates first and every module cleanly FKs into it without circular-dependency problems.

**Taxonomy — three kinds of data, three different rules:**

| Data kind | Rule | Example |
|---|---|---|
| Module-internal | Stays in the owning module, never leaves. | `Artist.About`, `User.PasswordHash` |
| Module-owned, cross-cut | Other modules keep projected copies, synced via events (Corollary 5). | `User.Email` → Notification's copy, `Artist.Name` → Search's projection |
| Shared reference data | Dedicated `SharedDbContext`, every module FKs in, no duplication. | `Genres`, `Countries`, `Currencies` |

**Why no duplication for reference data:** it's shared vocabulary, not a fact owned by one module.
Genres have no ownership semantics — no module "decides" when genres change. 8 rows × N modules
of event-synced copies is ceremony without benefit, and risks drift between modules that should
be using identical vocabulary. A single FK-addressable source is the structurally correct shape.

**Why `ExcludeFromMigrations` stays in module contexts:** a module with an `ArtistGenreEntity.Genre`
nav pulls `GenreEntity` into its model (needed for `.Include()` projection), so EF would try to
manage the `Genres` schema from that context's migrations unless told otherwise.
`modelBuilder.Entity<GenreEntity>().ToTable("Genres", t => t.ExcludeFromMigrations())` is the
idiomatic EF Core way to say "I read this, someone else owns its DDL." Not a workaround —
*the* pattern.

**Why not use Grzybek's "Administration module" shape instead:** cosmetically similar (one owner,
everyone else reads), but wraps reference data in a module-with-Contracts shape preferred when the
data is write-complex (audited tax codes, user-editable catalogues with workflows). For
static, FK-addressable reference data like genres, dedicated-context is strictly better —
DB-level integrity, zero runtime lookup overhead, less ceremony.

---

## Event Infrastructure (already scaffolded)

`Concertable.Shared` already has `IDomainEvent`, `IEventRaiser`, `IDomainEventDispatcher`,
`EventRaiser`. `DbContextBase` dispatches events on `SaveChangesAsync`. What's not yet built:

- **Integration events** (cross-module) vs. domain events (intra-module). Today everything is a
  domain event. North-star: cross-module messages are integration events, delivered via an in-process
  bus, with a transactional outbox for durability.
- **Outbox pattern.** Events that cross module boundaries must be persisted in the same DB
  transaction as the entity change that raised them, then published by a background dispatcher.
  Otherwise a crash between "entity saved" and "event dispatched" silently breaks other modules'
  projections.
- **Idempotent handlers.** Every projection update handler must be idempotent — events can be
  redelivered.
- **Versioning.** Events are part of the cross-module contract and need the same care as any API.
  Additive changes only; breaking changes require event versioning.

---

## This Codebase, North-Star Shape

### Modules
- **Identity** — users, auth, roles. Source of truth for every user-scoped fact.
- **Artist** — artist profiles, genres, bios. Source of truth for artist identity.
- **Venue** — venue profiles, capacity, hireable-ness.
- **Concert** — opportunities, applications, bookings, concerts. The orchestration domain.
- **Payment** — Stripe accounts, payouts, ticket purchases, refunds.
- **Notification** — email + in-app messages, user contact preferences.
- **Search** — cross-module read-only module for search results. Already extracted. **Owns zero
  write tables.** `SearchDbContext` is purely `ExcludeFromMigrations` mappings
  (`ArtistSearchModel`, `VenueSearchModel`, `ConcertSearchModel`) over tables owned by Artist/
  Venue/Concert. Search has no migrations of its own and no event handlers — it reads source
  rows live. Canonical "pure consumer" module. `IReadDbContext` retires when `SearchDbContext`
  stands up. **Unlike every other module**, Search doesn't own projection tables populated by
  events — it has no owned side to maintain, so there's no need.

### Who owns what data
| Fact | Source of truth | Who else has a copy |
|---|---|---|
| `User.Id`, `Role`, `Password`, `RefreshToken` | Identity | Nobody |
| `User.Email`, `User.DisplayName`, `User.Avatar` | Identity | Artist, Venue, Notification (projections) |
| `User.StripeAccountId` (manager) / `StripeCustomerId` (customer) | Identity | Payment (projection) |
| `Artist.Name`, `Artist.About`, `Artist.Genres`, `Artist.Location` | Artist | Search (projection) |
| `Venue.Name`, `Venue.Capacity`, `Venue.Location` | Venue | Search (projection), Concert (booking snapshot) |
| `Concert.Id`, `Booking.Id`, `Application.Id` | Concert | Payment (booking snapshot for payout) |
| `Review.Stars`, `Review.Comment`, `Review.CreatedAt` | Concert | Artist (`ArtistReviewProjection`, ArtistId-scoped — no VenueId), Venue (`VenueReviewProjection`, VenueId-scoped — no ArtistId). Restricted per consumer, see §5. |
| `Artist.Rating`, `Artist.ReviewCount` (aggregate over Review) | Artist (`ArtistRatingProjection`, fed by Concert's `ReviewSubmittedEvent` — already implemented) | Search (future, via `ArtistSearchModel`) |
| `Venue.Rating`, `Venue.ReviewCount` (aggregate over Review) | Venue (`VenueRatingProjection`, fed by Concert's `ReviewSubmittedEvent` — symmetric to Artist) | Search (future, via `VenueSearchModel`) |
| `Payout.Status`, `Ticket.Status` | Payment | Concert (via events) |

### Cross-module events (illustrative, not exhaustive)

**From Identity:**
- `UserRegistered(UserId, Role, Email, DisplayName, Avatar)`
- `UserEmailChanged(UserId, NewEmail)`
- `UserAvatarChanged(UserId, NewAvatarUrl)`
- `UserDisplayNameChanged(UserId, NewDisplayName)`
- `ManagerStripeAccountLinked(UserId, StripeAccountId)`
- `ManagerStripeAccountVerified(UserId)`
- `CustomerStripeAccountLinked(UserId, StripeCustomerId)`

**From Artist:**
- `ArtistCreated(ArtistId, UserId, Name, Genres, Location)`
- `ArtistUpdated(ArtistId, Name, Genres, Location)`
- `ArtistAvatarChanged(ArtistId, AvatarUrl)` — if Artist maintains separate avatar from User

**From Venue:**
- `VenueCreated`, `VenueUpdated`, `VenueDeleted` — same shape.

**From Concert:**
- `OpportunityPosted(OpportunityId, VenueId, Date, Genres, Amount)`
- `ApplicationSubmitted(ApplicationId, OpportunityId, ArtistId)`
- `ApplicationAccepted(ApplicationId, ArtistUserId, VenueUserId, BookingId)`
- `ConcertScheduled(ConcertId, BookingId, Date)`
- `ConcertCompleted(ConcertId)` — payment trigger.
- `ReviewSubmittedEvent(ReviewId, ArtistId, VenueId, ConcertId, ReviewerUserId, Stars, Comment, CreatedAt)` — fan-out: Artist handler updates `ArtistRatingProjection` + `ArtistReviewProjection` (no `VenueId`); Venue handler updates `VenueRatingProjection` + `VenueReviewProjection` (no `ArtistId`). Each consumer projects only its own association — see §5.

**From Payment:**
- `PayoutRequested(PayoutId, BookingId, Recipients)`
- `PayoutSettled(PayoutId)` / `PayoutFailed(PayoutId, Reason)`
- `TicketPurchased(TicketId, ConcertId, CustomerId, Amount)`

### Cross-module commands (Contracts-surface)

**Payment.Contracts:**
- `RequestPayoutCommand(BookingId, Splits)` — published by Concert when a concert completes.
- `PurchaseTicketCommand(ConcertId, CustomerId, Quantity, PaymentMethodId)` — published by Web via Concert.
- `RefundTicketCommand(TicketId)`.

**Notification.Contracts:**
- No commands. Pure event consumer. Reacts to `ApplicationAccepted`, `ConcertScheduled`,
  `PayoutSettled`, etc. with appropriate emails + in-app messages.

---

## Hot-Path Walkthroughs (North-Star)

### Artist creates a profile
1. Web → `Artist.IArtistService.CreateAsync(request)` — stays in-process, single module call.
2. Artist writes `ArtistEntity` to `ArtistDbContext`. Raises `ArtistCreated(artistId, userId, ...)`.
3. `SaveChangesAsync` commits + dispatches event.
4. Search handler updates search projection. Notification handler... does nothing (no notification on
   artist create).
5. Zero cross-module reads.

### Concert application gets accepted → payout eventually
1. Venue manager accepts an application via Concert. Concert writes `BookingEntity` to its own context.
   Booking snapshot includes `ArtistUserId`, `VenueUserId`, `OpportunityGenres`, `BookingAmount` —
   all already known to Concert (either stored locally or carried on the application). Raises
   `ApplicationAccepted(...)`.
2. Notification handler reacts: reads its own `UserContactProjection` by userId → sends email, writes
   in-app message. No Identity call.
3. Concert happens (external time passage / manual status change). Concert raises `ConcertCompleted`.
4. Concert publishes `RequestPayoutCommand(bookingId, splits)` to Payment. `splits` contains
   `(UserId, Amount)` pairs — Concert knows the userIds from its booking snapshot.
5. Payment handler reads its own `PayoutAccount` projection by userId → gets `StripeAccountId` →
   calls Stripe → writes `PayoutEntity` → raises `PayoutSettled`.
6. Concert reacts to `PayoutSettled`, marks booking paid. Notification reacts, emails both parties.

**On the hot path** (each handler): one query, against the module's own context. Zero cross-module
round-trips. Zero `.Include` chains.

### User updates Stripe account
1. Web → `Identity.IStripeService.LinkAccountAsync(stripeAccountId)`.
2. Identity writes to `IdentityDbContext`. Raises `ManagerStripeAccountLinked(userId, stripeAccountId)`.
3. Payment handler receives event → upserts its `PayoutAccount` projection.
4. Next payout for this user uses the new account. No runtime Identity call.

### Artist renames themselves
1. Artist writes `ArtistEntity` change. Raises `ArtistUpdated(artistId, newName, ...)`.
2. Search handler updates its projection. Concert (if it cached artist name on bookings for display)
   handles the event too. Most of the time Concert has no opinion on the artist name and the event
   is a no-op for it.

---

## What This Buys You

1. **Runtime performance.** Every workflow runs inside one module's context. One query, local data.
   No module boundary crossings on the hot path.
2. **Clean failure modes.** A module being temporarily unhealthy (bug, slow query, deadlock) doesn't
   cascade. Notification is broken? Events queue, handlers retry, other modules don't care.
3. **Truly independent extraction.** Moving a module to its own service is a deployment change, not
   a code change. No query rewrites. The event bus becomes the network; the projections and
   commands don't change.
4. **Testability.** Each module tests against its own DbContext with faked events in/out. No shared
   test fixture, no seeded user hierarchy to make an artist test run.
5. **Boundaries enforced by the compiler.** Modules literally cannot reach into each other's data
   because the types aren't in scope. Reviewers don't need to catch violations — the code won't
   build.

## What This Costs You

1. **Storage.** Every projection duplicates fields. Trivial at this codebase's scale.
2. **Eventual consistency.** Projections lag reality by milliseconds. When it matters (payment —
   you must use the Stripe account as of *now*, not as of 200ms ago): design the workflow to tolerate
   it (idempotent retries) or accept a targeted synchronous call at the exact boundary where
   consistency matters, documented as a deliberate exception.
3. **Event infrastructure work.** Outbox table, dispatcher, idempotency keys, event versioning. One-time
   cost, then it's plumbing.
4. **Onboarding complexity.** "Where does this field come from?" now has a longer answer. Mitigated
   by keeping projection update handlers colocated with their table and naming them consistently
   (`OnUserEmailChanged → UserContactProjection`).
5. **More code per feature.** A new cross-cutting field = a new event + new projection column + new
   handler in each consumer. Versus "add a property and join." This is the real tax.

---

## Explicit Non-Goals

- **Full CQRS / ES.** Projections, not event sourcing. The write side is boring EF. The read side
  is projections maintained by event handlers. No rebuilding from event logs, no event store.
- **Distributed messaging.** The bus is in-process. Nothing runs outside this deployable yet.
- **Async command handling.** Commands are in-process, synchronous, transactional. Events are in-process,
  can be synchronous (same transaction) or via outbox (next transaction) depending on criticality.
- **Pure actor-style isolation.** Modules share a DB, share a process, share a transaction when
  it's a single-module operation. The boundary is conceptual + compile-time, not runtime-isolated.
- **Zero pragmatic exceptions.** If a single targeted cross-module synchronous query makes a flow
  dramatically simpler and doesn't recur, document it as an exception and move on. The rule exists
  to prevent drift, not to enable zealotry.

---

## Bridge from Current State (80% version) to North Star

This is the multi-year target. Today is mid-extraction. Rough order:

1. **Finish current extractions (Artist → Venue → Concert → Payment).** Each module gets its own
   `DbContext`, Contracts, Application, Infrastructure. Cross-module reads via `IReadDbContext` or
   direct Contracts facade calls — the 80% version. This is what the per-module plans describe.

2. **Introduce integration events + outbox.** Before Payment extraction, because Payment is the
   first module where eventual consistency matters enough to justify the infrastructure. Formalize
   the distinction between domain events (intra-module) and integration events (cross-module).

3. **Stand up per-module projections, one hot path at a time.** Each consumer module that
   needs foreign data owns its own projection table, populated from integration events.
   - Concert: `ArtistReadModel` + `VenueReadModel` — owned tables on `ConcertDbContext`,
     populated by `ArtistCreated`/`ArtistUpdated` + venue equivalents from Artist/Venue
     contracts.
   - Payment: `PayoutAccount` projection (replaces the deprecated
     `ArtistManagerRepository.GetByConcertIdAsync` hack).
   - Notification: extract it, give it `UserContactProjection`.
   - Artist/Venue rating projections — already landed.

4. **Retire `IReadDbContext`.** Each non-Search use site gets replaced with an owned projection
   read or a command dispatch. Search is the last consumer — once `SearchDbContext` stands up
   with its `ExcludeFromMigrations` mappings over Artist/Venue/Concert tables, `IReadDbContext`
   and the `Concertable.Data.Application` project are deleted.

5. **Delete nav properties pointing at foreign-module types.** FKs between *modules* become plain
   primitives (`Guid UserId`, `int ArtistId`) wherever a consumer doesn't need join-shape reads.
   Where a consumer *does* need to JOIN (Concert needs `OpportunityApplication.Artist.Name` for
   DTOs), the nav is retyped against the consumer's own owned projection class (e.g.
   `OpportunityApplicationEntity.Artist : ArtistReadModel` — a Concert.Domain type mapped to
   Concert's own `ArtistReadModels` table, not `Artist.Domain.ArtistEntity`). EF configs in each
   module's context stop configuring relationships to other modules' Domain types.
   Concert.Domain / Search.Domain never reference Artist.Domain / Venue.Domain. FKs into **shared reference data**
   (`GenreEntity` etc. — see §6) stay as real EF relationships, pointing at `SharedDbContext`.
   At this point `ApplicationDbContext` has shrunk to just reference-data tables; rename it
   `SharedDbContext` (same process, same DB, semantically accurate). The Artist/Identity/etc.
   `ExcludeFromMigrations` lines in `ApplicationDbContext` retire here because the navs that
   pulled those entities into its model are gone. The per-module `ExcludeFromMigrations` for
   `GenreEntity` stays — that's the §6 idiom.

   This step also retires the cross-context FK hand-edit documented in
   `feedback_cross_context_fk.md`: `SharedDbContext` has zero outbound FKs, so it migrates
   first and every module's FK into `Genres` works naturally — no circular dependency.

6. **Retire query-shaped facades.** `IArtistModule.GetSummaryAsync` etc. either go away (callers
   read via their owned projection) or remain as narrow, documented synchronous-consistency
   exceptions.

At each step: previous work isn't undone, it's refined. The 80% version is a valid stopping point
if priorities change. The north star is worth climbing toward only as long as the team values the
boundary.

---

## Decision Test

When adding new code: does this change add a cross-module read, nav, or query-shaped facade call?

- **Yes, and there's no projection for this data in the caller's module:** pause. Add the event +
  owned projection (§5), or document an explicit exception. Don't silently add the leak.
- **Yes, but a projection exists:** you're reading from your own context. Fine.
- **Yes, and you're Search:** use `ExcludeFromMigrations` mapping on `SearchDbContext` over the
  source table. Search is the structural exception.
- **Yes, via `IReadDbContext`:** transitional only. Allowed while Search hasn't fully migrated.
  Everywhere else, prefer owned projections.
- **No, it's a command:** fine.
- **No, it's an intra-module read:** fine.

---

## Reference
- Kamil Grzybek — `modular-monolith-with-ddd` (the canonical MM reference implementation).
- Vaughn Vernon — "Implementing Domain-Driven Design" (context maps, integration events).
- Pat Helland — "Life Beyond Distributed Transactions" (why duplication + eventual consistency
  beats distributed coordination).
- Oskar Dudycz — writings on the outbox pattern and module integration.
