# Manager Front Page Plan

## Progress (2026-05-18)

**Phase A: ~80% done.** See [MANAGER_FRONT_PAGE_FEEDBACK.md](./MANAGER_FRONT_PAGE_FEEDBACK.md) for session decisions that supersede this plan in conflict.

| Phase | Step | Status |
|---|---|---|
| A.1 | TS contracts (split: shared common DTOs + per-SPA overview/kpis) | ✅ |
| A.2 | Fixtures × 6 (empty/mid/thriving per persona), pinned dayjs anchor | ✅ |
| A.3 | `dashboardApi` per SPA (no interface, mock-bodied for now) | ✅ |
| A.4 | Shared visual primitives (DashboardCard, KpiTile, ChartTooltip, MonthlyRevenueChart, ActivityFeed, SectionGrid, StripeConnectBanner, ProfileHealthCard, WidgetState, PersonaSwitcher) | ✅ |
| A.5 | Venue widgets (11) + page + route | ⚠️ Wired against shared hooks; need to flip to local `./hooks/` |
| A.6 | Artist widgets (10) + page; layout diverged from venue (NextConcertHero, ApplicationsPipeline, no Settlements/Open opps) | ⚠️ Files exist; needs `dashboardApi.ts` + `hooks/` folder + import fixes |
| A.7 | Stripe + Profile-Health banners | ✅ Done via overview widgets |
| A.8 | UX freeze, persona switch verification, builds green | ⏳ Pending |

**Open Phase A todo:** see [FEEDBACK.md → Open Phase A todo](./MANAGER_FRONT_PAGE_FEEDBACK.md#open-phase-a-todo-pick-up-here-post-compact).

**Key divergences from original plan** (codified in FEEDBACK.md):

- `Header` → `Overview` everywhere (DTO, endpoint, hook).
- `gig` → `concert` everywhere.
- Persona-specific dashboard code lives in the SPA (`app/web/{venue,artist}/src/features/dashboard/`), not in `app/web/shared/`.
- `dashboardApi` per SPA, `venueApi`-style flat const object — no interface/dispatcher/mock-vs-real split.
- No `take` / `monthsBack` query params on FE hooks (server decides).
- `ProfileHealth` is a single `items: { id, label, href, done }[]` list.
- Artist layout diverges (NextConcertHero, ApplicationsPipeline grouped by status, full-width RecommendedOpportunities, no Settlements).

## Context

The artist and venue web SPAs (`app/web/artist/`, `app/web/venue/`) currently
render placeholder front pages — `<div>Artist Dashboard</div>` /
`<div>Venue Dashboard</div>` — at `/_artist/` and `/_venue/`. This is the
first thing a logged-in **manager** sees after signing in.

Managers run an operation: they post opportunities, review applications, take
payments, ship concerts, get paid, and watch their reputation. A single-strip
placeholder doesn't reflect any of that. This plan replaces both front pages
with a **dense manager workbench dashboard** that shows what needs attention,
what's in flight, money, reputation, and discovery — in one glance.

Scope: **one PR** delivering both personas. Implementation is shared between
artist and venue where the widget is symmetric, persona-specific where it
diverges.

## Goal & shape

**Shape:** Classic dashboard (multi-widget grid) with a triage strip at the
top — the "S1 + S12 hybrid" from the brainstorm. Static layout v1, no
drag-to-rearrange.

**Why:** Managers are operators. They need to *act* (top of page) and *be
aware* (rest of page). The dashboard pattern is the boring-but-correct
choice for fixed product widgets shaped by engineers.

**Out of scope** (deferred to follow-ups):

- SignalR push for real-time updates (polling-only in v1)
- Drag-to-rearrange widgets / user-customised layouts
- Custom date ranges per widget (fixed MTD / next-30-days / last-90-days)
- Kanban pipeline view (S3) and calendar-first view (S4)
- BI-style user-authored chart specs

## Architecture overview

| Concern | Approach |
|---|---|
| **Chart data** | Backend ships pre-aggregated typed DTOs via new methods on existing `IXModule` facades. Per [CLAUDE.md](../CLAUDE.md) — `Dto` types in `Module.Application/DTOs/` (or `Module.Contracts/` if cross-module). Controllers return Dto verbatim. |
| **Chart rendering** | **Recharts** in the frontend. Spec lives in the widget component as JSX, not in the DB. (See "Why frontend-spec" below.) |
| **Query composition** | **Specification pattern** — same as `ConcertHeaderRepository` ([prior art](../api/Modules/Search/Concertable.Search.Infrastructure/Repositories/ConcertHeaderRepository.cs) / [spec example](../api/Modules/Search/Concertable.Search.Infrastructure/Specifications/ConcertSearchSpecification.cs)). One `IQueryable` is composed from `IXSpecification.Apply(...)` calls and materialised once. No N+1 sub-queries. |
| **Round-trip granularity** | **One HTTP round trip per visual section.** KPI strip = 1 call (4 numbers in one response). Each chart = 1 call (multi-series in one aggregate query). Each list = 1 call. See ["Round-trip plan"](#round-trip-plan) below. |
| **Data tables** | shadcn Data Table (TanStack Table under the hood — already in the stack). |
| **Layout** | Tailwind CSS Grid (`grid-cols-12` with widget `col-span-*`). Static. |
| **Real-time** | TanStack Query polling on the active tab — `refetchInterval: 30s` for action sections, `60s` for charts/lists, `static` for header. SignalR push added in a follow-up. |
| **Empty / loading / error states** | Each widget owns its own. Shared skeleton primitives in `app/web/shared/src/components/skeletons/`. |
| **Auth** | Existing role-based route guards (`_artist`, `_venue`) already gate the pages. Widget visibility may further branch on profile/setup state. |

### Why frontend-spec for charts

Where the chart spec lives is **orthogonal** to the chart library. Recharts,
Vega-Lite, and Tremor can all be authored either side. The convention for
**fixed product dashboards** (Stripe, Linear, GitHub, Vercel) is
frontend-spec because the engineer designs the chart and ships it with the
app. The only thing that flips this is **user-authored / customisable
dashboards** (Metabase, Superset, Grafana) where the spec *is* user-generated
content — explicitly out of scope here.

Backend ships the *numbers*, frontend draws the *picture*.

## Backend query strategy

Two principles, both modelled on the existing Search repositories:

### 1. One IQueryable per call, composed via specifications

Inside each repository method, build a single `IQueryable<T>` by chaining
specification `Apply(...)` calls — exactly like
`ConcertHeaderRepository.SearchAsync` chains
`searchSpecification → geometrySpecification → sortSpecification` before a
single `ToListAsync()`. The query materialises **once** and gives one SQL
round trip per call. No issuing 3 sub-queries inside one repository method.

For dashboard charts specifically: if a chart needs gross + net + count per
month, that's **one** `.GroupBy(month).Select(g => new
MonthlyRevenuePointDto { Gross = g.Sum(...), Net = g.Sum(...), Count =
g.Count() })` — EF compiles it to one `SELECT ... GROUP BY month` with
multiple aggregate columns. Not three separate queries.

**Two new reusable generic specifications** (used by both charts and
list/count widgets — see "Where they're used" below). They mirror the
existing `IGeometrySpecification<T>` shape: marker-interface constraint +
single open-generic implementation registered once in DI.

Both specifications key off the existing
[`DateRange`](../api/Shared/Concertable.Shared.Domain/DateRange.cs) value
object via **one** marker interface — no `IHasEndDate` / `IHasOccurredAt`
split.

#### Prerequisite — align ConcertEntity to use `DateRange`

`OpportunityEntity` already owns `DateRange Period` (mapped as an owned
type with columns `StartDate`/`EndDate`). `ConcertEntity` currently has
separate `DateTime StartDate` / `DateTime EndDate` fields — a DDD
inconsistency given both entities semantically span a range.

**Refactor `ConcertEntity` to own `DateRange Period`** exactly the same
way `OpportunityEntity` does:

- Replace `StartDate`/`EndDate` properties with `DateRange Period`
- In `ConcertEntityConfiguration`: `builder.OwnsOne(c => c.Period, p => { p.Property(x => x.Start).HasColumnName("StartDate"); p.Property(x => x.End).HasColumnName("EndDate"); });`
- Update factory methods, services, projections, and any consumers that read `c.StartDate` / `c.EndDate` to read `c.Period.Start` / `c.Period.End`
- Re-scaffold `InitialCreate` via `./initial-migrations.ps1` per [CLAUDE.md](../CLAUDE.md)

After this, both range-entities share the same marker, and the generic
specifications work uniformly.

#### Single marker — `IHasDateRange`

```csharp
// Concertable.Shared.Domain/Entities/IHasDateRange.cs
public interface IHasDateRange
{
    DateRange Period { get; }
}
```

`OpportunityEntity : IHasDateRange` and `ConcertEntity : IHasDateRange` —
both satisfied via their owned `Period` property. EF translates
`e.Period.Start` / `e.Period.End` to the underlying columns.

#### `IUpcomingSpecification<T>` — period has not yet ended

```csharp
// Concertable.Shared.Application/Specifications/IUpcomingSpecification.cs
public interface IUpcomingSpecification<TEntity> where TEntity : class, IHasDateRange
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query);
}

