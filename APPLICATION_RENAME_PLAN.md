# Application / Booking Rename Plan

Deferred refactor. To run **after** the Stripe `<PaymentElement>` payment flow
lands — touching too many surfaces in parallel will create rebase pain.

## Why

`OpportunityApplication` and `ConcertBooking` carry redundant prefixes that
made sense in the pre-modular monolith but don't anymore. Now that
`Concert` is its own bounded context with its own DbContext, the namespace
already disambiguates — the type-name prefix is just noise.

| Current | Proposed | Reason |
| --- | --- | --- |
| `OpportunityApplicationEntity` | `ApplicationEntity` | Inside `Concert.Domain`; "Application" is unambiguous in that context. |
| `ConcertBookingEntity` | `BookingEntity` | Every booking is a concert booking; the `Concert` prefix is the namespace's job. |
| `OpportunityApplicationService` | `ApplicationService` | Same rationale. |
| `IOpportunityApplicationService` / `Validator` | `IApplicationService` / `IApplicationValidator` | Same. |
| `OpportunityApplicationController` | `ApplicationController` | Route changes from `/api/opportunityapplication/...` → `/api/application/...`. |
| `OpportunityApplicationDtos.cs` | `ApplicationDtos.cs` | File + record names. |
| `OpportunityApplicationEntityConfiguration` | `ApplicationEntityConfiguration` | Same. |
| Frontend `applicationApi` | unchanged (`applicationApi`) — just URL paths flip | Already shorter on the FE. |

The Application↔Booking lifecycle distinction stays — these are still two
separate aggregates. Only the prefixes go.

## Scope

### Backend

- Rename entity types in `Concert.Domain` + their EF configurations.
- Rename services / interfaces / validators in `Concert.Application` and
  `Concert.Infrastructure`.
- Rename controller + route base path.
- Rename DTOs / request / response types.
- Rename `Concert.Infrastructure/Services/Acceptance/*` references.
- Update `IConcertModule` facade if any method names mention
  `OpportunityApplication` (most don't — internal types).
- Re-scaffold `Concert` migrations (per project rule: nuke + regenerate
  `InitialCreate` rather than additive migrations). Table names will flip
  `OpportunityApplications` → `Applications`, `ConcertBookings` → `Bookings`.
  Verify no FK collisions across modules (none expected — Concert tables are
  module-internal).

### Frontend

- `app/src/types/application.ts` — already neutral.
- `app/src/api/applicationApi.ts` — flip URL constants only.
- Route file paths under `app/src/routes/...checkout.$applicationId.tsx`
  stay; the resource name in the path string changes.
- `useApply` / `useApplicationQuery` — names already neutral, no rename.
- Page files that hard-code `/opportunityapplication/` URLs — search and
  replace.
- TanStack route paths that mention `opportunityapplication` — flip.

### Tests

- Integration tests under `Concertable.Web.IntegrationTests` — search/replace
  type names + URL paths.
- E2E tests under `Concertable.E2ETests` — same.
- Unit tests under `Concertable.Concert.UnitTests` (or trio retirement
  successors) — same.

### Out of scope for this pass

- Database column names that don't follow the table-name rename
  (e.g. foreign-key columns named `ApplicationId` already).
- The Application API project rename (still `Concert.Api` — the controller
  inside it gets renamed, not the assembly).
- Domain event names like `ApplicationAcceptedEvent` if they exist —
  re-evaluate once entity rename lands; may also need flipping.

## Sequencing

1. ✅ Stripe `<PaymentElement>` migration lands first — see payment-flow
   notes elsewhere.
2. ⬜ Run this rename in one bundled PR. Per project memory, refactors in
   this area are preferred as a single bundled PR rather than split.
3. ⬜ Re-scaffold migrations in dependency order
   (`Shared → Identity → Artist → Venue → Concert → Contract → AppDb`).
4. ⬜ Verify integration + E2E green.

## Risk

- Touches every controller + service + DTO file in `Concert` — wide blast
  radius but mechanical.
- Migration re-scaffold means anyone with local seeded data has to re-seed.
- Frontend route paths shift — bookmarked URLs from before the rename
  break (acceptable; non-public API).
