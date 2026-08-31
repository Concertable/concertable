# Repository-per-microservice migration — Customer frontend fold progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\customer-next`
- Branch: `main` at `e21ae9079ca2fdd3a0063a252f05499159d608ff`
- PR: none; the private extraction proof is pushed directly to `main`
- Dependency/package gates: none; this extraction proof is independent of the monorepo Stage 3 and Stage 4 deliveries
- Last reconciled: **2026-08-31** from fetched `origin/main`, exact `ls-remote` equality, local validation, and completed review

## Current state

This stream is terminal. The private `Concertable/customer-next` proof now contains the Customer backend,
web, mobile, and customer-only shared package plus the standalone workspace, lockfile, package-feed,
environment, ignore, tooling, and mobile-asset support closure. `origin/main` exactly equals the reviewed
head `e21ae9079ca2fdd3a0063a252f05499159d608ff`.

No agent following this ledger may monitor or edit rt3, Stage 4 fleet E2E, Auth-next, or any monorepo
migration ledger other than this file. The private proof is not authorization to rename repositories,
change production, or make customer-next canonical.

## Next Steps

No Customer frontend-fold work remains. This terminal ledger is the evidence record for private
`customer-next` head `e21ae9079ca2fdd3a0063a252f05499159d608ff`; reopen it only if that head or a recorded
gate objectively drifts. Canonical rename, deployment, and production cutover begin only under their own
explicitly authorized checkpoint.

## Completed work

- The Customer backend, web, mobile, and `@concertable/customer` histories were folded into private
  `Concertable/customer-next`; local Customer workspaces use `file:` linkage and external
  `@concertable/{shared,web,mobile}` dependencies use the published `alpha` channel.
- `b63a311` made the extracted workspace standalone with its root manifest, lockfile, package feed, ignore
  state, production environment seam, Vite helper, route tree, and canonical `CarveCustomer.slnx`.
- `b484496` restored the complete production URL closure and all four Expo assets; `e21ae90` retired the
  obsolete force-push handoff so this ledger remains the exclusive durable stream record.

## Verification

- `npm ci`: 1,237 packages restored from the committed lockfile.
- `npm run build:shared`: 3/3 Vitest tests passed and the customer-only package built.
- `npm -w @concertable/web-customer test`: 1/1 Vitest test passed.
- `npm run build:web`: shared package, `tsc -b`, and Vite production build passed; the emitted artifact was
  checked for both `https://auth.concertable.co.uk` and `https://business.concertable.co.uk`.
- `npm run build:mobile`: shared build/tests, Customer mobile `tsc --noEmit`, and Android Expo export passed.
- `dotnet build CarveCustomer.slnx --configuration Release`: 51 projects restored and built with 0 errors
  (existing analyzer warnings remain). The earlier `--no-restore` probe was invalid because test-project
  assets were absent; the canonical standalone gate includes restore.

## Reviews

Full and incremental review completed at `e21ae9079ca2fdd3a0063a252f05499159d608ff`; all findings were
resolved and the pushed head was approved with no open findings.

## Decisions, discoveries, blockers, and deviations

- A multi-path fold must include support files outside the selected app subtrees. Customer's relocated Vite
  app uses `app/.env.production`, and Expo's unchanged `../assets/*` references require `app/assets/`.
- B2B established the support-file categories but its local proof did not contain the shared mobile assets;
  compare referenced paths against the extracted tree rather than copying its inventory blindly.
- This is an extraction proof only. Canonical rename, deployment, and production cutover remain separately authorized work.
- This ledger has no write ownership over the Concertable monorepo's rt3 or fleet branches.