// Concertable.Shared.Infrastructure/Specifications/UpcomingSpecification.cs
internal class UpcomingSpecification<TEntity> : IUpcomingSpecification<TEntity>
    where TEntity : class, IHasDateRange
{
    private readonly TimeProvider timeProvider;

    public UpcomingSpecification(TimeProvider timeProvider)
        => this.timeProvider = timeProvider;

    public IQueryable<TEntity> Apply(IQueryable<TEntity> query)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return query.Where(e => e.Period.End > now);
    }
}
```

#### `IDateRangeSpecification<T>` — range-overlap with a query `DateRange`

```csharp
// Concertable.Shared.Application/Specifications/IDateRangeSpecification.cs
public interface IDateRangeSpecification<TEntity> where TEntity : class, IHasDateRange
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, DateRange range);
}

// Concertable.Shared.Infrastructure/Specifications/DateRangeSpecification.cs
internal class DateRangeSpecification<TEntity> : IDateRangeSpecification<TEntity>
    where TEntity : class, IHasDateRange
{
    public IQueryable<TEntity> Apply(IQueryable<TEntity> query, DateRange range)
        => query.Where(e => e.Period.Start < range.End && e.Period.End > range.Start);
}
```

Both use `DateRange` as the entity's property *and* (for
`IDateRangeSpecification`) as the query parameter. No raw `DateTime from,
DateTime to` pairs floating around.

#### DI — one open-generic line per specification

```csharp
services.AddScoped(typeof(IUpcomingSpecification<>), typeof(UpcomingSpecification<>));
services.AddScoped(typeof(IDateRangeSpecification<>), typeof(DateRangeSpecification<>));
```

Covers every closed-generic resolution
(`IUpcomingSpecification<ConcertEntity>`,
`IUpcomingSpecification<OpportunityEntity>`, future entities) — no per-T
registration needed.

#### Where they're used

| Specification | Dashboard sections | Closed generics |
|---|---|---|
| `IUpcomingSpecification<T>` | VS5 upcoming concerts strip; AS5 upcoming gigs strip; VS2 + AS2 KPI counts for "upcoming concerts/gigs"; VS7 open opportunities | `ConcertEntity`, `OpportunityEntity` |
| `IDateRangeSpecification<T>` | Anywhere a date window narrower than "all future" is needed against a range entity — e.g. "concerts in next 30 days" KPI counts | `ConcertEntity`, `OpportunityEntity` |

**Ticket sales and payouts are point-in-time events**, not range entities.
They do **not** implement `IHasDateRange` and they do **not** go through
these specifications. Their date-window filters stay inline in the repo
method, still using the `DateRange` value object as a parameter for
symmetry:

```csharp
var window = new DateRange(now.AddMonths(-6), now);
await context.TicketSales
    .Where(t => t.PurchasedAt >= window.Start && t.PurchasedAt < window.End)
    .GroupBy(t => new { t.PurchasedAt.Year, t.PurchasedAt.Month })
    .Select(g => new MonthlyRevenuePointDto { /* multi-aggregate */ })
    .ToListAsync();
