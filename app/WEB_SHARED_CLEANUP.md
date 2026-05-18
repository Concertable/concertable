# `app/web/shared/` Cleanup — SPA-Specific Code Audit

Generated 2026-05-18. Investigation triggered by `ApplicationsPage.tsx`
hard-coding `useParams({ from: "/_venue/my/opportunities/$opportunityId/applications" })`
while sitting in `app/web/shared/` — leftover bleed from the prior
three-surface split (see [[project_three_surface_split]]). Most of `app/web/shared/`
is genuinely shared (UI primitives, auth glue, Stripe wrappers, etc.); this
list is **only** the files that single-SPA-belong but didn't get moved.

## Principle

A file in `app/web/shared/src/` belongs to a single SPA if **any** of:

1. It hard-codes a `/_venue/`, `/_artist/`, `/_customer/`, or `/_business/` route literal.
2. Only one SPA imports it (`app/web/{role}/src/...` is the sole consumer).
3. Its component / function name names a role (`VenueXxx`, `ArtistXxx`, `CustomerXxx`).

Move target: `app/web/{role}/src/features/<feature>/pages/` (or wherever the
SPA's existing feature folder lives).

## High-confidence moves (route literal evidence) — ✅ DONE

| File | Target SPA | Evidence |
|---|---|---|
| ~~`features/concerts/pages/ApplicationsPage.tsx`~~ | venue | route `/_venue/my/opportunities/$opportunityId/applications` |
| ~~`features/concerts/pages/AcceptApplicationPage.tsx`~~ | venue | route `/_venue/applications/$applicationId/accept` |
| ~~`features/concerts/pages/TicketCheckoutPage.tsx`~~ | customer | route `/_customer/concert/checkout/$id` |

## Medium-confidence moves (single-SPA import + role-specific UI) — ✅ DONE

| File | Target SPA | Evidence |
|---|---|---|
| ~~`features/venues/pages/CreateVenuePage.tsx`~~ | venue | imported only by venue SPA |
| ~~`features/venues/pages/MyVenuePage.tsx`~~ | venue | uses `OpportunitySection`; imported only by venue |
| ~~`features/artists/pages/CreateArtistPage.tsx`~~ | artist | imported only by artist SPA |
| ~~`features/artists/pages/MyArtistPage.tsx`~~ | artist | artist-specific UI; imported only by artist |
| ~~`features/customer/pages/PreferencesPage.tsx`~~ | customer | imported only by customer |
| ~~`features/customer/pages/CreatePreferencePage.tsx`~~ | customer | imported only by customer |
| ~~`features/search/pages/CustomerFindPage.tsx`~~ | customer | name + imported only by customer |
| ~~`features/search/pages/CustomerHomePage.tsx`~~ | customer | name + imported only by customer |
| ~~`features/search/pages/FindArtistPage.tsx`~~ | **venue** | name says "artist" but only `app/web/venue` imports it — venues search for artists |

**Route-file import sites left broken** — the SPA route files still import these
pages via `@/features/concerts` etc. (which alias-resolves to shared, where the
files no longer live). User indicated the route files are themselves being
moved out next, so the broken imports are tolerated for now.

## Ambiguous — needs human call

- **`features/concerts/pages/ApplicationCheckoutPage.tsx`** — used by both
  venue and artist. Likely needs to split into `VenueAcceptCheckoutPage` (in
  venue) and `ArtistApplyCheckoutPage` (in artist), since the two flows are
  semantically different (accept-side 3DS vs apply-side payment). Or keep one
  page and pass a `mode` prop from each SPA's route. Decide before moving.
- ~~**`features/search/pages/FindVenuePage.tsx`**~~ — original audit said both
  SPAs imported it; on re-check only `app/web/artist` does. Moved to artist
  SPA 2026-05-18.

## What's already moved (prior session)

For context — these were the same pattern, already addressed:

- `features/artists/pages/ArtistDashboardPage.tsx` → `app/web/artist/src/features/dashboard/ArtistDashboardPage.tsx` (deleted from shared)
- `features/venues/pages/VenueDashboardPage.tsx` → `app/web/venue/src/features/dashboard/VenueDashboardPage.tsx` (deleted from shared)

## Suggested order

1. **High-confidence first** — three files, mechanical move + import path
   update in the importing SPA. Will likely also drop some of the pre-existing
   route-typing build errors that flag `/_venue/...` as not assignable to the
   shared SPA's `RouterCore` (since the file's own route literal proves it
   doesn't belong in shared).
2. **Medium-confidence** — `Create*`/`My*`/`Customer*`/`FindArtist` pages, one
   commit per SPA or one bundled commit.
3. **`ApplicationCheckoutPage`** — design decision needed (split vs prop).
   Defer until after the easy wins.

## Out of scope

- Don't touch `components/ui/*` (shadcn primitives — always shared).
- Don't touch generic infra (axios client, Stripe wrapper, query provider,
  layout shells, route guards) — those are correctly shared.
- Don't refactor the moved files' internals — pure relocation + import path
  fix-up.
