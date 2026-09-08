# Code review — Refactor/FrontendRegistryPackageConsumers

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `73e36b581f434b8d460da095cdc8a0168542945e`  `(2026-09-08)`
**Judgment:** `changes-requested`

## Review pass — 2026-09-08 — full

**Candidate base:** `3052fb9ac8e21f51bd4ff7e49a36176ad8f770c2`
**Candidate head:** `73e36b581f434b8d460da095cdc8a0168542945e`
**Candidate branch:** `Refactor/FrontendRegistryPackageConsumers`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:5be4f96102ab84f0751548ddf9604cc186606155af8a2edb6e7fa36fd07b8cb9` `(12 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable--worktrees-Refactor-FrontendRegistryPackageConsumers\5c97c8fc-deba-45af-ad34-2124ba6d754e\scratchpad\review-bundle`
**Candidate bundle identity:** `sha256:488bfd8ca5cf4bc1903341a2b7d4e3e1b2b91ad6145d3998ea626d60756237a3`
**Work-order path:** `reviews/Refactor-FrontendRegistryPackageConsumers.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Lens dispatch was unavailable in this session, so every layer — native/general, correctness, convention,
changed-behaviour test impact, and the routed `plans` rules — ran in the parent over the frozen bundle, which
is the contract's stated fallback. Ten of the twelve paths are generated lockfiles reviewed as invariants
rather than line by line; the reviewable surface is `app/scripts/carve-fe.mjs`,
`app/scripts/carve-fe.test.mjs`, `app/package.json` and the two plan documents.

`skill_router.py` over the frozen path set routes exactly one standard, `plans`, for the two
`plans/platform/*` files. No path matches the merge gate's generic security patterns
(`^\.github/workflows/`, the auth/credential vocabulary) or this repo's `security_paths`
(`Concertable.Auth`, `Concertable.Payment`, `.Contracts`, `Controller*.cs`), so this pass requires no
`Security-reviewed up to commit:` marker.

### Findings

- [x] **FRP1 — LOW — convention/documentation-completeness** — `app/README.md:37`
  The candidate makes a committed per-surface lockfile a *required* input to the `carve-fe` gate: the carve
  now runs `npm ci`, so adding any dependency to a surface without regenerating its lock fails that job. The
  new regeneration entry point is `npm run lock:carve`, but `app/README.md`'s `## Commands` block — the file
  that states it is "only the workspace inventory and the commands", and the documented entry point for
  working in `app/` — lists `dev:*`, `build:*` and `lint:boundaries` and does not mention it. A developer who
  adds a package to `web/customer` gets a red `carve-fe` job with an npm "missing from lock file" error and
  no pointer to the command that fixes it. Fix: add `npm run lock:carve` to that command block with one
  sentence saying the per-surface `package-lock.json` is what a standalone surface restores and what
  `carve-fe` installs, that it is inert inside the workspace, and that changing an app's dependencies means
  regenerating it.

### Verified clean

Checks that could have produced a finding and did not, each confirmed against the frozen tree:

- **Root lock stays in sync.** `app/package.json` gains only a `scripts` entry, and `app/package-lock.json`'s
  `packages[""]` records `name, hasInstallScript, workspaces, dependencies, devDependencies` — no `scripts` —
  so `npm ci` at `app/` in the `fe-boundaries` job cannot desync on this diff.
- **The committed locks are cross-platform.** Generated on Windows through `--package-lock-only`, they carry
  every platform variant of each native dependency (`@rolldown/binding-*`, `@tailwindcss/oxide-*`,
  `lightningcss-*`, 38 platform-specific entries in the customer web lock, `linux-x64-gnu` and
  `linux-x64-musl` included), so the Linux runner's `npm ci` resolves its own binaries. Vite 8.2.2 bundles
  through rolldown, so the absence of `@rollup/*` and `@esbuild/*` is the real graph rather than a gap.
- **No machine-specific or secret content reached the locks.** No `file:` specifier, no temp or cache path,
  no user name, no `_authToken`; every `@concertable` entry resolves to a plain content-addressed
  `https://npm.pkg.github.com/download/...` URL.
- **The gate actually fires for this diff.** `test.yml`'s `INERT` pattern excludes `*.md` and the
  `plans/`, `reviews/`, `docs/`, `.agents/`, `.claude/`, `.codex/` trees; the ten `app/**` non-Markdown paths
  are not inert, so `run_fe=true` and both `carve-fe` and `fe-boundaries` run.
- **The test's surface list matches the gate's.** `carve-fe.test.mjs`'s seven surfaces equal `SURFACES` in
  `carve-fe.mjs` and the `carve-fe` matrix in `test.yml`. An eighth surface added without a lock fails the
  carve job on its own, so the duplicated list is not load-bearing enough to be a defect.
- **`--write-lock` is credential-guarded and writes nothing machine-specific.** It falls under the existing
  `!prepareOnly && !GITHUB_PACKAGES_TOKEN` guard, and the carved `.npmrc` carrying the temp cache path is
  never copied back — only the lock is.
- **No dead code from the retired `build-config` special case.** Every remaining import and binding
  (`mkdirSync`, `cache`, `npm`, `npmPrefix`, `readFileSync`, `execFileSync`) still has a live use.
- **Routed `plans` rules hold.** Both documents keep their required headers and the `## Reviews` and
  `## Next Steps` sections; `## Next Steps` opens with the four blocker fields for the 8A gate;
  `plan_graph.py` reports 0 errors and 0 warnings. The 522-line terminal event log was removed as the
  standard's compaction requires, and no fact was copied into a sibling ledger.
- **Comment discipline.** The only comments the candidate adds are the two non-obvious invariants a reader
  needs at that line — npm keeping an already-locked version for a dist-tag spec, and lockstep publication —
  plus the script's own header usage block. `73e36b581` removed the one design-narration comment this pass
  caught before the freeze, so the net diff carries none.

### Not covered by this pass

`npm ci`'s behaviour inside a real carved install is not provable from the frozen tree. A local full carve of
`web/customer` was running at freeze time and the `carve-fe` job over all seven surfaces is the authoritative
evidence; neither is a finding, and both are recorded in the plan ledger's verification section.