```

These queries live in exactly one place each (the chart endpoint or the
MTD KPI orchestration). A specification earns its keep on reuse, and a
one-call-site filter doesn't qualify.

Both specifications are **shared cross-module primitives** — interfaces
in `Concertable.Shared.Application/Specifications/`, impls in
`Concertable.Shared.Infrastructure/Specifications/`, markers in
`Concertable.Shared.Domain/Entities/`. Concert (and any future module
with range entities) consumes them.

**What we explicitly do *not* introduce:**

- No `IVenueXxxSpecification` / `IArtistXxxSpecification` — venue/artist filtering is just `.Where(x => x.VenueId == venueId)` inline. One-line `Where` clauses don't earn an interface.
- No purpose-built per-query specifications (`IVenueApplicationsAwaitingReviewSpecification` etc.) — specifications earn their keep when *reused*.
- No `IHasEndDate` / `IHasOccurredAt` — `IHasDateRange` covers the range-entity needs uniformly; point-in-time entities don't get a marker at all.

Existing `ISortSpecification<T>` / `IGeometrySpecification<T>` aren't used by the dashboard endpoints (no user-driven sort/geometry on internal "give me X" queries).

### 2. One HTTP round trip per visual section

The frontend makes **one call per dashboard section**, not one call per
data point inside that section. The KPI strip is one section (one call → 4
numbers). A chart is one section. Each list is its own section because
they have independent polling cadences and failure modes.

Where a section's data crosses modules (e.g. the KPI strip needs counts
from Concert + Payment), the controller in the persona's home module
(`Venue.Api` / `Artist.Api`) orchestrates the cross-module aggregation by
calling each `IXModule` facade in parallel via `Task.WhenAll`. From the
client's perspective it's one round trip; server-side it's a single
request that fans out to module facades — each of which does **one**
specification-composed query.

```csharp
// VenueDashboardController.GetKpisAsync — illustrative
var (apps, opps, upcoming, mtdRevenue) = await TaskWhenAll(
    concertModule.GetVenueApplicationsAwaitingReviewCountAsync(venueId, ct),
    concertModule.GetVenueOpenOpportunitiesCountAsync(venueId, ct),
    concertModule.GetVenueUpcomingConcertsCountAsync(venueId, ct),
    paymentModule.GetVenueTicketRevenueMtdAsync(venueId, ct));
return new VenueDashboardKpisDto(apps, opps, upcoming, mtdRevenue);
```

This is the same shape as `ConcertHeaderRepository.SearchAsync` returning a
single `IPagination<ConcertHeaderDto>` — one call out, one composed
result.

## Shared frontend additions

New shared primitives (one place, both personas):

```
app/web/shared/src/features/dashboard/
  components/
    DashboardCard.tsx         # title + optional action + body + footer shell
    KpiTile.tsx               # big-number tile with optional sparkline + delta
    ChartCard.tsx             # DashboardCard + Recharts ResponsiveContainer
    ActivityFeed.tsx          # generic timeline list (icon + line + timestamp)
    SectionGrid.tsx           # 12-col Tailwind grid wrapper
    StripeConnectBanner.tsx   # conditional onboarding nudge
    ProfileHealthCard.tsx     # checklist (bio / photos / genres / Stripe)
  hooks/
    useDashboardPolling.ts    # opinionated TanStack Query defaults (30s/60s)
  index.ts
