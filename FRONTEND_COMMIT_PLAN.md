# Frontend boundaries — commit slicing plan

**Goal:** turn the fused working tree (stages 1–4 rewrites + relocations, all uncommitted) into a
readable history: **rewrites first (apps at their OLD paths), then move-only rename commits.**
Standing rule for every commit: show staged diff, wait for explicit approval. No Claude trailers.

## OPEN QUESTION (user decides before step 2)

One commit for all stage 1–4 rewrites, or per-stage commits?
**Recommendation: ONE commit.** The stages repeatedly touched the same files (auth index,
concerts index, ProfileMenu/Navbar, package.json…); reconstructing four intermediate states from
the fused tree means hand-rebuilding each file's per-stage content with no gate runnable in
between. One rewrites commit + two move-only commits is honest and cheap.

## Current working-tree state (2026-06-05)

- Stages 1–4 ✅ + web b2b relocation ✅ + mobile b2b relocation ✅. Gates green at last full run
  (2 mobile tsc 0, 4 web builds 0).
- **IN FLIGHT — mobile flatten `mobile/b2b/business` → `mobile/b2b`** (user: "b2b IS business").
  Dir already `git mv`'d to `mobile/b2b`; **configs NOT yet fixed**. Pending:
  1. `mobile/b2b/metro.config.js`: `../../shared` → `../shared` (watchFolders + global.css input)
  2. `mobile/b2b/tsconfig.json`: paths + include `../../shared/...` → `../shared/...`
  3. `mobile/b2b/tailwind.config.js`: content `../../shared/src/**` → `../shared/src/**`
  4. `mobile/b2b/App.tsx`: `import "../../shared/global.css"` → `"../shared/global.css"`
  5. `mobile/b2b/package.json`: name `@concertable/mobile-business` → `@concertable/mobile-b2b`
  6. `app/package.json`: workspace `"mobile/b2b/business"` → `"mobile/b2b"`; script
     `dev:mobile:business` → `dev:mobile:b2b` pointing at `@concertable/mobile-b2b`
  7. `npm install` (temp cache: `--cache "$env:TEMP\npm-cache-stage4"` — AppData cache is AV-flaky),
     then hand-delete the stale `"mobile/b2b/business"` lockfile key (npm leaves moved-workspace
     keys as `"extraneous": true` forever; `mobile/artist`+`mobile/venue` orphans are in HEAD — leave)
  8. Gate: `npx tsc --noEmit` in `mobile/b2b` → 0
  - NOT touched (external identifiers): `app.json` expo name/slug/scheme/bundleIds stay
    `concertable-business`/`com.concertable.business` (OAuth redirect scheme + store IDs).
  - Note: flatten un-breaks `app.json` `../assets/...` (assets live at `mobile/assets`; the nested
    location had silently broken those paths).

## api/ scoping

The repo has UNRELATED uncommitted api/ workstreams (Search migrations/seeders, EscrowService,
AppHost Program.cs, …). Pathspec-scope every commit below. api/ files that DO belong to the
rewrites commit (Stage 2 / D1):
- `api/Concertable.Customer/Modules/Review/.../{Concert,Artist,Venue}ReviewService.cs` (200-false)
- `api/Concertable.Customer/Modules/Review/Tests/.../ReviewApiTests.cs`
And to the web move-only commit: `api/Concertable.AppHost.Shared/DistributedApplicationBuilderExtensions.cs`
(`AddSpaSurface` `tierSegments` + `"b2b"` args).

## Commit 1 — stages 1–4 rewrites (apps at OLD paths)

1. Safety snapshot first: `git add -A` then note `git write-tree` output (recoverable via reflog
   even after reset).
2. `git reset` (index → HEAD).
3. Move dirs back (filesystem, plain `mv`):
   - `app/web/b2b/{venue,artist,business}` → `app/web/{venue,artist,business}` (leave
     `app/web/b2b/shared` — it's a Stage 3 rewrite, stays in commit 1 at this path)
   - `app/mobile/b2b` → `app/mobile/business`
4. Revert the relocation-only edits (everything else stays):
   - `app/package.json`: workspaces back to `web/{venue,artist,business}` + `mobile/business`
     (KEEP `customer/shared` — Stage 4 rewrite); script back to `dev:mobile:business` /
     `@concertable/mobile-business`
   - web `{venue,artist}/tsconfig.app.json`: `../../shared`→`../shared`, `@b2b` `../shared`→
     `../b2b/shared`, include likewise; `business/tsconfig.app.json`: `../../shared`→`../shared`
   - web `{venue,artist,business}/vite.config.ts`: same depths; `envDir '../../'`→`'../'`
   - `app/web/shared/src/index.css` `@source`: `b2b/{venue,artist,business}/src` →
     `{venue,artist,business}/src` (KEEP the `b2b/shared/src` line — Stage 3 fix)
   - `api/...AppHost.Shared/DistributedApplicationBuilderExtensions.cs`: revert `AddSpaSurface`
     to `RepoPath(builder, "app", "web", surface)`, drop `tierSegments` + the three `"b2b"` args
   - mobile app configs (now at `mobile/business/`): metro/tsconfig/tailwind/App.tsx back to
     `../shared`; package name back to `@concertable/mobile-business` (i.e. undo flatten step 5)
   - plan docs: `SHARED_FRONTEND_BOUNDARIES_PLAN.md` D5/Stage-4 relocation lines un-ticked for
     this commit? NO — keep docs as-is (docs describe the end state; they land in commit 1, that's
     fine and not worth surgical splitting).
5. `npm install` (temp cache) → lockfile reflects old paths + `customer/shared`; clean any new
   extraneous orphans.
6. Gate: 4 web builds + 2 mobile tsc green at old paths.
7. Stage pathspec: `app/` + the two plan/brief mds + `FRONTEND_COMMIT_PLAN.md`? (no — this file is
   scaffolding, delete at the end) + the 4 Review api files + root `CLAUDE.md`. Show diff → approval
   → commit. Suggested message: shared-frontend boundaries stages 1–4 (tiers, @customer/shared,
   mobile parity).

## Commit 2 — move-only: web manager apps → `app/web/b2b/`

Replay exactly (all enumerated in commit-1 step 4): `git mv` the three app dirs; workspaces globs;
tsconfig/vite depths + `@b2b` → `../shared`; `envDir '../../'`; index.css `@source` b2b paths;
AppHost.Shared `AddSpaSurface` tierSegments. `npm install` + lockfile orphan cleanup. Gate: 4 web
builds. Diff → approval → commit.

## Commit 3 — move-only: `mobile/business` → `mobile/b2b` (flatten, no nesting)

`git mv mobile/business mobile/b2b`; package name `@concertable/mobile-b2b`; workspaces
`"mobile/b2b"`; script `dev:mobile:b2b`; configs stay `../shared` (depth unchanged at 1 level —
that's the whole point of the flatten). `npm install` + lockfile cleanup. Gate: `tsc --noEmit` in
`mobile/b2b` + `mobile/customer`. Diff → approval → commit.

## After

- Delete this file (scaffolding).
- Update memory: boundaries refactor committed; record the three commit hashes.
- Remaining repo noise: the other api/ workstreams stay uncommitted (separate efforts).
