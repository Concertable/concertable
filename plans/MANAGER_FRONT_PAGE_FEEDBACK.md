# Manager Front Page — Session Feedback & Decisions

Captured during Phase A implementation. These supersede the original plan
where they conflict. Read alongside [MANAGER_FRONT_PAGE_PLAN.md](./MANAGER_FRONT_PAGE_PLAN.md).

## Naming & terminology

- **`Header` → `Overview`** everywhere. The dashboard top strip is named
  `Overview` (DTO: `VenueDashboardOverview` / `ArtistDashboardOverview`,
  endpoint: `/overview`, hook: `useVenueOverview` / `useArtistOverview`).
  Reason: `Header` collides with the existing search `Header` polymorphic
  type (`Artist | Venue | Concert`) at
  `app/shared/src/features/search/types.ts`.
- **`Concert` not `gig`** anywhere. Artist KPI label is "Upcoming concerts",
  endpoint is `/upcoming-concerts`, hook is `useArtistUpcomingConcerts`,
  hero is `ArtistNextConcertHero`. Backend entity is `ConcertEntity` so the
  FE matches.

## Architecture

- **Persona-specific dashboard code lives in the SPA, not in shared.**
  The plan originally placed `VenueDashboardPage` in
  `app/web/shared/src/features/venues/pages/`. **Wrong.** Dashboard widgets
  + page + hooks + fixtures + `dashboardApi.ts` + `types.ts` are 100% tied
  to the manager persona — they belong in `app/web/{venue,artist}/src/features/dashboard/`.
  Shared keeps only agnostic UI primitives and cross-cutting types.
- **Per-SPA `dashboardApi.ts` following the `venueApi.ts` pattern.** One
  flat const object per SPA, default-export. No interface, no mock vs real
  dispatcher, no separate mock file. The mock IS the dashboardApi — when
  real endpoints land, swap each method body from
  `return venueFixtures[selectPersona()].xxx;` to
  `const { data } = await api.get(...); return data;`. Export shape stays.
  Reason: React-idiomatic is object literal + structural typing, not
  interface+impl (that's a .NET/Angular DI-container reflex that doesn't
  transfer to React).
- **No `take` / `monthsBack` parameters on FE hooks.** Server decides
  response size. Hook signatures are nullary (`useVenueInbox()` not
  `useVenueInbox(5)`). The `/charts/ticket-revenue?monthsBack=6` and
  `?take=N` query strings in the original plan should be dropped — the
  server is authoritative on window/limit.
- **One hook per file in a `hooks/` folder.** Matches the existing
  `app/shared/src/features/venues/hooks/` convention. Each section gets
  its own `useVenueXxx.ts` file plus a barrel `index.ts`.

## Data model

- **`ProfileHealth` is a single items list with `done: boolean` per item.**
  Not split `items[]` + `done[]`. BE returns the full checklist;
  FE renders rows, ticking `done: true` ones. Shape:
  ```ts
  interface ProfileHealthItem { id; label; href; done: boolean }
  interface ProfileHealth { completeness: number; items: ProfileHealthItem[] }
  ```

## Code quality rules from review

- **`formatCurrency` lives in `@concertable/shared/lib`** (single source).
  Signature: `formatCurrency(cents, { currency?, compact?, fractionDigits? })`.
  Don't duplicate locally in widgets.
- **`<ChartTooltip>` is our wrapper around recharts' `<Tooltip>`.** All
  the recharts type-juggling lives inside `ChartTooltip.tsx`. Charts compose
  `<ChartTooltip currency="GBP" />`, never inline a content callback.
- **No `as string` casts that lie.** Use `String(x)` to coerce honestly
  (e.g. `key={String(p.dataKey)}` not `key={p.dataKey as string}`).
- **No overdefensive `?? ""` on values that are statically known.**
  Recharts payloads come from our `dataKey`s — they're deterministic.

## Mock-tier infrastructure (deleted in Phase C)

When real BE lands, `dashboardApi.ts` **stays** — method bodies are swapped from fixture returns to real `api.get(...)` calls. Export shape is unchanged. These are the only things deleted:

- `app/web/{venue,artist}/src/features/dashboard/fixtures/` — fake data per persona
- `app/shared/src/features/dashboard/persona.ts` — `FixturePersona`, `selectPersona`, `NOW` anchor + date helpers (`daysAhead`, `daysAgo`, `hoursAgo`, `monthsAgoIso`)
- `app/web/shared/src/features/dashboard/components/PersonaSwitcher.tsx` — dev-only floating control

Fixtures pin a deterministic dayjs anchor: `NOW = dayjs("2026-05-18T12:00:00Z")`. No live `Date.now()` calls.

