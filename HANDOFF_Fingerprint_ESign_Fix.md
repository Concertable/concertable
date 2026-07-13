# Handoff — finish the BookingAgreement fingerprint + e-signature work on `Feature/BookingAgreement`

You are picking up mid-stream on branch **`Feature/BookingAgreement`** (personal repo, GitHub only —
plain `git`/`gh`, **never** the ADO/`ship`/`create-gh-pr` work skills). The goal: **land all of the
below in this branch, verified.** Delete this file in the commit that completes the work.

> ⚠️ **FIRST, ORIENT — the working tree is NOT clean and NOT all mine.** Run `git status` and
> `git diff`. There is substantial **concurrent/parallel work** in the tree that a previous session did
> not create (see §1). Do **not** `git checkout .`, `git reset --hard`, or blanket-revert anything —
> you will destroy in-progress work. Reconcile file-by-file.

---

## 1. Current working-tree state (read before touching anything)

Two unrelated bodies of uncommitted change are interleaved:

### (a) Parallel `ESignature` DDD hardening — NOT to be reverted, likely someone else's in-flight work
- `ESignature.cs`: `Ip` changed from `string?` → strongly-typed `IPAddress?`.
- New `Infrastructure/Data/Configurations/ESignatureConfiguration.cs`: shared owned-type mapping
  (`Configure<TOwner>`), IP persisted via an `IPAddress↔string` `ValueConverter`, length-bounded columns.
- Edits to `BookingAgreementEntityConfiguration.cs`, `OpportunityEntityConfiguration.cs`,
  `BookingAgreementDocument.cs` (PDF), `ClientContextAccessor.cs`, `IClientContext.cs`,
  `AcceptExecutor.cs`, `BookingAgreementApiTests.cs`.
- A **full migration re-scaffold**: every module's `InitialCreate` deleted (…`1312xx/1314xx`) and
  regenerated (…`1433xx/1435xx/1436xx/1437xx`). This is the output of `api/initial-migrations.ps1`.
- **Action:** treat this as authoritative in-progress work. Confirm with Tommy what state it's in.
  It overlaps with the value-object direction in §3 and may already fix part of §4 — reconcile, don't
  redo.

### (b) A REJECTED fingerprint rename (the previous session's churn) — undo or fold into §3
The previous session renamed the fingerprint keyed-strategy family `…FingerprintComponent` →
`…FingerprintComposer` and extracted a facade. **Tommy rejected the name "Composer"** (see §3). These
uncommitted files are that churn:
- **New (rejected name):** `Interfaces/IContractFingerprintComposer.cs`,
  `Renderers/ContractFingerprintComposer.cs`,
  `Renderers/{FlatFee,DoorSplit,Versus,VenueHire}FingerprintComposer.cs`
- **Deleted (originals):** `Interfaces/IContractFingerprintComponent.cs`,
  `Renderers/{FlatFee,DoorSplit,Versus,VenueHire}FingerprintComponent.cs`
- **Edited:** `Renderers/TermsFingerprintCalculator.cs` (now injects the facade + only hashes — this
  *structural* change is good and wanted), `Infrastructure/Extensions/ServiceCollectionExtensions.cs`
  (fingerprint DI lines renamed).
- **Action:** the *facade extraction* is wanted; the *"Composer" name* is not. Redo per §3. Since these
  files are only mine, it is safe to `git checkout --`/`rm` **just these specific paths** to reset the
  fingerprint family — but verify `ServiceCollectionExtensions.cs` has no parallel edits first.

### (c) Committed this session — keep, do not touch
- `219b1981` `test(b2b): assert e-signatures in the agreement PDF via the UI E2E` — the UI E2E FlatFee
  scenario now downloads the agreement PDF and asserts both signatures (Gherkin: `When … downloads the
  booking agreement` → `Then the agreement PDF is signed by "Artie Artist" and "Vera Venue"`). Also
  de-duped the PdfPig text-extraction helper into `Concertable.Testing` as `Pdf.ExtractText(byte[])`
  (used by both the E2E page object and `BookingAgreementApiTests`).
- `a004a39c` `docs: warn against defaulting away a failure signal` — root `CLAUDE.md` rule.

---

## 2. The REAL bug (highest priority) — artist e-signature missing from the rendered PDF

The E2E assertion added in `219b1981` **fails** in a full-stack run: the agreement PDF contains
`Signed by Vera Venue` (the venue) but **not** `Signed by Artie Artist` (the artist).

**Root cause** — `Infrastructure/Services/BookingAgreementBuilder.cs` (~line 55):
```csharp
application.ArtistESignature,          // ← the EF-tracked owned instance still owned by ApplicationEntity
new ESignature(currentUser.Id, ...),   // ← venue gets a FRESH instance (so it's fine)
```
Passing `application.ArtistESignature` hands the *same* owned-entity instance to the new
`BookingAgreementEntity`. EF then tracks one owned `ESignature` under two owners. `b2b-web` logs it at
accept:
> *The same entity is being tracked as different entity types
> `BookingAgreementEntity.ArtistESignature#ESignature` and `ApplicationEntity.ArtistESignature#ESignature`
> … two store changes, which might not be the desired outcome.*

The artist owned entity is mis-persisted → the background-rendered-at-accept PDF omits the artist line.

**Why integration tests miss it (`BookingAgreementApiTests.Agreement_Pdf_RendersBothPartyESignatures`
passes):** integration renders the PDF **lazily on download** (FakeBlobStorage reports the blob absent),
reading a clean reload from SQL. E2E serves the **background-rendered-at-accept** blob, produced from the
mis-tracked in-memory graph. Different render path → only the full-stack E2E exposes it.

