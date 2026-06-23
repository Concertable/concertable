# app/web — Technical Debt

---

## MED

### Web concert detail has no Buy Tickets affordance below the `@3xl` container width

The only `buy-tickets` control on the web concert detail page is the sidebar `ConcertCard`, rendered inside `<div className="hidden w-72 shrink-0 @3xl:block">` in `app/web/shared/src/features/concerts/components/ConcertDetails.tsx`. Below the `@3xl` container breakpoint (48rem) — a browser window roughly narrower than ~800px — that wrapper is `display:none` and there is **no fallback**, so a customer on a narrow window literally cannot buy a ticket. The mobile `ConcertDetails` (`app/mobile/shared/src/features/concerts/components/ConcertDetails.tsx`) handles this with a sticky bottom buy bar; the web version has no equivalent. The same gate is a silent test footgun: every concert-detail UI E2E scenario fails with a 30s `GetByTestId("buy-tickets")` timeout the instant the test viewport drops below that width (e.g. when adding mobile-width coverage) — it reads as a backend/data failure but is pure CSS layout.

**Resolves when:** the web `ConcertDetails` exposes a buy affordance visible at every width (e.g. a sticky bottom bar mirroring mobile, or the buy button promoted out of the `@3xl`-only sidebar), with at least one narrow-viewport E2E asserting `buy-tickets` is reachable.
