# Code review — Refactor/MobileSharedQueryClientExport

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `71b1493dabe702aba649054ece40817bb8292388`  `(2026-09-09)`
**Security-reviewed up to commit:** `71b1493dabe702aba649054ece40817bb8292388`  `(2026-09-09)`
**Judgment:** `approved`

## Review pass — 2026-09-09 — full

**Candidate base:** `64b3dccec2c48bf1327ce93f229a6628ccf1c325`
**Candidate head:** `71b1493dabe702aba649054ece40817bb8292388`
**Candidate branch:** `Refactor/MobileSharedQueryClientExport`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:697395c7846be506edb23b1e3f1de46521588aa5efb7d91ab6828736a551bce9` `(1 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-MobileSharedQueryClientExport.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

### Findings

No open issues.

One declaration in `app/mobile/shared/src/providers/AppProviders.tsx` changes from `const queryClient`
to `export const queryClient`. Nothing else in the diff.

- **It mirrors an existing tier contract rather than inventing one.** `@concertable/web` already exports
  its instance from `app/web/shared/src/lib/queryClient.ts`, so the mobile tier's public surface now
  matches web's instead of being narrower for no stated reason.
- **No runtime behaviour changes.** The instance, its `defaultOptions`, and its caches are untouched;
  only the module's export surface widens. Nothing on `main` imports it yet, so no existing consumer
  resolves differently.
- **It is a producer-only step by construction.** The consumer that needs it, `app/mobile/b2b`, is on
  PR #950 and resolves `@concertable/mobile` from the feed, so the export must be published first. #950's
  `carve-fe (mobile/b2b)` job failed with `TS2305 ... has no exported member 'queryClient'`, which is
  exactly the gap this closes.
- **Security:** the exported value is an in-memory `QueryClient`. It carries no credential, token or
  configuration secret, and widening a module export does not change what the app fetches or stores. No
  frozen path matches an auth, payment, contract or controller pattern.

Carries no skip-E2E opt-out: a published package's public surface is a positive E2E trigger.
