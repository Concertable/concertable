# Review — Fix/web-b2b-tenant-header-verification

**Review status:** `complete`

**Judgment:** `approved`

**Reviewed up to commit:** `07a52528fb0a0245d70b1ab0c56ec657248fe1be`

- Candidate (final): `ea985630f..07a52528fb0a0245d70b1ab0c56ec657248fe1be` — one line of `app/scripts/verify-fe-package.mjs`
- Branch: `Fix/web-b2b-tenant-header-verification`
- Scope: all
- Changed paths: 1
- Routed skills: none (`skill_router.py` returns no route for `app/scripts/`)
- Security layer: not run — no changed path classifies as security-sensitive

## Findings

### F1 — the branch's first fix pointed the check at a module Node cannot load — major, fixed

Dropping the `"features/tenant/constants"` argument resolved `TENANT_HEADER` at the type level but
aimed the check at the `features/tenant` barrel. Importing that under plain Node executes
`configureWebClient(import.meta.env.VITE_API_URL)` inside `b2bClient` at module load. Found by running
it, not by reading it: verifying `@concertable/web-b2b@0.1.0-alpha.0.6503` cleared the `TS2305` and
then failed with `Cannot read properties of undefined (reading 'VITE_AUTH_AUTHORITY')`.

That is why the original argument existed — `features/tenant/constants` is a load-safe leaf, which the
topology work emptied of the header without moving the check.

Fixed: the check asserts what the tier owns (`TENANT_ROLE_LABELS`, the `TenantRole` type) from that
leaf. Re-verified against the same published artifact: typecheck and runtime execution both pass,
exit 0.

## Evidence the parent verified

- The failure is real and pre-existing: identical `TS2305` at runs `34379769582` (sha `e4989767f`,
  2026-09-09T16:55) and `34413031384` (sha `61b83c98c`, 22:36).
- `@concertable/web-b2b` is stranded at `0.1.0-alpha.0.6503` while every other tier reached
  `0.1.0-alpha.0.6606` — confirmed against `orgs/Concertable/packages/npm/web-b2b/versions`.
- Why the old argument was once right: the published `6503` tarball's
  `dist/features/tenant/constants.d.ts` declares `TENANT_HEADER = "X-Tenant-Id"`. Read from the actual
  tarball, not inferred.
- Why it is wrong now: `app/web/b2b/shared/src/features/tenant/constants.ts` exports only
  `TENANT_ROLE_LABELS`, and `src/features/tenant/index.ts:1` re-exports
  `{ TENANT_HEADER, TENANT_ROLES }` from `@concertable/b2b/features/tenant`.
- Why the default resolves: `web-b2b`'s exports map carries `./features/tenant`, and the `6503`
  tarball's `dist/features/tenant/index.d.ts` re-exports `TENANT_HEADER` too — so the fixed check is
  correct against both the current source and the last good publication.
- `b2bChecks` also imports `type TenantRole` from `features/tenant/types`, which
  `src/features/tenant/types.ts` provides.
- `@concertable/b2b` publishes before `@concertable/web-b2b` in the workflow's tier order, so the
  re-export's target is on the feed by the time web-b2b is verified.

The publication gate is the terminal proof and runs on merge; it is already red, so landing this cannot
make it worse.
