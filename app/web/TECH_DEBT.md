# app/web — Technical Debt

---

## MED

### Web concert detail Buy Tickets below `@3xl` — fixed, narrow-viewport E2E outstanding

Fixed on `Fix/TechDebtSweep`: the single `ConcertCard` now reflows (full-width at the top below
`@3xl`, sticky sidebar at/above it) instead of being `display:none`, so `buy-tickets` is reachable at
every width and stays one unambiguous testid (Playwright strict mode stays happy). Outstanding only: a
**narrow-viewport E2E** asserting `buy-tickets` is reachable at a sub-`@3xl` width (needs Docker).

**Resolves when:** the narrow-viewport E2E scenario lands green.
