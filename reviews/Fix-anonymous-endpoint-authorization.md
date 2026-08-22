# Code review — Fix/anonymous-endpoint-authorization

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `0ae63fb63d2d36c136737d852671c8bbbb0bd9d0`  _(2026-08-22)_
**Security-reviewed up to commit:** `0ae63fb63d2d36c136737d852671c8bbbb0bd9d0`  _(2026-08-22)_

> Range reviewed: `2323c77e7..9b2248a78` (original 2 commits) + base merge to `0ae63fb63` (see incremental note below).
> Two mandatory layers (native general review + security layer for the touched `Controller*.cs` / `Concertable.Payment` paths) plus the architecture-aware lenses. Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **SEC1 — LOW — security (defense-in-depth)** — `api/Concertable.B2B/src/Concertable.B2B.Web/Controllers/BlobController.cs:21`
  Anonymous `GET /blob/download` reads from the single `"images"` container, which also holds private
  contract/invoice PDFs written by `PdfBlobCache` under `contracts/…`/`invoices/…` prefixes. Not exploitable
  as routed (Guid-named blobs; single-segment route rejects encoded `/`; no List op), but name-secrecy +
  routing shape is the only barrier. **Fixed:** `Download` now returns `NotFound` for any `blobName`
  containing `/` or `\`, so it cannot resolve a namespaced private blob regardless of future route shape.
  Broader fix (separate public/private containers) logged in `api/TECH_DEBT.md`.

### Reviewed, no code change required

- **Native general review (Layer 1):** no findings. Verified the removed `Upload`/`Delete` endpoints are dead
  (no callers in `api/`; frontend `blobApi.ts` calls only `/blob/download`), the guard's
  `IsDefined(typeof(AuthorizeAttribute), inherit:true)` correctly counts `[HasPermission]`/`[Admin]`
  (both derive from `AuthorizeAttribute`), and the arch-test project references `Concertable.B2B.Web` so
  `BlobController` is actually in scope.
- **Payment `[Authorize]` (Lens A / security pt.1):** correct and complete. No IDOR — `TransactionService.GetAsync`
  derives identity server-side via `currentUser.GetId()` and `TransactionRepository.GetAsync` filters
  `PayerId == userId || PayeeId == userId`; no client-supplied id.
- **Lenses B–E:** no findings. No cross-service/runtime dependency added; `BlobController` uses only the shared
  `IImageService`; naming/style compliant (`this.`-qualified, sealed, file-scoped); the removed
  `IBlobStorageService` field/param/usings are gone cleanly.

### Deferred to tech debt (tracked in `api/TECH_DEBT.md`, not fixed here)

- **Payment/all-service mutating guard (Lens F / security Gap A):** the new
  `Mutating_endpoints_declare_authorization_explicitly` guard is B2B-only. A per-service copy is the exact
  duplication `Concertable.Shared/TECH_DEBT.md` already flags for boundary guards, so the proper fix is one
  shared reflection helper covering every service — logged, not force-fit here. Payment's `[Authorize]` is
  verified correct and exercised by `Concertable.Payment.CompositionTests` (web host boots with it).
- **Private-vs-public blob container separation:** the broader form of SEC1 — logged.
- **~30 anonymous-by-omission read endpoints (security Gap C):** the largest residual exposure (private
  contract/invoice PDFs on unauthenticated GETs, fail-closed only via a service-layer filter) — logged, needs a
  per-endpoint public/private classification.

## Incremental review — 2026-08-22 (base merge `origin/main` → `0ae63fb63`)

`origin/main` advanced 113 commits during the merge attempt, so base was merged into the branch. Those
commits are others' already-reviewed, already-merged PRs — not re-reviewed here. The only new,
previously-unreviewed code is one merge-conflict resolution, plus one stale-reference fix:

- **`ControllerBoundaryTests.cs` conflict resolution** — main renamed the shared helper
  `ControllerTypes()` → `GetControllers()` and added `Controller_route_segments_match_controller_names_and_routes`.
  Resolved by keeping main's helper + new test and re-adding `Mutating_endpoints_declare_authorization_explicitly`
  on the shared helper, extended so `GetControllers()` also scans `Concertable.B2B.Web.dll` (keeps `BlobController`
  in the mutating-guard's scope; harmless to the other two tests — Web-host controllers have no `RouteSegment`
  field and don't inject `TimeProvider`). Verified: B2B + Payment build clean, `ArchitectureTests` 11/11 pass.
- **`api/TECH_DEBT.md`** — the cross-reference to the "hand-rolled boundary guard" item was retargeted: PR #725
  resolved and deleted that item by building the shared `Concertable.Composition.Testing` assembly-guard helper,
  so the note now points at that existing helper as the model for a shared auth-guard.

No new findings.
