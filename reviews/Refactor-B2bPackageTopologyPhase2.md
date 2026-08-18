# Code review — Refactor/B2bPackageTopologyPhase2

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `fc59c26aa8459aca425ece6e61302c0939b4f99a`  _(2026-08-18)_

**Security-reviewed up to commit:** `fc59c26aa8459aca425ece6e61302c0939b4f99a`  _(2026-08-18)_

> Range reviewed: `de4f377e8..fc59c26aa` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native/correctness** — `app/b2b/shared/src/features/venues/hooks/useVenue.ts:60`
  Removed the premature editor facade, eliminating the unawaited per-call asynchronous mutation callback.
- [x] **CV1 — HIGH — frontend state ownership** — `app/b2b/shared/src/features/artists/store/useArtistStore.ts:37`
  Removed both editor stores and facades; the package now exposes TanStack-owned query/mutation APIs without mirroring server entities into Zustand.
- [x] **CV2 — HIGH — write boundary** — `app/b2b/shared/src/features/artists/hooks/useArtist.ts:36`
  Removed the draft-to-request facades so Phase 3 consumers must supply locally buffered, validated request values.
- [x] **CV3 — MEDIUM — test coverage** — `app/b2b/shared/src/features/artists/api/artistApi.ts:6`
  Added artist and venue multipart tests covering complete create requests and image-less update requests.

## Security review

No security issues found in the frontend package publication and verification workflow changes.
