# B2B Payment saga producer plan

Next steps live in @plans/typed-result/B2B_PAYMENT_SAGA_PRODUCER_PROGRESS.md → `## Next Steps`.

Expand Payment's published surface and runtime so the B2B consumer can implement the authorized SEC1
financial lifecycle saga without a runtime reference to Payment. B2B sends only Payment-owned Contracts
commands; Payment executes capture, deposit, and refund inside its own runtime and publishes Payment-owned
outcome events. Infrastructure and cancellation faults remain exceptional. Expected financial refusals
are explicit contract outcomes, and retries replay the same outcome without moving money twice.

## Phases

- [x] **Phase 1 — exact Reunion producer baseline.** Consume the exact Reunion package artifact from
  producer commit `113be42f532d5d7e8daf1c362262ff7a7854b7bc`, including the flexible Option HTTP
  terminals. Resolve its same-commit dependency closure, record package versions and SHA-256 hashes,
  and use only temporary restore inputs. Do not copy or recreate Reunion extensions in Concertable.
- [x] **Phase 2 — additive Payment contracts.** Add Payment-owned capture, deposit, and refund command
  and outcome contracts carrying operation ID and booking correlation. Preserve every existing Client
  contract so Customer remains source-compatible.
- [x] **Phase 3 — idempotent Payment runtime.** Persist operation execution/replay state before remote
  money movement, make capture/deposit/refund booking-idempotent, handle commands in Payment.Web, and
  publish terminal or deferred outcomes through Payment's transactional outbox. Preserve invariant
  exceptions for impossible internal state.
- [ ] **Phase 4 — producer verification and artifacts.** Add focused unit, integration, architecture,
  and HTTP contract tests. Run Payment and repository builds, carve, formatting, package ownership,
  package pack/provenance, and plan graph gates. Commit each verified boundary, restore temporary
  Reunion inputs, and leave exact Payment package artifacts for the B2B consumer. Do not push, publish,
  open a PR, or merge.

## Package topology

- Producer layer: `Concertable.Payment.Contracts` owns the additive command/outcome wire surface;
  `Concertable.Payment.Client` republishes in the same package release without changing an existing
  public identity.
- Consumer layer: B2B consumes both packages by `PackageReference`. Customer consumes them too but
  needs no source migration because the new saga surface is additive.
- Delivery DAG: Payment producer → package publication → generated platform sync → B2B published-
  package revalidation. Local exact artifacts make the consumer delivery-ready, never merge-ready.

## Verification

- Payment and full API Release builds: 0 errors.
- Payment unit and integration suites plus architecture tests: green.
- Standalone service carve and package-ownership inventories: green.
- Re-scaffold Payment migrations through `api/initial-migrations.ps1` if the model changes.
- Scoped formatting and `git diff --check`: green.
- Exact package versions, source commit, hashes, and reproducible local artifact location recorded.
- Plan graph: 0 errors and 0 warnings.
- No local E2E; merge queue owns E2E unless a queue failure needs diagnosis.
