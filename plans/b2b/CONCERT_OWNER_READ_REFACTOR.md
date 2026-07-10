# Owner vs public concert read — architecture refactor

> Working doc for a design decision + refactor on branch `Feature/BookingAgreement` (PR #90, **not
> merged**). Delete this file when the refactor lands. This is a **read-architecture** decision that
> reaches beyond the booking-agreement feature (venue/opportunities, concert/agreement, any future
> owner-only sub-resource) — decide the pattern deliberately, then apply it.

## What prompted this

The booking-agreement download button needed a home. An accepted deal is a **concert** for both
parties, so the download belongs on the concert detail page. Wiring it there exposed a latent
modelling problem, and a fix (`b589b42b`) that has a real flaw.

## Accurate current state (verified — do not assume, this doc corrects earlier mistakes)

- **The my-concert page reads a PUBLIC, non-tenant-scoped endpoint.** FE `useMyConcert(id)` →
  `useConcert(id)` → `GET /concert/{id}` → `ConcertController.GetDetailsById` →
  `concertService.GetDetailsByIdAsync` → **`IPublicConcertRepository`** (`PublicConcertDbContext`, **no
  tenant filter**). The marketplace `/find/concert/$id` uses the same endpoint. So the my-concert read
  is not owner-scoped at all.
- The only tenant-scoped concert detail read is `ConcertRepository.GetDetailsByApplicationIdAsync`
  (via `GET /concert/application/{applicationId}`), used elsewhere — not by the my-concert page.
- **`b589b42b` put owner affordances on the SHARED public response, gated only on state.**
  `ConcertResponseMappers.ToDetailsResponse` emits `Actions.Agreement` (and, pre-existing,
  `Actions.Cancel`) when `dto.State == Booked`. `State` is on every concert, and the response is
  served by the public repo → **the agreement/cancel links appear on the public/marketplace response
  to non-owners** (cosmetic only — the endpoints below are party-gated — but wrong for a legal-doc
  affordance). This is the flaw to fix.
- **Download endpoints (both party-scoped, 404 for non-parties — keep both):**
  `GET /api/Application/{id}/agreement/pdf` (Phase 3) and `GET /api/Concert/{id}/agreement/pdf`
  (`b589b42b`, resolves concert→booking→agreement via the tenant-filtered `BookingAgreementRepository`).
  The PDF is a binary download triggered on click — necessarily its own request; that part is fine.
- **`PublicConcertDbContext` has no `BookingAgreements` table** (so the shared read projection
  `QueryableConcertMappers.ToDetails` cannot query agreement existence — this is why an earlier attempt
  to thread an `agreementBookingIds` param through `ToDetails` was a dead end; that attempt was fully
  reverted, `ToDetails` is back to its original 4-parameter form).
- **`[TenantPersona(TenantType.Venue)]` on `ConcertController` does NOT block artists.** It is only
  the default persona for `[HasPermission]` checks; `GetDetailsById` has no `[HasPermission]`, which is
  why artists read concerts fine. A new owner endpoint without `[HasPermission]` is callable by both
  parties; the tenant-scoped repository (null → 404) is the real gate.
- **`useMyConcert` lives in `app/shared`** (every SPA, incl. customer) but is a **manager-only**
  concept. It only works there today because `/concert/{id}` happens to exist on every backend. A
  B2B-only owner endpoint (e.g. `/concert/user/{id}`) must NOT be called from `app/shared` (boundary
  rule in `app/web/shared/CLAUDE.md`). `useMyConcert` should move to `app/web/b2b/shared`.

## Venue precedent (the codebase's actual pattern — also corrects an earlier mistake)

Venue does **not** use two DTOs. `GET /venue/{id}` (public) and `GET /venue/user` (owner) **both
return the same `VenueDetailsResponse`**; the `/user` endpoint resolves *which* venue by tenant and
adds `[HasPermission]`. Owner sub-data (opportunities) is a **separate endpoint**
(`/api/Venue/{venueId}/opportunities`), not embedded.

So the established pattern is: **one shared detail DTO, an owner endpoint that resolves identity +
permission, and owner sub-resources as their own endpoints.** Whether that separate-sub-resource
choice is right *in general* (vs a richer owner response in one call) is part of what to decide here —
it recurs for venue/opportunities and concert/agreement alike.

**Venue is NOT broken and is out of scope.** Verified: `VenueDetailsResponse` has **no `Actions` /
`ActionLink` field** — it carries zero owner-only data, which is exactly why public + owner can share
it with no leak. Owner affordances are composed FE-side in the manager app. **Concert is the outlier**
that (uniquely) put `Actions` on its shared response — that is the whole bug. Do not refactor venue's
read; align concert *to* venue. The separate opportunities endpoint is a legitimate REST sub-resource
(its own auth, leaks nothing) — the embed-vs-separate point is a **forward-looking consistency
convention to settle**, not a required venue change now.

## The decision to make

How should an owner's view of a concert differ from the public marketplace view, such that owner
affordances (cancel, agreement download link) reach **only** the two parties, with no leak and no
`bool owned` magic flag on a shared mapper? Candidate shapes (pick one, or propose better):

1. **Venue-style: shared DTO + owner endpoint + separate sub-resource endpoints.** Owner concert read
   = a tenant-scoped endpoint (404 for non-parties); reaching it means you're a party, so it can emit
   the action links unconditionally. Public `/concert/{id}` emits **no** action links. Agreement stays
   its own download endpoint. Consistent with venue. Cost: the public `ConcertDetailsResponse` loses
   `Actions`; a small owner mapper/endpoint is added.
2. **Two distinct responses** (public marketplace vs owner) — cleaner separation, more types.
3. **Rich owner response embedding owner sub-data in one call** — the "one optimized call" idea;
   weigh against resource-boundary/caching and the union-type warning in `api/CLAUDE.md`.

Whatever is chosen, also decide the **general** rule (embed owner sub-resources vs separate endpoints)
so venue/opportunities and concert/agreement follow the same convention.

## Constraints / must-preserve

- **Keep the v2 simplification (`5867f8c1`)** — dropped write-only agreement columns + keyed
  fingerprint. Not in scope to touch.
- The agreement download must **work for both parties** from the concert detail page. One call to
  load the page (link included); the PDF is a separate on-click request (fine).
- **No owner affordance may appear on the public/marketplace response.** Fix the same latent leak for
  `Cancel` in the same pass.
- **No `bool owned` flag** on a shared mapper; **no union DTO** with always-null owner fields
  (`api/CLAUDE.md`: model the intersection, not the union).
- **Respect the web boundary**: B2B-only endpoints/calls belong in `app/web/b2b/shared`, never
  `app/shared`. Move `useMyConcert` to `b2b/shared` as part of this.
- Keep **both** agreement download endpoints (application- and concert-scoped) — both are valid APIs.
- `b589b42b`'s state-gated action links on the shared response are to be reworked per the chosen shape.

## Verification gate

- `dotnet build api/Concertable.slnx` green.
- Concert integration via the `integration-debug` skill (agreement download by both parties + 404 for
  a stranger; add a concert-owner-read test for the chosen shape).
- Four web builds green (boundary gate — the `useMyConcert` move + any shared-type change touches all).
- `./initial-migrations.ps1` only if the model changes (this is read/response shaping — likely not).
- UI E2E regress via `e2e-ui-regress` (the FlatFee scenario already downloads the agreement on the
  concert page — keep it green through the refactor).
