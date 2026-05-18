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
