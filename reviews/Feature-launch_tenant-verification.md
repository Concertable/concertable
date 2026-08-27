# Code review — Feature/launch_tenant-verification

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `8bcbde3bff27b96372bee4d3e54a969d3a1f7c2a`  `(2026-08-27)`
**Judgment:** `approved`

Findings NAT1 + NAT2 remediated on-branch in the commit that carries this file; the only delta past the
frozen head is that remediation (this review's own output), so no incremental pass is owed.

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

## Cross-area notes

- [wontfix] `app/web/b2b/shared/src/features/organizations/api/organizationApi.ts:7` — same `undefined`-on-204
  pattern as NAT1, but pre-existing on the base, latent (a B2B manager always has an organization, so the 204
  branch is unreachable), and its repair is independent of this branch. Transferred to
  `app/web/b2b/shared/TECH_DEBT.md` (LOW) with the resolution condition.

## Parent finalization

Native layer: `code-reviewer` agent over the frozen range (independent cold context). Parent verified both
citations against the source, confirmed the failure scenarios, and re-checked the Phase 6 removals against
the routed .NET standards — no additional findings. Phase 6 backend removals and the re-scaffolded migration
are clean (symmetric, no dangling references anywhere in `api/` or `app/`, `VenueChangedDomainEvent` still
raised for every remaining venue mutation). Both native findings fixed on-branch; five web builds +
`lint:boundaries` + 28 web-b2b unit tests green after the fix.
