# Code review — Refactor/CamelCaseJsonEnumSurfaces

> **This file is a work order, not a discussion.** Fix open `[ ]` findings directly and report what
> changed. Tick `[x]` as landed.

**Reviewed up to commit:** `ec6eeaeeb0790a66cf852fa64bdfc7a64d4a47ab`  _(2026-08-20)_

> Range reviewed: full PR (`merge-base(main)..ec6eeaeeb`). The **consumer/sync half** of the camel-case
> JSON enum cut-over (producer #595 merged; FE packages republished under the `alpha` tag with camel-case).
> This flips the b2b **venue/artist** surfaces to camel-case wire values so they match the new backend +
> republished `@concertable/*` packages.

## Findings

No issues found. Checked:

- **Wire literals flipped, display labels preserved.** `VenueApplicationsWidget` / `ArtistApplicationsPipelineWidget`
  status maps use camel-case **keys** (`pending`, `accepted`, `confirmed`, `awaitingPayment`, `rejected`,
  `withdrawn`) while `label:` keeps human text ("Pending", …) — the "labels derive from the enum, never
  re-type wire values" convention.
- **Comparison sites flipped in lockstep** — `ApplicationCard` (`status === "pending"`),
  `VenueAcceptCheckoutPage` (`status === "accepted"`) — the two the #595 native pass flagged as the
  deferred surface's app-shell breakers; both now match the camel-case union.
- **TenantType routes** — `useTenant("venue")`, `TenantChooser`/`TenantSwitcher tenantType="venue"`,
  `resolveTenantRoute("venue")` (and artist equivalents) flipped to the camel-case TenantType wire value.
- **Fixtures** (empty/mid/thriving, venue+artist) flipped to camel-case status/genre/paymentMethod; the
  `main`-merge conflict in `thriving.ts` resolved to camel-case values + `main`'s current
  `/api/application/...` route casing.
- **No leftover PascalCase enum literals** in `app/web/b2b/{venue,artist,business}` (grep-clean).
- **No pin bump needed** — `carve-fe` rewrites `@concertable/*` to the `alpha` dist-tag, which now
  carries the camel-case packages from #595's publish.
- **No security paths** touched (frontend-only; no `*.Contracts`/Payment/Controller) — no security marker.

Full carve-fe + typecheck validated on draft-PR CI (local monorepo install was too heavy to run here).