```

Add to `app/web/shared/package.json` (if not already):

- `recharts` — chart library
- `@tanstack/react-table` — likely already transitively via shadcn Data Table; verify

Add a `./features/dashboard` export in `app/shared/package.json` if any
cross-app types live there (probably not — UI-only).

## Venue front page

**Page component:** `app/web/shared/src/features/venues/pages/VenueDashboardPage.tsx`
(replace placeholder).

### Layout (12-col grid)

```
+-------------------------------------------------------------+
|  Welcome header + profile health (col-span-12)              |
+-------------------------------------------------------------+
|  KPI strip — 4 tiles (col-span-3 each)                      |
|  apps to review | open opps | upcoming concerts | MTD rev   |
+-------------------------------------------------------------+
|  Stripe Connect banner (col-span-12, conditional)           |
+-------------------------------------------------------------+
|  Applications to review (col-span-7)  | Inbox (col-span-5)  |
+-------------------------------------------------------------+
|  Upcoming concerts strip — horizontal scroll (col-span-12)  |
+-------------------------------------------------------------+
|  Ticket revenue by month chart (col-span-7) | Reviews (5)   |
+-------------------------------------------------------------+
|  Open opportunities (col-span-7)  | Activity feed (5)       |
+-------------------------------------------------------------+
|  Recent settlements (col-span-12)                           |
+-------------------------------------------------------------+
```

### Round-trip plan — Venue

One controller (`VenueDashboardController` in `Venue.Api`) exposes one
endpoint per visual section. Each endpoint = **one HTTP round trip**,
composed server-side from one or more module-facade calls
(parallel via `Task.WhenAll` when cross-module).

| # | Section | Endpoint | Returns | Facade calls | Cadence |
|---|---|---|---|---|---|
| VS1 | Header (welcome + profile health + Stripe + reviews) | `GET /api/venues/me/dashboard/header` | `VenueDashboardHeaderDto` | `IVenueModule.GetProfileHealthAsync` + `IPaymentModule.GetConnectStatusAsync` + `IReviewModule.GetVenueReviewSummaryAsync` (parallel) | on mount |
| VS2 | KPI strip (4 numbers) | `GET /api/venues/me/dashboard/kpis` | `VenueDashboardKpisDto` | `IConcertModule.{applicationsAwaitingReview, openOpportunities, upcomingConcerts(days=30)}CountAsync` + `IPaymentModule.GetVenueTicketRevenueMtdAsync` (parallel) | 30s |
| VS3 | Applications to review list | `GET /api/venues/me/dashboard/applications-to-review?take=5` | `ApplicationDto[]` | `IConcertModule.GetVenueApplicationsAwaitingReviewAsync` | 30s |
| VS4 | Inbox preview | `GET /api/venues/me/dashboard/inbox?take=5` | `MessageThreadDto[]` | `IMessagingModule.GetRecentThreadsAsync` | 30s |
| VS5 | Upcoming concerts strip | `GET /api/venues/me/dashboard/upcoming-concerts?take=5` | `ConcertCardDto[]` | `IConcertModule.GetVenueUpcomingConcertsAsync` | 60s |
| VS6 | Ticket revenue chart (6-month series, gross + net) | `GET /api/venues/me/dashboard/charts/ticket-revenue?monthsBack=6` | `MonthlyRevenuePointDto[]` | `IPaymentModule.GetVenueTicketRevenueByMonthAsync` | 60s |
| VS7 | Open opportunities | `GET /api/venues/me/dashboard/open-opportunities` | `OpportunityWithCountsDto[]` (opportunity + application count + days-until) | `IConcertModule.GetVenueOpenOpportunitiesAsync` (one query joining application counts) | 60s |
| VS8 | Activity feed | `GET /api/venues/me/dashboard/activity?take=10` | `ActivityItemDto[]` | `INotificationModule.GetRecentActivityAsync` | 30s |
| VS9 | Recent settlements | `GET /api/venues/me/dashboard/settlements?take=5` | `SettlementDto[]` | `IPaymentModule.GetVenueRecentSettlementsAsync` | 60s |

**~9 round trips on first page load** (down from ~14 in the original per-widget
plan), with each refresh cycle hitting only the sections whose polling
interval has elapsed.

### New DTOs (Venue-driving)

Most live in `api/Modules/Concert/Concertable.Concert.Application/DTOs/`,
`api/Modules/Payment/Concertable.Payment.Application/DTOs/`, etc. Cross-module
shapes (the kind exposed via `IXModule`) belong in `Module.Contracts/`.
Aggregate dashboard DTOs (`VenueDashboardKpisDto`, `VenueDashboardHeaderDto`)
live in `Venue.Application/DTOs/` since the orchestration owns them.

- `VenueDashboardHeaderDto { ProfileHealth, StripeConnect, ReviewSummary }`
- `VenueDashboardKpisDto { ApplicationsToReview: int, OpenOpportunities: int, UpcomingConcerts: int, MtdRevenueCents: long }`
- `VenueProfileHealthDto { Completeness: int, MissingItems: string[] }`
- `MonthlyRevenuePointDto { Month: DateOnly, GrossCents: long, NetCents: long }` (one row = one aggregate result; reused by artist payouts)
- `ActivityItemDto { Type: enum, At: DateTime, Subject: string, Url: string }` (shared with artist)
- `SettlementDto { ConcertId: int, ConcertName: string, At: DateTime, AmountCents: long, Direction: enum }`
- `OpportunityWithCountsDto { Opportunity: OpportunityDto, ApplicationCount: int, DaysUntilDeadline: int }`

Reuse existing DTOs where possible (`ApplicationDto`, `ConcertCardDto`,
`ReviewSummaryDto`, `MessageThreadDto`).

## Artist front page

**Page component:** `app/web/shared/src/features/artists/pages/ArtistDashboardPage.tsx`
(replace placeholder).

### Layout (12-col grid)

```
+-------------------------------------------------------------+
|  Welcome header + profile health (col-span-12)              |
+-------------------------------------------------------------+
|  KPI strip — 4 tiles (col-span-3 each)                      |
|  pending apps | accepted awaiting checkout | upcoming gigs  |
|  | MTD payouts                                              |
+-------------------------------------------------------------+
|  Stripe Connect banner (col-span-12, conditional)           |
+-------------------------------------------------------------+
|  My applications (col-span-7)  |  Inbox preview (col-span-5)|
+-------------------------------------------------------------+
|  Upcoming gigs strip (col-span-12)                          |
+-------------------------------------------------------------+
|  Payout trend chart (col-span-7) | Reviews snapshot (5)     |
+-------------------------------------------------------------+
|  Recommended opportunities (col-span-7) | Activity (5)      |
+-------------------------------------------------------------+
```

### Round-trip plan — Artist

`ArtistDashboardController` in `Artist.Api`, same pattern as venue.

| # | Section | Endpoint | Returns | Facade calls | Cadence |
|---|---|---|---|---|---|
| AS1 | Header | `GET /api/artists/me/dashboard/header` | `ArtistDashboardHeaderDto` | `IArtistModule.GetProfileHealthAsync` + `IPaymentModule.GetConnectStatusAsync` + `IReviewModule.GetArtistReviewSummaryAsync` (parallel) | on mount |
| AS2 | KPI strip | `GET /api/artists/me/dashboard/kpis` | `ArtistDashboardKpisDto` | `IConcertModule.{applications(status=Pending), applications(status=AcceptedAwaitingCheckout), upcomingConcerts(days=30)}CountAsync` + `IPaymentModule.GetArtistPayoutsMtdAsync` (parallel) | 30s |
| AS3 | My applications (grouped by status) | `GET /api/artists/me/dashboard/applications?take=10` | `ApplicationDto[]` | `IConcertModule.GetArtistApplicationsAsync` | 30s |
| AS4 | Inbox preview | `GET /api/artists/me/dashboard/inbox?take=5` | `MessageThreadDto[]` | `IMessagingModule.GetRecentThreadsAsync` | 30s |
| AS5 | Upcoming gigs strip | `GET /api/artists/me/dashboard/upcoming-gigs?take=5` | `ConcertCardDto[]` | `IConcertModule.GetArtistUpcomingConcertsAsync` | 60s |
| AS6 | Payout trend chart (6-month series) | `GET /api/artists/me/dashboard/charts/payouts?monthsBack=6` | `MonthlyRevenuePointDto[]` (reused DTO) | `IPaymentModule.GetArtistPayoutsByMonthAsync` | 60s |
| AS7 | Recommended opportunities | `GET /api/artists/me/dashboard/recommended-opportunities?take=5` | `OpportunityCardDto[]` | `ISearchModule.GetRecommendedOpportunitiesForArtistAsync` | 60s |
| AS8 | Activity feed | `GET /api/artists/me/dashboard/activity?take=10` | `ActivityItemDto[]` (shared DTO) | `INotificationModule.GetRecentActivityAsync` | 30s |

**~8 round trips on first page load.**

### New DTOs (Artist-driving)

- `ArtistDashboardHeaderDto { ProfileHealth, StripeConnect, ReviewSummary }`
- `ArtistDashboardKpisDto { PendingApplications: int, AcceptedAwaitingCheckout: int, UpcomingGigs: int, MtdPayoutsCents: long }`
- `ArtistProfileHealthDto { Completeness: int, MissingItems: string[] }`

Reuse `MonthlyRevenuePointDto`, `ActivityItemDto`, `ApplicationDto`,
`ConcertCardDto`, `ReviewSummaryDto`, `OpportunityCardDto`,
`MessageThreadDto`.

## Backend additions — summary by module

> **Rule reminders:**
> - New `IXModule` methods must delegate to `Module.Application` services/repos — no inline EF in the facade.
> - Module.Contracts owns cross-module DTO shapes.
> - Inside each repo method: one composed `IQueryable` → one materialisation → one round trip, exactly like `ConcertHeaderRepository`. Aggregates use `.GroupBy(...).Select(g => new Dto { Sum1 = g.Sum(...), Sum2 = g.Sum(...), Count = g.Count() })` — multi-aggregate in **one** SELECT.
> - Reusable specifications (`IUpcomingSpecification<T>`, `IDateRangeSpecification<T>`) live in `Concertable.Shared.{Application,Infrastructure}/Specifications/`; markers in `Concertable.Shared.Domain/Entities/`. Module-specific specs (if any prove necessary later) live in `Module.Infrastructure/Specifications/` mirroring `Concertable.Search.Infrastructure.Specifications`.

### New controllers

- `VenueDashboardController` in `Concertable.Venue.Api/Controllers/` — orchestrates the venue's 9 section endpoints, injects all the facades it needs.
- `ArtistDashboardController` in `Concertable.Artist.Api/Controllers/` — same shape for the 8 artist sections.

### `IVenueModule` (Concertable.Venue.Contracts)

- `GetProfileHealthAsync(int venueId, CancellationToken)` — `VenueProfileHealthDto`

### `IArtistModule` (Concertable.Artist.Contracts)

- `GetProfileHealthAsync(int artistId, CancellationToken)` — `ArtistProfileHealthDto`

### `IConcertModule` (Concertable.Concert.Contracts)

Counts (used by KPI strip orchestration):

- `GetVenueApplicationsAwaitingReviewCountAsync(int venueId, ct)`
- `GetVenueOpenOpportunitiesCountAsync(int venueId, ct)`
- `GetVenueUpcomingConcertsCountAsync(int venueId, int days, ct)`
- `GetArtistApplicationsCountAsync(int artistId, ApplicationStatus status, ct)` (called twice from KPI orchestration for Pending and AcceptedAwaitingCheckout)
- `GetArtistUpcomingConcertsCountAsync(int artistId, int days, ct)`

Lists:

- `GetVenueApplicationsAwaitingReviewAsync(int venueId, int take, ct)`
- `GetVenueOpenOpportunitiesAsync(int venueId, ct)` — verify if existing; must return `OpportunityWithCountsDto` (application count joined in the same query, not a per-row lookup)
- `GetVenueUpcomingConcertsAsync(int venueId, int take, ct)` — verify if existing
- `GetArtistApplicationsAsync(int artistId, int take, ct)`
- `GetArtistUpcomingConcertsAsync(int artistId, int take, ct)` — verify if existing

Repo implementations compose: inline `.Where(x => x.VenueId == venueId)` /
`.Where(x => x.ArtistId == artistId)` + `IUpcomingSpecification<T>` /
`IDateRangeSpecification<T>` chained onto one `IQueryable`, materialised once.

### `IPaymentModule` (Concertable.Payment.Contracts)

- `GetVenueTicketRevenueMtdAsync(int venueId, ct)` — single `SUM` aggregate query, one row out
- `GetVenueTicketRevenueByMonthAsync(int venueId, int monthsBack, ct)` — `.GroupBy(month).Select(...)` returning `MonthlyRevenuePointDto[]` (gross + net + count, **one** query with multiple aggregates)
- `GetVenueRecentSettlementsAsync(int venueId, int take, ct)`
- `GetArtistPayoutsMtdAsync(int artistId, ct)`
- `GetArtistPayoutsByMonthAsync(int artistId, int monthsBack, ct)` — same `.GroupBy()` shape as venue, single query

### `INotificationModule` (Concertable.Notification.Contracts)

- `GetRecentActivityAsync(Guid userId, int take, ct)` — `ActivityItemDto[]`, one ordered LIMIT query

### `IReviewModule` / `IMessagingModule` / `ISearchModule`

- Verify existence of `GetVenueReviewSummaryAsync`, `GetArtistReviewSummaryAsync`, `GetRecommendedOpportunitiesForArtistAsync`, `GetRecentThreadsAsync` before adding. Where they exist, reuse; where they don't, follow the same single-query-via-spec pattern.

### Migrations

**One InitialCreate re-scaffold required** — driven solely by the
`ConcertEntity` `DateRange` refactor (Phase B Step 9). The owned-type
column mapping (`StartDate`/`EndDate` under `Period`) is a schema
*shape* change even though column names are unchanged; per
[CLAUDE.md](../CLAUDE.md), all module `InitialCreate`s re-scaffold
together via `./initial-migrations.ps1`. No additive migrations.

Beyond that, every dashboard widget is a *read* against existing entities
(opportunities, applications, concerts, tickets, payments, notifications,
reviews). No new tables.

## Visual specification

### Design principle

> **Everything noteworthy is one click away — but the dashboard itself
> shows only what fits on one screen without cognitive overload.**

Concretely:

- Each list/feed widget shows **at most 5 items** + a `View all →` link to the dedicated page. Don't dump 20 rows on the dashboard.
- Each KPI shows **one number** + optional one-line context (trend, delta). No nested tables inside tiles.
- Each chart shows **one signal** (e.g. revenue) with at most **two stacked series** (gross + net). No 4-line overlapping noise.
- Widgets either **answer a question** (KPI: "do I need to do something?") or **invite a click** (list: "tap to act"). If a widget does neither, cut it.

### Color & icon tokens

Use Tailwind tokens consistently across both personas:

| Semantic | Token | Used for |
|---|---|---|
| **Positive / money in** | `text-emerald-600` / `bg-emerald-50` | Revenue numbers, payout numbers, positive trend deltas |
| **Urgent / needs attention** | `text-rose-600` / `bg-rose-50` | Apps awaiting review, Stripe-incomplete banner, payment failures |
| **Pending / warning** | `text-amber-600` / `bg-amber-50` | Accepted-awaiting-checkout KPI, deadline-soon opportunities |
| **Neutral info** | `text-sky-600` / `bg-sky-50` | Upcoming concerts, recommended opportunities |
| **Muted secondary** | `text-muted-foreground` | KPI labels, timestamps, axis ticks, empty-state text |

Icons via Lucide (already in stack). One per widget header (e.g. `Inbox`,
`TrendingUp`, `Users`, `Music`, `Calendar`, `CreditCard`).

### Component anatomy

Mockups below show the **filled** state. Loading/empty/error variants are
covered separately further down.

#### `DashboardCard` shell — wraps every widget

```
┌─────────────────────────────────────────────┐
│  Icon  Section title           Action →     │ ← header row: icon left, title, optional CTA right
├─────────────────────────────────────────────┤
│                                             │
│   (widget body slot)                        │
│                                             │
├─────────────────────────────────────────────┤
│  Optional footer link / metadata            │ ← only when present
└─────────────────────────────────────────────┘
```

Tailwind: `rounded-lg border bg-card p-4 shadow-sm`. Header is `flex items-center justify-between mb-3`.

#### `KpiTile`

```
┌──────────────────────┐
│  Apps to review      │ ← label (muted, sm)
│                      │
│   12                 │ ← big number (3xl/4xl, bold)
│                      │
│  ↑ +3 since yesterday│ ← trend line (green/red, xs)
└──────────────────────┘
```

Props: `label`, `value`, `delta?`, `intent?` (positive/urgent/pending/neutral
— selects color), `icon?`, `href` (clicking the whole tile navigates).

Tiles are **always clickable** — destinations are listed in
["What clicking each widget does"](#what-clicking-each-widget-does) below.
One-click principle applies here above all.

#### `ChartCard` (Recharts)

```
┌────────────────────────────────────────────┐
│  📈 Ticket revenue          Last 6 months  │
├────────────────────────────────────────────┤
│   £                                        │
│  3k ┤                  ▄                   │
│  2k ┤            ▄    █▆                   │
│  1k ┤   ▂   ▂  █▆   ▆██  ▆                 │
│  0  └──────────────────────────────         │
│     Nov  Dec  Jan  Feb  Mar  Apr            │
└────────────────────────────────────────────┘
```

- Recharts `<AreaChart>` for monthly series, gross stacked over net
- X-axis: month abbreviation; Y-axis: currency-formatted, max 4 ticks
- Tooltip on hover: month + gross + net + count
- Color: emerald for revenue, sky for payouts (artist side)
- No legend chrome unless ≥2 series; reduce visual noise
- "Last 6 months" is a static label in v1 — custom date ranges are out of scope (see top-of-plan). Picker UI added in a follow-up when needed.

#### Application row (list widget body)

```
┌──────────────────────────────────────────────┐
│  [avatar]  Artist name             £1,200    │
│            Opportunity: Fri 4 Jul · Slot 2    │
│            "Quick bio line if present..."     │
│                                    [Accept] [Decline]
└──────────────────────────────────────────────┘
```

Reuses existing `ApplicationCard` from `app/web/shared/src/features/concerts/components/applications/` — wrap in `DashboardCard` body with a 5-item limit and `View all applications →` footer.

#### Concert / gig strip card (horizontal scroll)

```
┌────────────┐  ┌────────────┐  ┌────────────┐
│ [banner]   │  │ [banner]   │  │ [banner]   │
│ Fri 4 Jul  │  │ Sat 12 Jul │  │ Sun 21 Jul │
│ Slot night │  │ Open mic   │  │ Album launch│
│ Venue: Foo │  │ Venue: Bar │  │ Venue: Baz │
│ ▓▓▓▓▓▒░ 72%│  │ ▓▓░░░░░ 18%│  │ ░░░░░░░ 0% │ ← sold-% bar
└────────────┘  └────────────┘  └────────────┘
        ← horizontal scroll →