## Artist diverges from venue layout

Artist isn't a mirror of venue:

- **No `ApplicationsToReview`** — artists don't review applications.
- **No `OpenOpportunities`** — artists don't post them.
- **No `Settlements`** — artists receive money, don't pay out.
- **`ApplicationsPipeline`** — artist's applications grouped by status
  (Pending / Awaiting payment / Confirmed / Rejected). Replaces venue's
  "applications to review".
- **`NextConcertHero`** — the most imminent concert promoted to a wide
  hero card with countdown ("In 4 days") + venue + ticket-sold progress.
  Sits where venue's upcoming-concerts strip sits.
- **`RecommendedOpportunities`** — full-width strip, more prominent than
  venue's open-opportunities widget. The artist's outbound funnel.

## Surviving file structure

```
app/shared/src/features/dashboard/
  contracts/common.ts          # shared DTOs (Activity, Application, Settlement, etc.)
  persona.ts                   # mock-tier — FixturePersona, selectPersona, NOW, date helpers
  polling.ts                   # DASHBOARD_POLLING tier constants (real product)
  index.ts                     # barrel: contracts/common + persona + polling

app/web/shared/src/features/dashboard/
  components/                  # agnostic UI primitives
    DashboardCard.tsx
    KpiTile.tsx
    MonthlyRevenueChart.tsx
    ChartTooltip.tsx
    ActivityFeed.tsx
    SectionGrid.tsx
    StripeConnectBanner.tsx
    ProfileHealthCard.tsx
    PersonaSwitcher.tsx        # dev-only
    WidgetState.tsx            # WidgetLoading / WidgetError / WidgetEmpty
    index.ts
  index.ts

app/web/venue/src/features/dashboard/
  VenueDashboardPage.tsx
  Venue*.tsx                   # 11 widget files
  dashboardApi.ts              # methods returning Promise<X>, currently fixture-backed
  types.ts                     # VenueDashboardOverview, VenueDashboardKpis
  hooks/
    useVenueOverview.ts        # one file per hook
    useVenueKpis.ts
    useVenueApplicationsToReview.ts
    useVenueInbox.ts
    useVenueUpcomingConcerts.ts
    useVenueTicketRevenue.ts
    useVenueOpenOpportunities.ts
    useVenueActivity.ts
    useVenueSettlements.ts
    index.ts
  fixtures/
    empty.ts mid.ts thriving.ts
    types.ts                   # VenueDashboardFixture
    index.ts                   # venueFixtures = { empty, mid, thriving }
  index.ts                     # exports { VenueDashboardPage }

app/web/artist/src/features/dashboard/  # same shape, artist-specific
```

## Open Phase A todo

✅ 1–10 all done. Phase A committed on `Feature/ManagerFrontPage` (commit `5fb54e96`) 2026-05-18.

11. **Phase A.8 — UX freeze.** Spin up dev server, hit `/_venue/` and `/_artist/` (both gated by auth — log in as a venue manager + artist; alternatively temporarily bypass guards in `_venue/route.tsx` / `_artist/route.tsx`). Toggle persona via `?persona=empty|mid|thriving` and check each visual state. Verify responsive collapse at tablet + mobile breakpoints.

## Session-2 deltas (2026-05-18, committed)

- **Applications widgets** redesigned: flat 3-column table (counterparty / status / actions) on **shadcn DataTable + `@tanstack/react-table`** (installed via `npm -w @concertable/web-* install @tanstack/react-table`). Status grouping replaced with column. No FE sort — BE will `ORDER BY` when the endpoint lands.
- **HATEOAS per-role `ApplicationActions`** — each SPA owns its own `applicationActions.ts` with `ApplicationActionName` union, `ApplicationActions` mapped type, and `APPLICATION_ACTION_LABELS` record. Mirrors `Concert.Api/Mappers/ApplicationResponseMapper.cs`.
- **`Application` type per SPA** (`app/web/{role}/src/features/dashboard/types.ts`) — nests shared `OpportunitySummary` + (venue) `ArtistSummary`. Drops `href` (actions are the only way to act), drops flat opportunity fields and `canAccept/canDecline/canCheckout` booleans.
- **`OpportunitySummary` + `OpportunityCard`** now carry structured `Contract`. New `contractSummary(contract)` helper at `app/shared/src/features/contracts/format.ts` registry-formats it (`flatFee` → "£N", `doorSplit` → "N% door", etc.). `ContractSummaryLabel.tsx` imports it.
- **`ActionLink`** primitive lives in `app/shared/src/types/common.ts` next to `Pagination<T>`. Removed duplicate from `features/concerts/types.ts`.
- **Reviews widgets** show recent excerpt list + aggregate header (was single-number tile). New shared `RecentReviewsList` primitive, new `useVenueRecentReviews` / `useArtistRecentReviews` hooks.
- **Page wrapper** dropped `max-w-7xl` for full-bleed. `DashboardCard` is `h-full` so cards in paired-row sections stretch to the row's tallest height.
- **Dashboard controller scope refined** — only owns aggregations (`overview`, `kpis`, `activity`). Plain list endpoints (`applications`, `inbox`, `upcoming-concerts`, `settlements`, `recommended-opportunities`) hit canonical resource controllers filtered to "me". Updates the round-trip plan in PLAN.md.

