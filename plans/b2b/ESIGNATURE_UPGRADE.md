# E-signature upgrade — click-wrap → self-hosted named signature

> **Branch:** `Feature/BookingAgreement` (stay on it — this extends the booking-agreement feature
> already in flight; the click-wrap code being upgraded only lives on this branch, so per root
> `CLAUDE.md` the upgrade is part of this feature, not a new branch).
>
> **What & why:** the booking agreement currently binds each party via **click-wrap** (a single
> "I agree to the contract terms." tickbox) + an audit record (`Consent`: user, timestamp, IP, UA)
> + an immutable terms snapshot + the terms-fingerprint guard. That is legally binding and
> market-standard for a self-serve marketplace — but it is **Tier 1 (simple) e-signature**, and it
> does not visibly read as *signing*. This plan upgrades it to a **self-hosted, named e-signature**
> (typed full name, optional drawn signature, explicit intent) rendered into the agreement PDF —
> **Advanced-tier**, no third-party dependency, no per-signature cost. It also renames the backend
> `Consent` vocabulary to `ESignature` so the front-end and back-end names coincide.
>
> **Companion docs:** [LAUNCH_PLAN.md](./LAUNCH_PLAN.md) (this is a scope change to its "Tier 1
> click-wrap only" decision — see §Scope change below),
> [LEGAL_REQUIREMENTS.md](../../api/Concertable.B2B/src/Modules/Contract/LEGAL_REQUIREMENTS.md) item 2.

---

## Why this, and why not DocuSign — the market evidence

Researched 2026-07-11. The gig/booking market splits by **deal shape**, not by budget:

- **Self-serve booking marketplaces** (our peer group — GigXchange, GigSalad, Encore, Poptop,
  Muzeek): the accepted terms **are** the binding contract, signed by **click-to-accept**.
  GigXchange brands theirs "**e-signed** digital contract, signed in a minute" — same family,
  nicer word. None put a DocuSign envelope in the booking flow.
- **Agency / venue deal-desks** (Gigwell → Dropbox Sign; Prism.fm, Opendate): bolt on **third-party
  e-sign** because their contracts are negotiated, redlined, multi-party, five-figure.

Contract-law consensus (Ironclad et al.): clickwrap is the *most-enforced* online agreement (~70%
court success vs 14% browsewrap); clickwrap and e-sign are equally binding under UK eIDAS / ESIGN /
UETA. The guidance: **clickwrap for high-volume standardized agreements; e-signatures for complex
negotiated ones.** Concertable is standardized/high-volume → clickwrap is *correct*, not a shortcut.

**So why upgrade at all?** Two product reasons, both legitimate:
1. **Positioning parity** with our closest competitor (GigXchange markets "e-signed") — matters for
   the differentiation thesis.
2. **Stronger attribution/evidence** — a typed/drawn name proves a *specific human* signed, moving
   us Simple → Advanced tier.

**Why not a third party (DocuSign / Dropbox Sign):** per-signature cost on *every* booking wrecks
marketplace unit economics at ~5% commission, and we'd pay for negotiation/version-control features
our fixed-template flow never uses. Self-hosted capture gets the product win without either cost.

> **Not legal advice.** The signature posture (and the disclosed-agent model) is exactly what to
> confirm with the solicitor already engaged for the T&Cs (Swim-lane A). This plan is engineering;
> it does not pre-empt that review.

---

## Scope change vs LAUNCH_PLAN

LAUNCH_PLAN currently records the shipped booking agreement as **"Tier 1 click-wrap only (Tier 2
drawn/DocuSign out of scope)."** This plan moves the **self-hosted named/drawn** slice of Tier 2
*into* scope (third-party/DocuSign stays out). When this plan completes, update that line to reflect
"Advanced-tier self-hosted e-signature (typed name + optional drawn), no third party."

---

## What stays exactly as-is (do not touch)

- **`TermsFingerprint`** and the `VerifyTermsUnchanged` guard — backend-only integrity, no FE
  counterpart, so it keeps its name (the "rename only where names coincide" rule). The signature
  upgrade does not change the mid-flight-edit protection.
