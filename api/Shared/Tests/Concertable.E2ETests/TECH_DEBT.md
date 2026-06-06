# Concertable E2E — Technical Debt

When an item is fixed, update this file.

---

## HIGH

### API E2E suites are outside the regress contract; UI scenarios stop at draft creation

Two facts compound into a coverage hole over the back half of the concert lifecycle:

1. [`E2E_BASELINE.md`](./E2E_BASELINE.md) + `./e2e.ps1 regress` track **Reqnroll UI scenarios only**. The four contract workflow features (`FlatFee`/`DoorSplit`/`VenueHire`/`VersusWorkflow.feature`) all end at draft creation + payment-card variants. No scenario posts a concert to the marketplace, finishes one, or settles a deferred contract — so "E2E green" never said anything about post → finish → settle → payout.
2. The API E2E tests that **do** cover that tail — `Concertable.B2B.E2ETests/Payments/ConcertDraftTests.cs` + `ConcertFinishedTests.cs` and `Concertable.Customer.E2ETests/Payments/TicketPurchaseTests.cs` — live in separate xUnit assemblies that no baseline, regress script, or merge gate runs. Unrun tests rot: at current HEAD the DoorSplit/Versus `ConcertFinishedTests` cannot pass (finish dispatches on `concert.ContractType`, which production never persisted), and nothing noticed.

Net effect: until the per-contract integration tests added 2026-06-06 (`Concertable.B2B.Concert.IntegrationTests/Concert/Concert*ApiTests.cs`), the post-accept lifecycle had **zero enforced coverage at any test level**. Three production bugs hid there: bookings and concert drafts persisted with `ContractType = 0` (FlatFee) so finish/settle dispatched the wrong workflow, and the deferred-contract stage sequence blocked booking settlement outright.

**Resolves when:**

- The API E2E suites join an enforced run contract: either `E2E_BASELINE.md` grows an Api-suite section that `./e2e.ps1 regress` executes (fully-qualified xUnit test names alongside the scenario names), or a parallel api-e2e gate sits in the same merge checklist as the UI regress.
- `ConcertFinishedTests` asserts the per-contract *outcome*, not just intermediate artifacts — for DoorSplit/Versus that means booking `Complete` after the settlement webhook round-trip, not merely that a settlement payment intent exists.
- Until both hold, the integration-level `Concert*ApiTests` are the only finish/settle gate — keep them green.
