# app/mobile — Technical Debt

---

## MED

### Mobile app has never been bundled end-to-end — full `expo export` health unproven

CI has only ever `tsc --noEmit`'d the mobile surfaces; neither `mobile/customer` nor `mobile/b2b`
has been run through a real `expo export`/metro bundle. The FE carve (`carve-fe`) turning on the
mobile `expo export` step is the first time it happens, and it immediately surfaced a latent bug that
would have failed any bundle, in-monorepo or carved: `@concertable/mobile`'s `Logo` `require`d
`assets/brand/logo*.png` that the package never shipped (`files` lacked `assets`) and that physically
lived outside the package at `app/mobile/assets/`. Fixed on `Feature/platform_polyrepo_mobile-retarget`
(PR #413) by moving `brand/` into the tier (`app/mobile/shared/assets/`) and adding `assets` to the
tier's `files`.

Because the bundle path was never exercised, **more latent bundle failures may sit behind that first
one** (font/asset resolution, native-module config, metro/NativeWind edge cases). They can only be
found once the mobile carve runs past the asset step — which needs `@concertable/mobile` republished
with the assets fix (the carve restores the tier from the feed), i.e. after PR #413 merges. The
follow-up PR that adds `mobile/{customer,b2b}` back to the `carve-fe` matrix is where the remaining
tail gets chased.

**Resolves when:** both `mobile/customer` and `mobile/b2b` pass the `carve-fe` mobile `expo export`
gate green in CI (tracked as Phase 3 item 1's follow-up in
`plans/platform/POLYREPO_FULLSTACK_PROGRESS.md`).
