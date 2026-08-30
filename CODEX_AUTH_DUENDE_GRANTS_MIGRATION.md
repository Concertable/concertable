# Move Auth's Duende persisted grants from B2BDb to AuthDb

You are working alone with no memory of any prior conversation. This file is your complete brief.
Work independently; do not wait for further instructions unless you hit a genuine blocker.

## Context

`Concertable/concertable` is a .NET/React monorepo mid-migration to repository-per-microservice. Read
`plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` and its `_PROGRESS.md` ledger for full
background before doing anything else — specifically Stage 6's entry: Auth's backend extraction-and-build
proof is already done and pushed to private `Concertable/auth-next`, but **"still needs Duende persisted
grants moved from `B2BDb` to `AuthDb` before this can become canonical (a separate, unstarted piece of
work — the private repo is a proof, not yet the real cutover target)."**

**This is live production data** (persisted grants back real user sessions/logins via Duende IdentityServer).
Treat it accordingly — this is not a mechanical carve like the extraction proofs.

## What I want from you

1. Find where Duende's persisted-grant store is currently configured against `B2BDb` (likely
   `AddOperationalStore` or equivalent in Auth's or B2B's startup/DI wiring) and confirm the actual tables
   involved (`PersistedGrants`, `DeviceCodes`, etc. — Duende's standard operational-store schema).
2. Confirm `AuthDb` exists as a real, separate database context today, and what it currently owns.
3. Propose a migration approach that does not cause a live-traffic outage or silently drop in-flight
   sessions/grants — e.g. dual-write or a cutover window with a real migration script, not just "change
   the connection string and hope." Consider: is a hard cutover acceptable (grants are short-lived and
   losing in-flight ones just forces re-auth), or does it need an actual data copy? Investigate before
   assuming either.
4. Do not execute a destructive step (dropping the old table/store, changing production connection
   strings) without stopping to report your plan first. This one specifically warrants a checkpoint before
   the irreversible step, unlike the disposable filter-repo clones the rest of this migration has been
   using.

## Do not

- Do not touch `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PROGRESS.md` — a separate process
  owns folding results into that shared ledger.
- Do not touch any other stage's work (frontend folds, Stage 3 round-trips, Stage 4's TestKit) — all
  running concurrently, all independent of this.
- Do not push this straight to `main` or bypass the normal PR/merge-queue path — this is real schema/data
  work on the canonical repo, not a private staging-repo proof. Open a normal PR.

## Stop and report

Stop and report before any irreversible step (schema change, data migration execution, connection-string
cutover). Otherwise report: current grant-store configuration found, `AuthDb`'s current scope, the
migration approach you're proposing and why, and what you've verified so far (e.g. against a local/test
database, never production).
