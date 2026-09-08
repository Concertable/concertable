# Code review — Refactor/M1-Owner-Hosting-Sync

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `d71b0200fc4e500232c3ba0d17706d45bf1e0892`  `(2026-09-08)`
**Judgment:** `approved`

## Review pass — 2026-09-08 — full

**Candidate base:** `21760e777ecc6df0680bd846e8f814f035627ac4`
**Candidate head:** `d71b0200fc4e500232c3ba0d17706d45bf1e0892`
**Candidate branch:** `Refactor/M1-Owner-Hosting-Sync`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:e145218a4e1083ba65f834e87c70b9cc4319b39607d95137a7a118349d742337` `(13 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable\2f434374-df78-46b7-bff7-23badf4c4dcf\scratchpad\review-bundle-M1-P2`
**Candidate bundle identity:** `sha256:b118a67ea800e2281854fe5d9897b292bc38bc5aef18618306c64cef1d8cf89c`
**Work-order path:** `reviews/Refactor-M1-Owner-Hosting-Sync.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

This is a fresh frozen watermark for the landed-base candidate, not an extension of the earlier
`reviews/Refactor-M1-Platform-Contract.md` work order: that artifact exists on no branch and in no surviving
checkout. The candidate is the eight preserved staged commits plus one added commit that advances the Payment
consumer pin.

Lenses applied: extraction fidelity against the pre-extraction platform code, service-boundary ownership,
fail-closed behaviour of the new Auth client restriction, published-package closure, and legacy-contract
retention.

### Findings

- [x] **P2-1 — HIGH — published-package closure** — `api/Concertable.B2B/Directory.Packages.props:7`
  B2B and Customer could not compile their deployable closures from the feed. `538bbc568` added
  `IEscrowOperationsClient.AuthorizeAsync` to Payment's client and switched B2B's `ApplicationCheckoutService`
  onto it in the same commit, while both consumers pin Payment through `ConcertablePaymentVersion` at
  `0.1.0-alpha.0.1322`, which predates the method. Verified rather than inferred: the cached `1322`
  `Concertable.Payment.Client` assembly contains no `AuthorizeAsync`, and `Publish images` has failed on main
  since `516f4cc25` with CS1061 on exactly that call for both `B2B.Web` and `B2B.Workers`. The CI compile
  floor hid it because `local-platform.ps1` builds Payment from source, so image publication was the only
  gate exercising the real feed pin. Fixed by advancing both consumers to `0.1.0-alpha.0.1330`, whose publish
  commit `32fc63edc` carries the method.

No other findings. The following were examined and are clean:

- **Surface roster extraction is exact.** `B2BLocalSpaSurfaces.All` reproduces platform
  `LocalSpaSurfaces.B2B` as Venue 5175, Artist 5176, Business 5177, Admin 5178 in the same order, so each
  B2B host's `Cors__AllowedOrigins__*` roster is unchanged and no origin is widened.
  `CustomerLocalSpaSurfaces.Customer` reproduces Customer 5174. The owner `AuthClients` rosters reproduce the
  platform's authenticated subsets: B2B carries Venue, Artist and Admin and omits Business, which had a null
  `AuthClient`.
- **The Customer mobile surface's duplicated argument is faithful, not a slip.** `AddMobileCustomer` passes
  `customerWeb` as both `api` and `customerWeb`, which reads like a copy-paste defect; the pre-extraction call
  `AddMobileSurface(builder, customerWeb, auth, tunnel, lanIp, "customer", customerWeb: customerWeb,
  paymentWeb: paymentWeb)` did the same, so `EXPO_PUBLIC_API_URL` and `EXPO_PUBLIC_CUSTOMER_API_URL` already
  resolved to one resource.
- **The Auth client restriction is fail-closed and covered.** `RestrictToEnabledClients` defaults to absent
  and preserves the bundled roster; an unknown name throws rather than being silently dropped; an explicitly
  empty roster yields zero clients. `WithSpaClients` clears every inherited `Auth__SpaClients__` key before
  writing, so a stale registration cannot survive replacement. Architecture tests cover replacement, the four
  enable-subset cases, the unknown-client throw, and the absent-restriction default.
- **Legacy contracts are retained deliberately.** `WithLocalSpaCorsOrigins` and the platform
  `Concertable.Frontend.Hosting` hosting paths remain with no surviving callers. That is this stage's stated
  compatibility boundary; Platform Contract PR #945 retires them.
- **Staged-but-unconsumed owner API is the planned seam.** The owner `AuthClients` rosters, `WithSpaClients`
  and `WithMobilePublicUrl` have no production call site in this stage; system composition consumes the
  combined roster in the later stages.

### Validation

- Auth AppHost, Auth Hosting and the Auth architecture-test project build clean.
- All four image-rail projects build package-clean from the committed feed pins after the Payment pin
  advance: `B2B.Web`, `B2B.Workers`, `B2B.Seed.Simulator` and `Customer.Web`.
- `B2B.Application.Infrastructure`, the project that fails on main, builds clean at the advanced pin.
- Remote CI, the merge queue and `Publish images` remain the authoritative gates for this candidate.
