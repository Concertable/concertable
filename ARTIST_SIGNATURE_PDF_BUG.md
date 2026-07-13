# Prompt: verify + regression-test the artist-signature-missing-from-PDF fix

> **Branch:** `Feature/BookingAgreement` (personal repo — plain `git`/`gh`, never the ADO/`ship`/
> `create-gh-pr` work skills). **Delete this file in the commit that completes the work.**

## What this is

A real, self-contained bug — **separate** from the "make the e-signature required" work already
committed (`c87d83e6`, `ec85766e`, `a62bba6d`). The booking-agreement PDF rendered at Accept is
missing the **artist's** signature line (`Signed by <artist>`); the venue's line is present.

## Status: the fix is ALREADY applied (uncommitted) — this is a VERIFY task, not an implement task

`Concert.Infrastructure/Services/BookingAgreementBuilder.cs` (~line 55) already reads:

```csharp
application.ArtistESignature with { },   // clone — was: application.ArtistESignature
```

Do **not** re-implement it. Confirm it's still there, understand why it's needed, verify it end to
end, and pin it with a regression test.

## Root cause

`BookingAgreementBuilder.BuildAsync` passed `application.ArtistESignature` — the **EF-tracked owned
instance still owned by `ApplicationEntity`** — straight into the new `BookingAgreementEntity`. EF then
tracks one owned `ESignature` under two owners and logs at accept:

> *The same entity is being tracked as different entity types
> `BookingAgreementEntity.ArtistESignature#ESignature` and
> `ApplicationEntity.ArtistESignature#ESignature` … which might not be the desired outcome.*

The artist owned entity is mis-persisted, so the agreement's copy comes out empty. `ESignature` is a
`record`, so `with { }` gives the agreement its own instance (mirrors the venue path, which already
built a fresh `ESignature`). A shared instance is the bug; note the shared **config**
(`ESignatureConfiguration`, committed in `a62bba6d`) does **not** fix a shared **instance** — the clone
is still required. Confirm the double-tracking warning is gone at accept after the clone.

## Why the integration suite doesn't catch it (and why a new test is needed)

`BookingAgreementApiTests.Agreement_Pdf_RendersBothPartyESignatures` passes even with the bug:
integration renders the PDF **lazily on download** (FakeBlobStorage reports the blob absent), reading a
**clean reload from SQL** — which is correct on disk. The E2E serves the **background-rendered-at-accept**
blob, produced from the mis-tracked in-memory graph — a different render path. Only the full-stack E2E
exposes it.

**Add an integration test that exercises the background-render path** (render immediately at accept from
the tracked graph, not lazily from a reload) and asserts the artist line is present — so this can regress
without needing the slow E2E. Look at how `IBookingAgreementPdfService.GenerateForBookingAsync` is invoked
in the accept background task vs. the lazy-download path.

## Verify (gates)

1. **Build** the Concert module + its two test projects (Windows: build test projects **serially** —
   parallel builds fight over shared `obj/` DLLs → false `CS2012`).
2. **Integration:** `Concertable.B2B.Concert.IntegrationTests` green, including the new background-render
   regression test.
3. **E2E (the reason this started):** the FlatFee UI scenario must go green with the artist signature
   present — `When … downloads the booking agreement` → `Then the agreement PDF is signed by
   "Artie Artist" and "Vera Venue"`. Run via the `e2e-ui-debug` skill / `./e2e.ps1 ui`; the
   `./docker-health.ps1` gate is mandatory. A suite that dies at startup is an environment problem — do
   not debug app code or reflex-rerun (see root `CLAUDE.md`).

## Note on the working tree

The tree also carries an in-flight fingerprint refactor (`…FingerprintComponent` → `…TermsSerializer`,
new `IContractTermsSerializer` + `ContractTermsSerializer` facade, DI renamed) that is **not** part of
this bug. Reconcile file-by-file; don't blanket-revert. The clone fix + a regression test are all this
task needs.
