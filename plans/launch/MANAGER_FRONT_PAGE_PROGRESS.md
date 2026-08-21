# Manager Front Page progress

- Plan: `plans/launch/MANAGER_FRONT_PAGE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/manager-front-page`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Feature-launch-dashboard-b2b-consumer`
- Branch: `Feature/launch_dashboard-b2b-consumer`
- PR: [#563](https://github.com/Concertable/concertable/pull/563)
- Dependency/package gates: producer packages are published; this API-changing consumer merge must complete its generated platform-sync follow-through
- Last reconciled: 2026-08-21 against `origin/main` `a364bebbdc5e75e28adf09b304513e9dcab72f1f`

## Current state

Implementation, authenticated Phase A.8 acceptance, current-main reconciliation, and incremental review are complete.
The exact reviewed work head is `27e51f65c422959ebda09893abeac603c6fb5a1f`. It includes current-main merge
`3a9cc268e`, the later platform-sync-only merge `27e51f65c`, the preserved dashboard validation fixes in `276360edb`,
the final acceptance fixes in `10dcd9313`, and the review corrections in `c531e4f1a`, `9a07f342f`, and `4d74f773d`:

- all five Vite SPAs reuse the already-trusted ASP.NET development certificate, bind explicitly to IPv4, and no
  longer create per-repo `basic-ssl` roots;
- Auth and B2B AppHosts inject the complete local SPA redirect/CORS origins, so a fresh clone needs no ignored
  environment file to authenticate Venue, Artist, or Admin;
- the B2B composition suite pins the complete Venue/Artist/Business/Admin resource-port roster to its Auth and backend
  origins, preventing future surfaces from silently repeating the Admin omission;
- Venue and Artist dashboard clients use the current kebab-case API routes;
- the shared manager header collapses behind an accessible mobile menu and dashboard grids constrain horizontal
  strips to their cards;
- the dev seeder idempotently creates immutable contracts for its already-booked applications through
  `ContractEntity.Create`, so advertised contract downloads succeed on both fresh and existing seeded databases.

The exact temporary CurrentUser Root certificate
`4870410CF4DC4717E1E3CFA0E47F05B7F3708781` and `.tmp-vite-localhost.cer` are verified removed. The normal trusted
ASP.NET development certificate remains by design. No local E2E was run.

The work-head push advanced the remote/PR from `0c09697b9906fc3f34a566a25fdac4771cabef50` through
`0c09697b..27e51f65c`; refreshed `origin/Feature/launch_dashboard-b2b-consumer` and PR #563 both equal the exact
reviewed work head. Review transport commit `b741b6123` is local and pending the checkpoint-transport push.

## Next Steps

1. Push the pending review and checkpoint transport, fetch, and require local, remote-tracking, and PR heads to match.
2. Require fresh exact-head CI green, complete `/merge`, follow the generated package/platform-sync PR to green and
   merged, then close the source worktree with `./scripts/worktrees.ps1 close -Worktree <path> -PullRequest 563
   -PlanManaged`.
3. From current `origin/main`, record terminal delivery evidence and delete the plan and ledger together in the
   required docs-only closeout change.

## Completed work

- Phase A UI, Phase B backend slices, live-data Phase C cutover, package producers, and consumer integration are
  implemented on PR #563 and its already-delivered prerequisite PRs.
- Reconciled PR #563 with the originally named current main in merge commit `2900c2c40`; the only conflicts were
  `ApplicationResponses.cs` and `OpportunityApiTests.cs`, resolved to current-main contracts while retaining
  role-specific dashboard response actions.
- Reconciled again after main advanced through Admin and platform-sync PR #709 in merge commits `3a9cc268e` and
  `27e51f65c`; the merges were conflict-free. Admin now shares the permanent development HTTPS/IPv4 configuration and
  has complete Auth/B2B origin wiring.
- Preserved and committed the Payment provider inventory and Venue activity integration URL corrections in
  `276360edb`; the supplied pre-resume runs were Payment unit tests 489/489 and Venue integration tests 31/31.
- Phase A.8 Browser acceptance passed for authenticated seeded Venue and Artist dashboards at 1440×1000, 834×1112,
  and 390×844. Every dashboard card loaded without an error state or page-level horizontal overflow. Venue decline
  reduced applications-to-review and produced the counterparty inbox item; Artist withdrawal reduced pending
  applications and produced `Application withdrawn.`; the seeded concert contract action completed without the prior
  404 after contract seeding was corrected.

## Verification

- `python .agents/hooks/plan_graph.py --root <worktree>` — 0 errors and 0 warnings after the review checkpoint.
- `dotnet build api/Concertable.B2B/src/Concertable.B2B.AppHost/Concertable.B2B.AppHost.csproj` — restored the current
  `0.1.0-alpha.0.1120` platform pin and succeeded with 0 errors; pre-existing CS0628 and vulnerability-feed warnings
  remain.
- `dotnet test api/Concertable.B2B/tests/Concertable.B2B.CompositionTests/Concertable.B2B.CompositionTests.csproj
  --no-build --no-restore` — 6/6 passed, including the new complete SPA-origin regression.
- `npm run build --workspace @concertable/web` followed by Customer, Venue, Artist, Business, and Admin web builds —
  all succeeded; existing chunk-size warnings only.
- `npm test --workspace @concertable/web` — 5 files and 31 tests passed after the review correction.
- Live readiness probes returned Auth discovery 200 with Venue CORS and B2B `/api/auth/me` preflight with the same
  allowed origin.
- Browser Phase A.8 — passed as recorded under Completed work. The attempted Venue checkout correctly exposed a
  separate seeded fake Stripe client secret and was not treated as a successful checkout; the authorized decline and
  withdrawal mutations were used to verify real application writes.
- Local E2E: intentionally not run; the merge queue owns the tier selected by `/merge`.
- Work-head push: starting remote/PR `0c09697b9906fc3f34a566a25fdac4771cabef50`; pushed range
  `0c09697b..27e51f65c`; refreshed remote-tracking and PR heads both verified at
  `27e51f65c422959ebda09893abeac603c6fb5a1f`.

## Reviews

- Review artifact: `reviews/Feature-launch_dashboard-b2b-consumer.md`.
- Incremental ranges through `c531e4f1a..27e51f65c` were reviewed through both mandatory layers and all mechanically
  routed current-main standards. `NAT2` and `NAT3` added Admin Auth/CORS wiring, `CI5` moved Admin onto the shared
  HTTPS/IPv4 setup, and `NAT4` added the AppHost composition regression. No open findings remain.
- Correctness and security watermarks are both `27e51f65c422959ebda09893abeac603c6fb5a1f`.
- Review transport: `b741b6123` (pending checkpoint-transport push).

## Decisions, discoveries, blockers, and deviations

- Manager dashboard URLs are `/`; `/_venue` and `/_artist` are TanStack pathless route identifiers, not browser URLs.
- Local SPA HTTPS is one machine-level ASP.NET development certificate exported into ignored per-surface PEM caches;
  builds and CI never invoke certificate setup. Developers trust it once with `dotnet dev-certs https --trust`.
- Vite binds to `127.0.0.1`; local development no longer depends on IPv6 localhost resolution.
- Shared `@concertable/web` source changes require rebuilding that workspace before standalone SPAs consume its `dist`
  exports.
- Every B2B web SPA is now asserted from the composed AppHost model against its development port and backend origin;
  authenticated surfaces additionally assert redirect, logout, and Auth CORS values.
- Booked dev applications must have contracts because production writes the contract aggregate directly during
  acceptance and the API advertises its action. The seeder does not write event-owned projections or external-provider
  rows.
- Phase A.8 changed only disposable seeded application state: one Venue decline and one Artist withdrawal.

## Resume prompt

```text
cd C:\Users\tommy\source\repos\Concertable\.worktrees\Feature-launch-dashboard-b2b-consumer
Read @plans/launch/MANAGER_FRONT_PAGE_PLAN.md and @plans/launch/MANAGER_FRONT_PAGE_PROGRESS.md and do what its `## Next Steps` says.
```