```

Reuses existing `ConcertCard`. Strip is Embla carousel (already in stack)
with 5 items max + `View all concerts →`.

#### Activity feed row

```
   • Mar 14, 10:32   Artist X applied to your "Slot night" opportunity   →
   • Mar 14, 09:01   Concert "Album launch" settled — £842 to artist     →
   • Mar 13, 19:45   Anna Q. left a 5-star review                        →
   • Mar 13, 14:12   Ticket sold: "Slot night" (now 28/40)               →
```

One row per event. Icon by type (apply / settle / review / sale).
Timestamp is muted, relative until 24h then absolute. Each row is a link
to the relevant detail page. 10-item cap + `View all activity →`.

#### Inbox preview row

```
   ●  Mike from "The Beans"          ← unread (rose dot) on unread rows
      "Cheers for the slot, quick question…"
                              2h ago
```

Reuses existing `Mailbox` component (the one already used for the unread
badge in the navbar) but in expanded form. 5 threads + `Open inbox →`.

#### Profile health card

```
┌─────────────────────────────────────┐
│  Profile health         80% complete│ ← progress bar across header
├─────────────────────────────────────┤
│  ✓  Add a bio                       │
│  ✓  Upload a banner                 │
│  ✓  Set genres                      │
│  ○  Add 3 photos          (1 of 3)  │
│  ○  Connect Stripe payouts          │
└─────────────────────────────────────┘
```

Driven by `IXModule.GetProfileHealthAsync` returning `Completeness: int +
MissingItems: string[]`. Checked items collapse; unchecked items are
clickable to the relevant settings page.

#### Stripe Connect banner (conditional)

```
┌─────────────────────────────────────────────────────────┐
│  ⚠  Finish Stripe setup to receive payouts              │
│     Without this, settlements won't reach your bank.    │
│                              [Complete setup → ]        │
└─────────────────────────────────────────────────────────┘
```

Only rendered when `IPaymentModule.GetConnectStatusAsync` reports incomplete. Amber background (`bg-amber-50 border-amber-200`).

#### Settlement row (venue) / payout row (artist)

```
   Mar 12   "Album launch"           £842 → Artist Anna   →
   Mar 09   "Open mic"               £128 → Artist Joe    →
   Mar 02   "Slot night"             £312 ← from venue    →   (artist view, money in)
