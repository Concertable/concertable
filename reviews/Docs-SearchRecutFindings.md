# Review — Docs/SearchRecutFindings

**Reviewed up to commit:** `f5a03360b499`
**Review status:** `complete`
**Judgment:** `approved`

PR #1011 — record what the search re-cut settled.

## Pass 1

**Pass judgment:** `approved`
**Effort:** `low`
**Mode:** `new`

Docs-only: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`, +89/-5, one file. No security
classification — no path matches the generic or repository `security_paths` inventories.

## Findings — none blocking

The substance is accurate and independently corroborated by this session's own work today, which is the
strongest thing that can be said about a findings record:

- The MinVer floor section matches measurements taken separately: search packs height **311**, auth
  published `0.2.0-alpha.0.285`, payment packs **366**, against a monorepo high-water of
  `0.1.0-alpha.0.1393`. Its conclusion — raise the floor rather than rely on a tag, because the floor
  holds whatever the height turns out to be — is the same one reached here, and its claim that no carve
  is accidentally safe is correct: every carve restarts in the same range against the same high-water.
- The `Exists()`-guarded `../BannedSymbols.txt` and `../TestConventions.targets` finding is the named
  defect class this migration keeps producing — a construct chosen for tolerance where the thing it
  tolerates is mandatory — and the record states the right fix (carry the file in and anchor the import
  root-relative) rather than repointing the guard.
- The three-way-merge duplicate `GlobalPackageReference` warning is the useful kind: a clean apply is not
  evidence of a correct one.

## Two staleness notes, not blocking

Both concern events **after** this branch was written, and a dated findings record is allowed to describe
the state it observed. Flagged so the next reader does not act on them as current:

- "`payment`, `customer` and `b2b` each need the identical one-line change" for the `0.2` floor —
  payment landed it in `payment#6`, merged 19:48. Only customer and b2b remain.
- "`customer` … is still unguarded" for the banned-API list and tier gate — `customer#5`, merged 21:18,
  enforces the test-tier gate and carries the guard files to the root.

Neither changes the guidance; both reduce the outstanding set the document names.