**Proposed fix** — give the agreement its own instance (mirrors the venue path; `ESignature` is a record):
```csharp
application.ArtistESignature is null ? null : application.ArtistESignature with { },
```
**Caveats to check first:** (1) `AcceptExecutor.cs` was edited by the parallel work — re-verify the
accept/build path. (2) The parallel `ESignatureConfiguration` may change how the owned type is tracked;
confirm whether the double-tracking warning still appears after that work — the clone is very likely
still required (a shared *config* does not fix a shared *instance*), but verify. Consider adding an
integration test that reproduces the **background-render** path (not just lazy-on-download) so this can't
regress without the slow E2E.

---

## 3. Fingerprint keyed-strategy: extract the facade + fix the naming + (maybe) value-object it

`ITermsFingerprintCalculator`/`TermsFingerprintCalculator` (Concert module, `…/Renderers/`) computes the
"terms fingerprint" — a SHA-256 hash of the deal-defining numbers, recorded at Apply and re-checked at
Accept so neither party is bound to terms that silently changed.

**Three things, in priority order. Confirm naming + value-object shape with Tommy BEFORE writing —
the last session burned time by charging ahead on both.**

1. **Extract the keyed dispatch into its own facade** (this part is wanted). Today
   `TermsFingerprintCalculator` both (a) holds a `FrozenDictionary<ContractType, …>` of per-type
   strategies AND (b) does the hashing — and its dict is of a *different* interface than the strategies
   it dispatches. Every other keyed-strategy family in the codebase (`ContractMapper`,
   `AgreementTermsRenderer`, `PayeeResolver`, `ArtistShareCalculator`, `PaymentAmountMapper`,
   `ContractUpdater`) uses the canonical shape: a facade class that *implements the strategies' own
   interface*, holds the dict, and delegates. Make the fingerprint family match: a facade implementing
   the strategy interface, injected into `TermsFingerprintCalculator`, which then only hashes.

2. **Naming — do NOT impose a uniform word across families.** The old name `Component` is the only bad
   one (not an agent-noun; `System.ComponentModel`/React-loaded). "Composer" was tried and **rejected**.
   `Renderer`/`Mapper`/`Resolver`/`Calculator` are all fine and **stay** — they describe genuinely
   different things (a terms *Renderer* produces presentation text for the PDF; the fingerprint piece
   produces a hash input — not the same kind of thing). Pick a **purposeful** name for the fingerprint
   pieces and confirm it with Tommy. Do not rename any other family.

3. **Primitive-obsession / value object (Tommy's DDD point).** The per-contract pieces currently emit
   **raw concatenated strings** (`"Fee=500.00"`, `"ArtistDoorPercent=70"`) and the fingerprint is passed
   around as a bare `string`. Consider a typed `TermsFingerprint` value object (equality, `ToString`,
   `Parse`/`TryParse`) so the calculator's output and the stored/compared value aren't stringly-typed.
   **Confirm the exact shape with Tommy before implementing** — this overlaps with the parallel
   `ESignature`→`IPAddress` value-object work in §1(a), so there may be an established local idiom to
   match.

4. **Document it** in `api/docs/CODE_PATTERNS.md` (§ "Keyed strategy resolver"): the keyed dispatch is
   always its **own facade** (never inlined into a consumer that also does other work, as the calculator
   did), with the three-role template — Interface `I{Cap}` shared by facade + strategies; strategies
   `{Key}{Cap}` registered as concrete DI types; facade `{Cap}` (unprefixed) holds the dict + is the DI
   default. `{Cap}` = agent-noun of the strategy's single method (framed as *structural*, not "use one
   word"). Keep the 4-separate-strategies + separate-DI shape — it's consistent with all six siblings
   (their strategies are equally slim one-liners and still registered individually).

---

## 4. Verify (gates before calling it done)

1. **Build** the affected .NET projects (Concert module + the two test projects) and, if any web
   changed, all four SPAs (`app/web` — see `app/web/CLAUDE.md`, all-four-green is the boundary gate).
   Windows tip: build test projects **serially** — parallel builds fight over shared `obj/` DLLs
   (`CS2012 … used by another process`, worsened by Bitdefender) and produce false failures.
2. **Integration:** `Concertable.B2B.Concert.IntegrationTests` — `BookingAgreementApiTests` green.
3. **Migrations:** the tree has a full rescaffold (§1a). Per `api/CLAUDE.md`, model changes are handled
   by `api/initial-migrations.ps1` (nuke + re-scaffold every `InitialCreate`), never additive
   migrations. Ensure the final migration set is consistent with the final model.
4. **E2E (the reason this all started):** the FlatFee UI scenario must go **green** with the artist
   signature present. Run via the `e2e-ui-debug` skill / `./e2e.ps1 ui` — **`./docker-health.ps1` gate is
   mandatory** (a single fresh-container round-trip). Environment was flaky last session: one boot died
   at SQL warmup (`pre-login handshake`, zero scenarios), another booted but had blob-emulator warmup
   churn. Per root `CLAUDE.md`: **a suite that dies at startup is an environment problem — do not debug
   app code or reflex-rerun.** A genuine scenario failure (an `Assert.Contains` on the PDF text) is a
   real bug to fix.

---

## 5. After it's all green
- Delete this handoff file in the final commit.
- Push + open the PR: **plain `gh pr create`** against `master` (personal repo — no ADO work item, no
  `AB#`, no assignee, none of the work-only skills). Tommy had parked the push/PR pending this work.

## Naming lessons (so this isn't re-litigated)
- Don't force one name across structurally-similar-but-purposefully-different families.
- The agent-noun follows the method; `Renderer` (presentation) ≠ the fingerprint piece.
- Reach for a value object over stringly-typed primitives when the thing has identity/equality meaning
  (the fingerprint; the IP already being done in parallel).
- Confirm naming + value-object shape with Tommy *before* writing a multi-file rename.