```

Compact 1-line row. Date · concert name · amount · counterparty · arrow indicating direction.

### State variants

Each widget renders one of: `loading`, `empty`, `error`, `filled`.

| State | Pattern |
|---|---|
| **Loading** | `<Skeleton>` rectangles matching the filled layout. KPI tile = one `h-9 w-20` shimmer for the number; chart = `h-40 w-full`; list = 3 row shimmers. |
| **Empty** | Centered muted icon + 1-line message + suggested action button. Example: "No applications yet — share your opportunity to attract artists" with `[Share opportunity]` CTA. |
| **Error** | Muted error icon + "Something went wrong" + `[Retry]` button that calls `query.refetch()`. Never block the page; one widget's failure doesn't cascade. |
| **Filled** | As mocked up above. |

### Responsive collapse

| Breakpoint | Layout |
|---|---|
| **Desktop ≥ 1280px** | 12-col grid as designed. |
| **Tablet 768–1279px** | Collapse to 6-col grid: KPI tiles wrap to 2×2; the side-by-side pairings (apps + inbox, chart + reviews, opps + activity) stack vertically. |
| **Mobile < 768px** | Single column. Section order is **action-first**: KPI strip → Stripe banner → applications → upcoming concerts → chart → everything else. Lists drop to 3 items + `View all`. |

Mobile is desktop-first-collapsed, not a separate design — v1 priority is
desktop polish. A dedicated mobile-responsive pass is listed in
[Open follow-ups](#open-follow-ups-not-this-pr).

### What clicking each widget does

To make the "one click away" promise explicit:

| Click target | Destination |
|---|---|
| KPI tile | The dedicated page filtered to that KPI's slice (e.g. "Apps to review" → `/applications?status=pending`) |
| Chart card | A larger version on a dedicated `/insights/revenue` page (future — for v1 the card is the canonical view) |
| List row | The detail page for that entity |
| `View all →` footer | The unfiltered list page |
| Activity row | The relevant entity's detail page (the application / concert / review that triggered the event) |
| Stripe banner CTA | Existing `/stripe-return` onboarding flow |

## Polling cadences

Codified in `useDashboardPolling.ts`. Cadence applies **per section
endpoint**, not per widget — because each section = one HTTP call.

| Tier | Cadence | Used for |
|---|---|---|
| `fast` | 30s | KPI strip, action lists (apps to review, my applications), inbox preview, activity feed |
| `normal` | 60s | Charts, upcoming concerts strip, open opportunities, settlements, recommended opportunities |
| `static` | on-mount only | Header (profile health + Stripe + reviews) |

All TanStack Query keys include `[persona, userId, sectionKey, ...args]` so
invalidation is targeted. Tab-focus refetch enabled. When SignalR push lands
in a follow-up, push events trigger targeted `queryClient.invalidateQueries`
for the affected section key.

## Build order — frontend-first with mocks

UX is the biggest unknown in this plan; backend queries are mechanical
once shapes are known. So build the frontend against a typed mock layer
first, lock the UX, then implement the backend against the contracts the
frontend already wrote.

### Phase A — Frontend with mocks

1. **TypeScript DTO contracts** — one file per section, hand-written. These become the API spec the backend implements later. Location: `app/web/shared/src/features/dashboard/contracts/` (e.g. `VenueDashboardKpis.ts`, `MonthlyRevenuePoint.ts`).
2. **Fixture data** — `app/web/shared/src/features/dashboard/mocks/`. At minimum three personas per role: empty-state, mid-tier, thriving. Vary numbers, lengths, and zero-states deliberately so visual edge cases (long names, 0%, very large amounts) are exercised.
3. **Mock api layer** — `app/web/shared/src/features/dashboard/api/mockClient.ts` exports the same function signatures the real client will (`getVenueDashboardKpis()` etc.). Each returns `Promise.resolve(fixture)` with 200–600ms randomised latency. Toggled via Vite env: `VITE_USE_MOCK_DASHBOARD=true` in `.env.local`. The real client is a sibling `realClient.ts`; a thin `index.ts` exports whichever the env var selects, so widgets never know which is wired.
4. **Shared frontend shell** — `DashboardCard`, `KpiTile`, `ChartCard`, `ActivityFeed`, `SectionGrid`, `StripeConnectBanner`, `ProfileHealthCard`, `useDashboardPolling`.
5. **Venue widgets** — wire each one against the mock client; iterate visual density and responsive behaviour.
6. **Artist widgets** — same.
7. **Stripe + profile-health banners** — both personas.
8. **UX freeze** — visual polish, persona-fixture swap to verify empty/mid/thriving all look right, responsive collapse, skeleton/empty/error variants verified by toggling fixtures.

### Phase B — Backend to match contracts

9. **ConcertEntity `DateRange` refactor** — align with `OpportunityEntity`; re-scaffold `InitialCreate`.
10. **Shared specifications** — `IHasDateRange` marker, `IUpcomingSpecification<T>` + `IDateRangeSpecification<T>` interfaces/impls, DI registrations.
11. **Backend endpoints** — new `VenueDashboardController` + `ArtistDashboardController`, new module-facade methods + repository methods + C# DTOs. The DTO field names and types **mirror the TS contracts verbatim** — that's the contract the frontend already wrote.
12. **Integration tests** — per new module-facade method against seeded data.

### Phase C — Cutover

13. **Swap mock → real** — flip `VITE_USE_MOCK_DASHBOARD=false`, smoke-test each section against the seeded venue + artist managers.
14. **Delete the mock layer** — `mockClient.ts`, `mocks/` fixtures, the env-toggle in the dispatcher. Keep the contracts (`contracts/`) — they're now living types used by both the real client and the widget components.
15. **Build verification** — `npm -w @concertable/web-venue run build`, `npm -w @concertable/web-artist run build` (per [app/web/CLAUDE.md](./web/CLAUDE.md)).

## Mock data layer

### File layout

```
app/web/shared/src/features/dashboard/
  contracts/                    # TS types — live past Phase C as the real contracts
    VenueDashboardHeader.ts
    VenueDashboardKpis.ts
    ArtistDashboardHeader.ts
    ArtistDashboardKpis.ts
    MonthlyRevenuePoint.ts
    ActivityItem.ts
    Settlement.ts
    OpportunityWithCounts.ts
    ProfileHealth.ts
    index.ts
  mocks/                        # deleted in Phase C
    fixtures/
      venue.empty.ts
      venue.mid.ts
      venue.thriving.ts
      artist.empty.ts
      artist.mid.ts
      artist.thriving.ts
    mockClient.ts               # implements api/client.ts function signatures
    selectFixture.ts            # reads ?persona=... query string or defaults
  api/
    client.ts                   # one file, env-toggled
    realClient.ts               # axios calls hitting /api/venues/me/dashboard/*
    mockClient.ts -> ../mocks/mockClient.ts   # re-export
  components/...
  hooks/...