## Session-3 deltas (2026-05-18, committed `094fd4d4` → `23c8fc4c`)

### B.9 — `ConcertEntity.Period` (commit `094fd4d4`)

- `ConcertEntity` drops `DateTime StartDate` / `EndDate`, owns `DateRange Period` (mirrors `OpportunityEntity`).
- `OwnsOne(e => e.Period, p => { p.Property(x => x.Start).HasColumnName("StartDate"); p.Property(x => x.End).HasColumnName("EndDate"); })` keeps DB column names identical.
- `ConcertSearchModel` (Search module's read projection over the same `Concerts` table with `ExcludeFromMigrations`) is untouched — same columns, no edits needed.
- Existing `QueryableConcertMappers` already project from `c.Booking.Application.Opportunity.Period.Start/End` via the nav chain — no DTO mapper changes.
- **Migration re-scaffold (`./initial-migrations.ps1`) deferred** until end of Phase B code work. Column names unchanged → no schema drift while deferred.

### B.10 — Specification pattern locked at dual-method shape (commits `e2193f46` + `23c8fc4c`)

```csharp
public interface IUpcomingSpecification<TEntity> where TEntity : class, IHasDateRange
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query);

    IQueryable<TParent> ApplyExpression<TParent>(
        IQueryable<TParent> query,
        Expression<Func<TParent, TEntity>> navigation);
}
```

Both overloads return `IQueryable` — Expression never escapes the spec impl. Internally, both call `private Expression<Func<TEntity, bool>> BuildPredicate()` (one source of truth for the rule + one `TimeProvider` read).

- **`ApplyExpression` uses `Concertable.Shared.Infrastructure.Expressions.ExpressionExtensions.Substitute`** — the existing extension (built on `ParameterReplacer`) that rewrites a predicate's parameter onto a navigation expression's body. Don't introduce a new `Lift` extension — `Substitute` is more general (returns `TResult`, not just `bool`) and already exists.
- `IDateRangeSpecification<T>` is symmetric: `Apply(query, range)` + `ApplyExpression<TParent>(query, nav, range)`.
- DI registered as open-generics in `AddSharedInfrastructure`.
- Single consumer of `ApplyExpression` today: `ConcertDashboardRepository`'s Application filter (`a => a.Opportunity`).

### B.11 — KPI endpoint shape (commits `d4f9a3a6` + `a91c7271` + `23c8fc4c`)

- **One SQL round trip per persona**, anchored on `VenueReadModels` / `ArtistReadModels`, projecting three (venue) / two (artist) scalar subqueries through new `QueryableVenueDashboardMappers.ToVenueCounts` / `QueryableArtistDashboardMappers.ToArtistCounts`. Matches the `ConcertHeaderRepository.SearchAsync` / `QueryableConcertHeaderMappers.ToHeaderDtos` precedent — single composed `IQueryable`, single materialisation.
- **`IConcertDashboardRepository`** is a dedicated read-shape repo (separate from `ConcertRepository` / `OpportunityRepository` / `ApplicationRepository`) — mirrors `ConcertHeaderRepository` precedent in Search. Per-aggregate count methods on the existing repos were tried then reverted; the dedicated repo is the right home for dashboard-shaped reads.
- **Cross-module orchestration lives in `IVenueDashboardService` / `IArtistDashboardService`**, not in the controller. Resolves "me" via `IXService.GetIdForCurrentUserAsync`, calls `IConcertModule`, assembles wire DTO. `Task.WhenAll` of one task today; Payment slots into the second position when it lands.
- **Controllers are one-line delegates.** Return `NoContent` (204) when the service returns null DTO — read-model projection hasn't populated yet for that venue/artist. Honest about "you exist by auth, the data just isn't here yet" vs a real 404.
- **`Venue.Api.csproj` and `Artist.Api.csproj` no longer reference `Concert.Contracts`** — controllers don't touch Concert types anymore.

### Period semantics — locked (codified in PLAN.md → "Period semantics")

- **"Upcoming" (concerts)**: `Period.End > now` (includes in-progress). Via `IUpcomingSpecification<ConcertEntity>`.
- **"Open" (opportunities)**: `Period.Start >= now` (excludes in-progress). Inlined in `ConcertDashboardRepository`, NOT via spec — different rule than "upcoming" by design (open = still accepting apps, not "still happening").
- **"Still relevant" (applications)**: parent `Opportunity.Period.End > now`. Via `IUpcomingSpecification<OpportunityEntity>.ApplyExpression(a => a.Opportunity)`.

### Wire-shape stubs at merge time (TODOs in code)

The KPI DTO matches the FE wire shape verbatim, with three fields hard-stubbed at 0 because their dependencies aren't built yet. Each has a TODO at the literal pointing at the missing dependency:

- `MtdRevenueCents: 0` → `IManagerPaymentModule.GetVenueTicketRevenueMtdAsync` (not built)
- `MtdPayoutsCents: 0` → `IManagerPaymentModule.GetArtistPayoutsMtdAsync` (not built)
- `AcceptedAwaitingCheckout: 0` → `IConcertWorkflowCapabilityRegistry` / `IAcceptsCheckout` workflow lookup (lift `ApplicationResponseMapper.cs` per-application logic to an aggregate count)

### Phase A.8 still pending

A.8 UX freeze (browser eyeball + responsive pass) was not done this session. Independent of Phase B. Pick up whenever — see Phase A todo above.

## Things NOT to redo

- Don't put dashboardApi back in shared.
- Don't reintroduce `take` / `monthsBack` params on hooks.
- Don't rename `Overview` back to `Header`.
- Don't inline ChartTooltip content callbacks in chart components — use `<ChartTooltip>`.
- Don't duplicate `formatCurrency`.
- Don't add `mock` to the api filename or export — it's just `dashboardApi`.
- Don't sort applications on the FE — BE orders by date on the endpoint.
- Don't put `ApplicationListItem` back in shared — each SPA owns its own `Application` type with role-specific `actions` + counterparty (artist | venue).
- Don't reintroduce `contractLabel: string` — `OpportunitySummary.contract: Contract` is the canonical shape; format with `contractSummary()`.
- Don't put `href` back on `Application` — the row IS the view; act via `actions`.
- Don't duplicate `ActionLink` — single source at `shared/types/common.ts`.
- **Don't expose `Expression<Func<T, bool>>` on the spec interface.** Two methods (`Apply` + `ApplyExpression<TParent>`), both IQueryable-shaped. Expression stays inside the impl. The `Substitute` extension on the navigation does the lift.
- **Don't add per-aggregate dashboard count methods to `IConcertRepository` / `IOpportunityRepository` / `IApplicationRepository`.** Dashboard counts live in `IConcertDashboardRepository` as one composed projection — one SQL round trip. Tried per-aggregate, reverted.
- **Don't make `ConcertModule` inline EF queries for dashboard reads.** `feedback_no_ef_in_facade` — facade impls delegate to repos. `IConcertDashboardRepository` is the right home.
- **Don't put cross-module orchestration in the controller.** Controllers are thin delegates to `IXDashboardService`. The service owns `Task.WhenAll` of facade calls and assembles the wire DTO. Payment / future facades slot into the service without changing the controller.
- **Don't change `Apply` on the spec to return `Expression<Func<T, bool>>` or expose a `Predicate` property.** Both shapes (`Apply` direct + `ApplyExpression<TParent>` via nav) are IQueryable-in/out by design — keeps the abstraction honest about what consumers receive.
- **Don't add `IHasDateRangeExpression` or static-Expression members on entities for nav-lift convenience.** That's an anti-pattern — pretends `ApplicationEntity` is a range entity (it isn't), and pulls `System.Linq.Expressions` into Domain. The asymmetry is handled at the spec call site via `ApplyExpression(query, nav)`.
- **Don't return `NotFound` from the dashboard KPI endpoint** when the read-model row is missing. The user owns the venue/artist by authorization (`GetIdForCurrentUserAsync` would have thrown 403 otherwise) — the projection just hasn't populated yet. Use `NoContent` (204).
- **Don't rename `VenueDashboardCountsDto` to drop the `Dto` suffix.** Keep `Dto` on cross-module DTOs in `Concert.Contracts` (the user explicitly preferred this over the CLAUDE.md "drop suffix" guidance — Concert.Contracts has multiple `XxxDto` records and dropping for one creates inconsistency).
