# Code review — Feature/launch_tenant-verification

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `3c2947fd725f5c50db498b42763a0dba8925b3f6`  `(2026-08-28)`
**Security-reviewed up to commit:** `3c2947fd725f5c50db498b42763a0dba8925b3f6`  `(2026-08-28)`
**Judgment:** `approved`

## Review pass — 2026-08-27 — full

**Candidate base:** `085520405dc79e98b4e8bfcf982ec1225a36249a`
**Candidate head:** `8bcbde3bff27b96372bee4d3e54a969d3a1f7c2a`
**Candidate branch:** `Feature/launch_tenant-verification`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:8ba476eb62d741eec6306270e9eebb1be61f4881135e20192025c122a7ef4948` `(76 paths)`
**Work-order path:** `reviews/Feature-launch_tenant-verification.md`
**Work-order mode:** `new`
**Pass judgment:** `approved` (both findings fixed on-branch in `b347f5cff`)

Covers Phase 5 (verification admin SPA + tenant-facing UI, `43b42bfb7`) and Phase 6 (retire
`VenueEntity.Approved` + its whole surface, `3685c5f47`). Layers: native/general (code-reviewer agent over
the frozen range) + parent synthesis against the routed .NET / React standards (`csharp-style`,
`csharp-naming`, `http-api`, `result-terminals`, `result-carriers`, `result-errors`, `persistence`,
`multitenancy`, `module-structure`, `domain-events`, `unit-testing`, `integration-testing`,
`typescript-style`, `routing`, `app-tiers`).

### Findings

- [x] **NAT1 — MEDIUM — correctness** — `app/web/b2b/shared/src/features/verification/api/verificationApi.ts:20`
  (CONFIRMED). `verificationApi.get` returned `undefined` on HTTP 204 — the normal response for a tenant that
  has never submitted verification (`VerificationController.Get` → `ToOkOrNoContent()`). A TanStack Query v5
  `queryFn` resolving to `undefined` throws (`Query data cannot be undefined`), so `useVerificationQuery`
  lands in a permanent error state: a console error on every venue/artist dashboard load (`VerificationBanner`)
  and on `/settings/verification`, and `isError`/`isSuccess` become unreliable for any future consumer.
  **Fix:** `get` now returns `Verification | null` (`null` on 204). All consumers already used `verification?.`
  / `{verification && …}`, so no consumer change needed. (The sibling `organizations/api/organizationApi.ts`
  has the identical latent pattern but a B2B manager always has an organization so it never fires — logged as
  a cross-area note, not fixed here.)

- [x] **NAT2 — LOW — correctness** — `app/web/b2b/shared/src/features/verification/components/VerificationForm.tsx:42`
  (CONFIRMED). `attached` was built in catalog order (`DOCUMENT_TYPES.filter(...)`) while
  `useVerification.submit` builds the zod-validated `documents` array in `buffer` insertion order
  (`Object.entries(buffer)`). Attaching files out of catalog order rendered a per-file validation error under
  the wrong file input. **Fix:** `attached` now derives from `Object.keys(buffer)` — the same iteration order
  `submit` uses.

- [wontfix] **pre-existing, transferred** — `app/web/b2b/shared/src/features/organizations/api/organizationApi.ts:7`
  has the identical `undefined`-on-204 pattern as NAT1, but it pre-existed on the base, is latent (a B2B
  manager always has an organization, so the 204 branch is unreachable), and its repair is independent of
  this branch. Logged to `app/web/b2b/shared/TECH_DEBT.md` (LOW) with its resolution condition.

Synthesis: native layer was the `code-reviewer` agent over the frozen range (independent cold context).
Parent verified both citations against the source, confirmed the failure scenarios, and re-checked the
Phase 6 removals against the routed .NET standards — no additional findings. Phase 6 backend removals and
the re-scaffolded migration are clean (symmetric, no dangling references anywhere in `api/` or `app/`,
`VenueChangedDomainEvent` still raised for every remaining venue mutation). Both native findings fixed
on-branch; five web builds + `lint:boundaries` + 28 web-b2b unit tests green after the fix.

## Review pass — 2026-08-28 — incremental (`8bcbde3bf..bd95f8c38`)

**Pass judgment:** `approved` — no findings.

Delta since the frozen head: (1) `79fd01b20` — the NAT1 + NAT2 remediation, this review's own output,
already `[x]` above; (2) merges pulling in `main` — PR #825 (publish-first `web-b2b` verification export,
reviewed in `reviews/Feature-launch_tv-web-b2b-verification.md`), #826 (platform-sync `0.1.0-alpha.0.1225`),
#827 (local-dev config, reviewed in `reviews/Chore-local-dev-config.md`), #823 (RepoSplit rt3, reviewed
independently); (3) plan/ledger + review docs. No un-reviewed executable code in the range.
`Concertable.B2B.Web` rebuilds 0-error on the new platform pin; five web builds + `lint:boundaries` green
post-merge. Backend verified live from a running `Concertable.B2B.AppHost`: OIDC login renders with the
correct venue `redirect_uri`, `GET /api/organization/verification` returns 401. Authenticated UI
click-through is the merge-queue `full-e2e` tier (label applied).

Commits after `bd95f8c38` are docs-only — the ledger reconcile (`5df88875f`) and this file's structural
flatten — so the watermark advanced to `5df88875f`.

**2026-08-28 (b):** #824 was ejected from the merge queue as `DIRTY` after #828 (platform-sync
`0.1.0-alpha.0.1228`) and #829 (`app/shared` + `app/web/b2b/shared` `BASE`-const api refactor + a
`TECH_DEBT.md` clear) landed on `main`. Merged `main` in (`fe22e96f3`); the only conflict was
`app/web/b2b/shared/TECH_DEBT.md` — resolved by keeping this branch's still-valid `organizationApi.get`
204 entry and dropping the `BASE`-const entry #829 fixed. `Concertable.B2B.Web` rebuilds 0-error on pin
`1228`; venue/artist/admin web builds + `lint:boundaries` green. All incoming code is #828/#829 —
independently reviewed and merged. No findings. Watermark → `fe22e96f3`.

## Review pass — 2026-08-28 — incremental (`fe22e96f3..d77d0ff5b`)

**Pass judgment:** `approved` — no findings.

Ejected from the queue twice more as `main` moved. Two more base merges: #829 (already noted),
#830 (`Refactor/mapper-naming` — renames every `*ResponseMappers.cs` → `*Mappers.cs`, independently
reviewed + security-marked in its own work order). The only conflict was `VenueResponseMappers.cs`
(deleted by #830's rename, modified by this branch) — resolved by re-applying this branch's one change
(drop `Approved = dto.Approved`) to #830's renamed `VenueMappers.cs` and `git rm`-ing the old file.
`Concertable.B2B.Web` + Venue unit/integration + venue/artist/admin web builds + `lint:boundaries` green
post-merge. No un-reviewed executable code. Both watermarks → `3c2947fd7` (the merge `d77d0ff5b` + the
docs commit recording it; nothing executable after `d77d0ff5b`).

## Security layer — 2026-08-28

Ran on the security-sensitive paths (`VenueController.cs`, `PrivilegedDbContext.cs`, the migration, the
admin/venue/artist API + route wiring; re-checked over the #830 mapper rename). **Zero findings.** Phase 6's backend diff is overwhelmingly
deletions that *reduce* attack surface: two `[Admin]` endpoints removed, and the entire
`VenuePrivilegedDbContext` (an unfiltered cross-tenant *writable* context) removed. The `Approved` field
is dropped from the wire contract, not added. The admin/venue/artist frontend is client-side wiring over
`/api/verification/*` endpoints whose `[Admin]` gating is server-side (Phase 4). No injection,
auth-bypass, privilege-escalation, crypto, or data-exposure issue introduced. Marker → `56a0bdce0`.