- **The immutable terms snapshot** (`BookingAgreementEntity` columns, `AgreementTermsRenderer`,
  the survives-contract-edit behaviour). We are adding *how the party signs*, not changing *what is
  frozen*.
- **`BookingAgreementEntity`** keeps its name — it *is* the e-signed agreement.
- **Both money flows / escrow / the four contract workflows** — untouched.

## The four consent entry points (all must move to the new signature step)

The signature act is captured at four FE sites today, all rendering the shared checkbox:

| Party / flow | File | Pays here? |
|---|---|---|
| Artist, simple apply (FlatFee/DoorSplit/Versus) | `app/web/b2b/artist/.../components/ApplyAction.tsx` | no |
| Artist, paid apply (VenueHire) | `app/web/b2b/artist/.../pages/ArtistApplyCheckoutPage.tsx` | yes |
| Venue, simple accept (VenueHire) | `app/web/b2b/venue/.../pages/AcceptApplicationPage.tsx` | no |
| Venue, paid accept (FlatFee/DoorSplit/Versus) | `app/web/b2b/venue/.../pages/VenueAcceptCheckoutPage.tsx` | yes |

Shared widget: `app/web/b2b/shared/.../components/applications/AgreeToTermsCheckbox.tsx`.
Backend records: `ApplyExecutor` (artist) + `AcceptExecutor`/agreement builder (venue).

---

## Phases (each independently shippable, each ends green)

### Phase 1 — Model + vocabulary: `Consent` → `ESignature`, add named signature ✅ SHIPPED

Backend only; no UI yet (the new fields accept nulls until Phase 3 populates them, so this phase is
a safe expand).