```

### Mock dispatcher pattern

```ts
// app/web/shared/src/features/dashboard/api/client.ts
import * as real from "./realClient";
import * as mock from "../mocks/mockClient";

const USE_MOCK = import.meta.env.VITE_USE_MOCK_DASHBOARD === "true";

export const dashboardClient = USE_MOCK ? mock : real;
```

Widgets import `dashboardClient` and never know which is active. Vite
tree-shakes the unused branch at build time when `VITE_USE_MOCK_DASHBOARD`
is set at compile time.

### Persona-switch in dev

A dev-only floating control (visible only when `VITE_USE_MOCK_DASHBOARD=true`)
lets you toggle between fixtures: `?persona=empty | mid | thriving`. Makes
it trivial to validate all visual states without rewriting fixtures.

### Why hand-rolled, not Faker / MSW

- **No Faker** — randomness obscures whether an edge case is the *fixture* or the *widget*. Hand-rolled fixtures are deliberate.
- **No MSW** — adds a service worker, complicates dev startup, overkill for a swap that's just an `if/else` over two clients. We aren't testing fetch-layer behaviour, we're testing widget rendering.

## Verification

End-to-end check after wiring:

- Log in as the seeded venue manager → `/_venue/` renders all widgets with real seeded data; pending applications resolve to known seeded apps.
- Log in as a seeded artist → `/_artist/` renders; payout trend shows seeded settlements.
- Each widget loading skeleton, error state, and empty state visible by toggling network in DevTools / seeding an empty user.
- Polling: keep tab open, action a seeded application from another browser, watch KPI tile + list both update within ~30s.
- Build green: all four `npm -w @concertable/web-* run build` commands.
- Unit/integration tests for each new module facade method (xUnit, follow existing `*.IntegrationTests` patterns).

## Open follow-ups (not this PR)

- SignalR push for instant updates on new applications + ticket sales
- Drag-to-rearrange widgets per user
- Custom date ranges per widget
- Kanban pipeline view as an alternate `/_venue/pipeline` route
- Calendar view as `/_venue/calendar`
- Mobile-responsive layout pass (v1 should be desktop-first; mobile gets a single-column stack)
