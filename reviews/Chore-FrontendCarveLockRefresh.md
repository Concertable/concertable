# Code review — Chore/FrontendCarveLockRefresh

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `e607a7a6a9aef9ac79a2f8d02df79aa495bcd9bb`  `(2026-09-08)`
**Judgment:** `approved`

## Review pass — 2026-09-08 — full

**Candidate base:** `6cd7fd6158765411c5ae76117e67d0df32f40d77`
**Candidate head:** `e607a7a6a9aef9ac79a2f8d02df79aa495bcd9bb`
**Candidate branch:** `Chore/FrontendCarveLockRefresh`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:45ad3b3c83683e630785ccd82398339d2d5b3301d942b9410c6851e624748984` `(8 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable--worktrees-Refactor-FrontendRegistryPackageConsumers\5c97c8fc-deba-45af-ad34-2124ba6d754e\scratchpad\ref-bundle`
**Candidate bundle identity:** `sha256:0fa7a0588490c6beeacb8cddca5736fb61131717b2d5177cb0e1fcd50841cb86`
**Work-order path:** `reviews/Chore-FrontendCarveLockRefresh.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Seven regenerated lockfiles and this plan's ledger. `skill_router.py` over the frozen path set routes only
`plans`; no path matches the merge gate's generic security patterns or this repo's `security_paths`, so no
security marker is required. Lens dispatch was unavailable, so every layer ran in the parent — the stated
fallback. The bundle carries the exact patch, the NUL path manifest and an identity manifest recording the
frozen tree OID; `tree_export` records `omitted-disk-capacity` rather than claiming an export, because `C:`
was under 1 GB free and no read-only subordinate consumed the tree.

A regenerated lockfile cannot be read line by line, so the review is the per-entry diff against the previous
generation rather than the patch text.

### Findings

No findings.

### Verified

- **Only the tier version moved, plus one named transitive.** Diffing every `packages` entry of all seven
  locks against their `6cd7fd615` state: 3–5 entries changed per lock, of which exactly one is not an
  `@concertable` tier — `electron-to-chromium` `1.5.424` → `1.5.425`, pulled by `browserslist` at
  `^1.5.420`. It is an ISC build-time data package that publishes a patch daily, identical across all seven
  locks, so the set stays internally consistent. Accepted deliberately: hand-editing a generated lock to
  suppress it would make the file unreproducible from `npm run lock:carve` and the next regeneration would
  silently undo the edit. The ledger records the general form of this as a Phase 4 requirement — whatever
  automates the refresh must surface the non-tier delta, since that delta is the only part a reviewer judges.
- **Every tier is on the new lockstep version.** All `@concertable` entries in all seven locks are
  `0.1.0-alpha.0.6462`, matching the `alpha` dist-tag the 8B merge's publish run 34284459421 produced, and
  each lock's total package count is unchanged from the previous generation
  (642/641/642/618/600 web, 878/877 mobile).
- **Root specifiers are untouched.** Every `packages[""]` `@concertable` specifier is still `alpha` in all
  seven, so the locks continue to agree with the manifests the carve rewrites.
- **No machine-specific or secret content.** No `file:` specifier, temp or cache path, user name, or
  `_authToken` in any of the seven.
- **The refresh cannot cascade.** `app/package.json` is not in this diff, and the per-surface locks live at
  `app/<surface>/package-lock.json`, which the publish workflow's `app/package-lock.json` path entry does not
  match — so nothing here re-triggers a frontend publish.
- **Routed `plans` rules hold.** `plan_graph.py` reports 0 errors and 0 warnings; the ledger's PR header,
  dependency gate, verification, decisions and `## Reviews` entries are current, and `## Next Steps` names
  this PR as the live gate with the 8A blocker fields intact below it.
- **The invariant test still passes.** `node --test scripts/carve-fe.test.mjs` is 8/8, including the
  lockstep-pin check across all seven surfaces.

### Not covered by this pass

That `npm ci` accepts these regenerated locks inside a real carved install is proved by the `carve-fe` matrix
on this PR, not from the frozen tree.