> **Done** (build 0 err · Concert unit 56/56 · Concert integration 104/104 B2B + 2/2 Customer ·
> migrations re-scaffolded). `Consent`→`ESignature` value object (+`SignatoryName` required,
> `DrawnSignatureImage` optional); `ArtistESignature`/`VenueESignature` on both entities;
> `ESignatureRequest` (client's half) threaded controller→service→dispatcher→executor→builder; the
> server still stamps user/time/IP/UA. `agreedToTerms` bool replaced by the `eSignature` object on
> apply/accept requests + validators. PDF section untouched beyond the rename (Phase 2 rewrites it).

- Rename the domain value object `Consent` → **`ESignature`**
  (`api/.../Concert.Domain/Entities/Consent.cs`). New shape:
  ```
  ESignature(Guid UserId, DateTime AtUtc, string? Ip, string? UserAgent,
             string SignatoryName, string? DrawnSignatureImage)
  ```
  - `SignatoryName` — required (the typed full name); the core of Advanced-tier attribution.
  - `DrawnSignatureImage` — nullable base64 PNG from the canvas (optional; name-only is valid).
    (MVP: inline column. If size becomes a problem, move to blob storage like the PDF — log in
    `api/.../Concert/TECH_DEBT.md`, don't pre-build it.)
- Rename `ApplicationEntity.ArtistConsent` → `ArtistESignature`,
  `RecordArtistConsent(...)` → `RecordArtistESignature(...)` (keeps stamping `TermsFingerprint`).
- Rename `BookingAgreementEntity.ArtistConsent`/`VenueConsent` →
  `ArtistESignature`/`VenueESignature`; update `Create(...)` params + the builder.
- API request field: replace the bare `agreedToTerms: bool` on the apply/accept requests with an
  **`eSignature` object** (`ESignatureRequest { string SignatoryName; string? DrawnSignatureImage }`);
  its presence *is* the consent. The controller/executor builds the `ESignature` value from the
  request **+ ambient context** (`ICurrentUser.Id`, `IClientContext.IpAddress/UserAgent`,
  `TimeProvider.GetUtcNow()`) — never trusting client-supplied identity/time.
- Legacy/pre-consent applications: `ArtistESignature` stays nullable (predates click-wrap); the
  null-fingerprint accept path is unchanged.
- Re-scaffold migrations: `./initial-migrations.ps1` from `api/` (renamed + new owned-type columns).
- **Gate:** `dotnet build api/Concertable.slnx` green · B2B Concert unit + integration tests via
  `integration-debug` (update the `{ agreedToTerms = true }` test payloads to the new object;
  assert `SignatoryName` persists on both parties' signatures). No E2E this phase.

### Phase 2 — PDF signature block ✅ SHIPPED

> **Done** (build 0 err · Concert unit 56/56 · BookingAgreement integration 15/15 incl. `%PDF`
> assertions). "Consent" section → "Signatures": per party "Signed by **{name}**", the drawn image
> if present (base64/data-URI decoded, corrupt → skipped not fatal), then `{ts} · user {id} · IP`.
> Legacy null case → "No recorded signature (predates e-sign)". Footer copy click-wrapped → e-signed.

- `BookingAgreementDocument` (`api/.../Concert.Infrastructure/Pdf/`): replace the "Consent" section
  with a **"Signatures"** section — for each party render: "Signed by **{SignatoryName}**", the
  drawn-signature image if present, then `{timestamp} · user {id} · IP {ip}`. Keep the honest
  null-case ("No recorded signature (predates e-sign)") for legacy agreements.
- Update the footer copy: "records the terms both parties click-wrapped" → "records the terms both
  parties **e-signed**".
- **Gate:** build green · Concert unit tests (PDF still renders, magic-number/`%PDF` assertions
  hold). No E2E this phase (PDF content is integration/unit-covered).

### Phase 3 — Front-end signature step + conspicuous terms

- Replace the shared `AgreeToTermsCheckbox` with an **`ESignaturePanel`** (rename the component +
  its `data-testid` `agree-to-terms` → `e-sign`). The panel shows:
  1. The **full binding terms** conspicuously at the point of signing — not just the fee summary,
     but cancellation + liability too (the single biggest enforceability lever per the research;
     courts want the actual terms accessible at the moment of assent). Reuse/extend
     `AcceptContractSummary`; link the platform T&Cs.
  2. A **typed full-name** input (required; enables the confirm/pay button — replaces the current
     `agreed` boolean gate).
  3. An **optional drawn-signature canvas** (the "feels like signing" bit).
  4. An explicit intent line: "By signing, I agree to and e-sign this booking agreement."
  - Never pre-check/pre-fill intent (enforceability best practice).
- Wire the new `eSignature` payload through the apply/accept API calls in all **four** entry points
  above. The button stays disabled until name is entered (and, where relevant, card authorised).
- **Gate:** all four web builds green (`web-venue`, `web-artist`, `web-customer`, `web-business`) —
  the boundary gate. Regenerate route trees only if routes changed (they don't).

### Phase 4 — E2E, docs, close-out (final)

- Update UI E2E page objects + steps: `AcceptApplicationPage`/`ApplyCheckoutPage` page objects and
  `VenueManagerSteps` — `agree-to-terms` → `e-sign`, fill the signatory name in `AgreeAndConfirmAsync`
  / `GotoCheckoutAndAgreeAsync`. The `FlatFeeWorkflow.feature` "booking agreement is downloadable"
  scenario should still pass end-to-end.
- **Run UI E2E** via `e2e-ui-debug` — this phase flips behaviour on a covered flow (apply/accept →
  payment → booking + agreement), so it meets the massive/risky bar in `plans/CLAUDE.md`.
- Update LAUNCH_PLAN's booking-agreement line (Tier-1 → Advanced-tier self-hosted e-sign; see
  §Scope change).
- `git rm` this plan in the final commit (its work is done).
- **Gate:** UI E2E green.

---

## Open questions / decisions to confirm before/while building

- **Drawn signature required or optional?** Plan assumes typed name **required**, drawn **optional**
  (name-only is a valid Advanced-tier signature; drawn adds perceived weight). Flip to required if
  the solicitor wants it.
- **Where to store the drawn image** — inline column (MVP default) vs blob storage (like the PDF).
  Start inline; revisit only if row size bites.
- **Solicitor sign-off** on the Advanced-tier posture + disclosed-agent model (Swim-lane A) — not a
  build blocker, but must land before launch.
