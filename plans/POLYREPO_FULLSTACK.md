# Full-stack polyrepo — frontend build separation

**Goal:** make `customer` and `b2b` genuine **full-stack** standalone units (their `api/` service **plus**
their web + mobile surfaces), each restoring shared code from a package feed and building alone — so the
service mirrors stop being "separate backend, frontend left behind in the monorepo." `auth`, `payment`,
`search` stay backend-only by nature (no frontend) and are already carved.

**Why now:** the backend is fully carved (feed `PackageReference`s, per-folder CPM, `EnforceServiceBoundary`,
`carve-*` CI, `platform-sync`). The frontend has **none** of that equivalent: shared FE is consumed via
`"*"` workspace symlinks + `../shared/src` path aliases under one root `package-lock`. There is nothing on
the FE a mirror could subtree-split into a standalone buildable repo. This plan builds the FE analogue of
the backend carve. It is the mandatory prerequisite for any full-stack split shape.

**This mirrors the backend effort** documented in [`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md)
("Cross-service contract distribution" / "Per-folder build closures"). Read that first — every phase here
is the npm counterpart of a step that already exists there for .NET.

---

## Starting state (verified 2026-07-31)

- **`@concertable/shared`** (`app/shared`) and **`@concertable/customer-shared`** (`app/customer/shared`) are workspace
  packages with a **clean `exports` map already** — but source-consumed: `main`/`types`/`exports` all point
  at `./src/*.ts`, `version: 0.0.0`, no build, no `files`, no publish config. Bundlers compile the raw TS
  through the workspace symlink. Import style is already bare subpaths (`@concertable/shared/features/auth`).
- **`web/shared`, `mobile/shared`, `web/b2b/shared`** are **not packages at all** — no `package.json`,
  consumed purely via tsconfig/vite/metro path aliases (`@/*` → `../shared/src`, `@b2b/*`, `shared/*`).
  ~29 files in `web/customer/src` alone still import through these aliases.
- Dependency graph (post the shared-boundaries refactor, which is **done**):
  `@concertable/shared` ← everything (web+mobile, customer+b2b) · `web/shared`/`mobile/shared` ← both sides ·
  `web/b2b/shared` ← b2b web only · `@concertable/customer-shared` ← customer web+mobile (and → `@concertable/shared`).
- **`@concertable/shared` is dual-target** (vite web **and** metro/RN mobile). This is the sharpest risk in
  the whole plan: a published package must build/consume correctly under **both** bundlers. It is already
  platform-agnostic by construction (the boundaries refactor removed RN/web leaks), but "agnostic source
  compiled in-tree" ≠ "published artifact metro will consume." Prove metro consumption early (Phase 1 gate).
- Registry model to reuse: backend uses `nuget.pkg.github.com/Concertable` + a `GITHUB_PACKAGES_TOKEN` PAT
  (`read:packages`) already documented in root `README.md`. The npm counterpart is `npm.pkg.github.com`
  with the owning `@concertable` scope.
- Toolchain: node 20, npm 10 (workspaces). `app/node_modules` is not committed — a fresh `npm install`
  from `app/` is the precondition for any build/pack gate.

---

## Decisions (resolve at the gate that needs them, not up front)

- **D-A — End state.** Buildable **read-only full-stack mirrors** (monorepo stays source of truth, like
  today's BE mirrors) **vs** a true one-way cut to independently-developed repos. Affects how much mirror
  tooling to invest in Phase 5; does **not** affect Phases 0–4. *Default until decided: buildable mirrors.*
- **D-B — Full-stack mirror mechanism (Phase 5).** `git subtree split` takes **one** prefix, so it cannot
  fuse `api/Concertable.Customer` + `app/web/customer` + `app/mobile/customer` + `app/customer/shared` into
  one repo. Two ways out: **(1)** restructure the monorepo to per-service colocation
  (`services/<x>/{api,web,mobile}`) — big move touching every path/alias/AppHost/CI; or **(2)** a
  multi-source mirror **assembler** that stitches the trees at mirror time. Resolve at Phase 5.
- **D-C — Publish format (Phase 1).** Build each shared package to `dist` (JS + `.d.ts`) **vs** publish raw
  TS source. *Recommended: `dist`* — a separate repo shouldn't compile another repo's raw TS, and it matches
  how a real published dep behaves; the cost is a build step per shared package and getting metro to consume
  `dist`. (Publishing source is less work now but re-imports the "consumer compiles our internals" coupling
  the carve exists to remove.)

---

## Phases

Each phase is independently committable and ends green. **Standing gate every phase** (the existing
four-green FE gate): `npm -w @concertable/web-{customer,venue,artist,business} run build` +
`tsc --noEmit` in both mobile workspaces (`mobile/customer`, `mobile/b2b`). Publish phases add: the package
builds, and a `npm pack` tarball installs + type-checks in a throwaway consumer.

### Phase 0 — Registry + PAT (unblocks everything; no code cutover)
- ✅ Add scoped npm registry config: root `app/.npmrc` (or per-surface, mirroring the BE "no repo-root config"
  rule — decide with D-B) mapping `@concertable` → `https://npm.pkg.github.com`.
- ✅ Provision a classic PAT with `write:packages` (publish) + `read:packages` (restore); reuse/extend the
  documented `GITHUB_PACKAGES_TOKEN`. Publishing and restoring `@concertable/shared@0.1.0-alpha.0.2129`
  proves the owning scope is available.
- ✅ Gate: a dry-run `npm view @concertable/shared --registry=https://npm.pkg.github.com` authenticates (404
  for "not yet published" is success — auth resolved).

### Phase 1 — Publish the universal core: `@concertable/shared` (publish-first, no consumer cutover yet)
- ✅ Build/package shape: `tsc` emits a per-file ESM + declaration tree to `dist`; `main`, `types`, and
  conditional exact/wildcard exports resolve the built tree; the package is publishable and packs only
  `dist`.
- ✅ Local publish proof: the tarball installs and passes `tsc --noEmit` in a throwaway consumer across
  barrel, hook, type, and nested feature subpaths; all four web builds and both mobile typechecks are green.
- ✅ Release automation: git-height versions (`0.1.0-alpha.0.<height>`) and `publish-fe-packages.yml` build,
  pack, idempotently publish under the `alpha` tag, then type-check the exact feed artifact in a fresh consumer.
- ✅ Feed publication: `@concertable/shared@0.1.0-alpha.0.2129` is published under the `alpha` tag and
  installs and type-checks from the feed in a fresh NodeNext consumer.
- **Do not cut consumers over yet** — like the BE, publish first; consumers still resolve the workspace copy.

### Phase 2 — Publish the remaining shared tiers + cut consumers over
- Make `web/shared` → `@concertable/web-shared`, `mobile/shared` → `@concertable/mobile-shared`,
  `web/b2b/shared` → `@concertable/b2b-web-shared` real packages (add `package.json` + `exports` + build + publish).
  Give `@concertable/customer-shared` the Phase-1 treatment (it depends on `@concertable/shared` → publish order matters).
- **Cut consumers over**: convert every `@/*` / `shared/*` / `@b2b/*` / `../shared/src` **path-alias** import
  to a **package** import; delete the cross-tree source aliases from every tsconfig/vite/metro config.
- Gate: grep clean — no surface config contains a cross-tree source alias (`../shared/src`, `../../shared/src`);
  no `from "@/..."`→shared or `from "shared/..."` source import survives. Four web + two mobile green **against
  the published packages**.

### Phase 3 — Per-surface standalone build closure + carve CI + boundary enforcement
- Prove each surface restores its shared deps **purely from the feed** (no monorepo-root workspace resolution).
- Add `carve-fe-{customer,b2b}` CI jobs (+ per manager web surface as needed): `git archive` the surface,
  restore from the feed, build alone — the FE analogue of the BE `carve-*` gates.
- Add an import-boundary rule (ESLint `no-restricted-imports` / dependency-cruiser) so a surface cannot reach
  into another surface's or an unpublished tier's source — the FE `EnforceServiceBoundary`.
- Gate: carve-fe jobs green in CI.

### Phase 4 — FE platform-sync
- Version-bump propagation: a shared-FE republish opens `chore/fe-platform-sync-*` bumping every consumer's
  pinned version, auto-merge (counterpart of `platform-sync.yml`) — or adopt changesets' release PR. Wire a
  `fe-platform-sync-alert` backstop like the BE one.
- Gate: a shared bump flows to customer + b2b and greens hands-off.

### Phase 5 — Produce full-stack repos (D-A + D-B resolve here)
- Per D-B: restructure to per-service colocation **or** build the multi-source mirror assembler; emit
  full-stack `customer` / `b2b` repos (api + web + mobile) that clone-and-build. `auth`/`payment`/`search`
  mirrors unchanged. Stand up the shared-FE package repo(s) (counterpart of `Concertable/shared`).
- Gate: a fresh clone of the full-stack `customer` mirror restores (NuGet **and** npm feeds) and builds its
  backend + every FE surface with 0 errors. Same for `b2b`.

---

## Out of scope
- The BE carve itself (done).
- The shared-FE boundaries detangle (done — was its own effort; do **not** reopen it here).
- Retiring the monorepo / true independent development (that's D-A = the "true cut", only if chosen).
