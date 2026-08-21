# Manager Front Page progress

- Plan: `plans/launch/MANAGER_FRONT_PAGE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/manager-front-page`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Feature-launch-dashboard-b2b-consumer`
- Branch: `Feature/launch_dashboard-b2b-consumer`
- PR: [#563](https://github.com/Concertable/concertable/pull/563)
- Dependency/package gates: producer packages are published; this API-changing consumer merge must complete its generated platform-sync follow-through
- Last reconciled: 2026-08-21 against local working tree based on `origin/main` `69df07b8b1ff36e98e82a0c6938b7bb849ee4383`

## Current state

Implementation, authenticated Phase A.8 acceptance, and incremental review are complete. The branch contains the
current-main merge `2900c2c40`, the preserved dashboard validation fixes in `276360edb`, the final acceptance fixes in
`10dcd9313`, and the responsive-search review correction in `c531e4f1a`:

- all four Vite SPAs reuse the already-trusted ASP.NET development certificate, bind explicitly to IPv4, and no
  longer create per-repo `basic-ssl` roots;
- Auth and B2B AppHosts inject their local SPA redirect/CORS origins, so a fresh clone needs no ignored environment
  file to authenticate Venue or Artist;
- Venue and Artist dashboard clients use the current kebab-case API routes;
- the shared manager header collapses behind an accessible mobile menu and dashboard grids constrain horizontal
  strips to their cards;
- the dev seeder idempotently creates immutable contracts for its already-booked applications through
  `ContractEntity.Create`, so advertised contract downloads succeed on both fresh and existing seeded databases.

The exact temporary CurrentUser Root certificate
`4870410CF4DC4717E1E3CFA0E47F05B7F3708781` and `.tmp-vite-localhost.cer` are verified removed. The normal trusted
ASP.NET development certificate remains by design. No local E2E was run.

## Next Steps

1. Run the final plan graph and proportional non-E2E validation, then use the plan-managed two-leg push protocol to
   publish and verify the exact reviewed work head and ledger transport head on PR #563.
2. Require fresh exact-head CI green, complete `/merge`, follow the generated package/platform-sync PR to green and
   merged, then close the source worktree with `./scripts/worktrees.ps1 close -Worktree <path> -PullRequest 563
   -PlanManaged`.
3. From current `origin/main`, record terminal delivery evidence and delete the plan and ledger together in the
   required docs-only closeout change.

## Completed work

- Phase A UI, Phase B backend slices, live-data Phase C cutover, package producers, and consumer integration are
  implemented on PR #563 and its already-delivered prerequisite PRs.
- Reconciled PR #563 with current main in merge commit `2900c2c40`; the only conflicts were
  `ApplicationResponses.cs` and `OpportunityApiTests.cs`, resolved to current-main contracts while retaining
  role-specific dashboard response actions.
- Preserved and committed the Payment provider inventory and Venue activity integration URL corrections in
  `276360edb`; the supplied pre-resume runs were Payment unit tests 489/489 and Venue integration tests 31/31.
- Phase A.8 Browser acceptance passed for authenticated seeded Venue and Artist dashboards at 1440×1000, 834×1112,
  and 390×844. Every dashboard card loaded without an error state or page-level horizontal overflow. Venue decline
  reduced applications-to-review and produced the counterparty inbox item; Artist withdrawal reduced pending
  applications and produced `Application withdrawn.`; the seeded concert contract action completed without the prior
  404 after contract seeding was corrected.

## Verification

- `python .agents/hooks/plan_graph.py --root <worktree>` — 0 errors and 0 warnings after the review checkpoint.
- `dotnet build api/Concertable.B2B/src/Concertable.B2B.AppHost/Concertable.B2B.AppHost.csproj --no-restore` — succeeded
  on the final contract-seed working tree with 0 errors; two NU1900 vulnerability-feed warnings were environmental.
- `npm run build --workspace @concertable/web` followed by Customer, Venue, Artist, and Business web builds — all
  succeeded after the responsive-search review correction; existing chunk-size warnings only.
- `npm test --workspace @concertable/web` — 5 files and 31 tests passed after the review correction.
- Live readiness probes returned Auth discovery 200 with Venue CORS and B2B `/api/auth/me` preflight with the same
  allowed origin.
- Browser Phase A.8 — passed as recorded under Completed work. The attempted Venue checkout correctly exposed a
  separate seeded fake Stripe client secret and was not treated as a successful checkout; the authorized decline and
  withdrawal mutations were used to verify real application writes.
- Local E2E: intentionally not run; the merge queue owns the tier selected by `/merge`.

## Reviews

- Review artifact: `reviews/Feature-launch_dashboard-b2b-consumer.md`.
- Incremental range `bb0e6f3f4..c531e4f1a` (46 commits) was reviewed through both mandatory layers and all routed
  standards. `NAT1` restored a compact search entry point below `lg`; no other findings survived.
- Correctness and security watermarks are both `c531e4f1abdf383c53711b8f261963091b739ebe`.

## Decisions, discoveries, blockers, and deviations

- Manager dashboard URLs are `/`; `/_venue` and `/_artist` are TanStack pathless route identifiers, not browser URLs.
- Local SPA HTTPS is one machine-level ASP.NET development certificate exported into ignored per-surface PEM caches;
  builds and CI never invoke certificate setup. Developers trust it once with `dotnet dev-certs https --trust`.
- Vite binds to `127.0.0.1`; local development no longer depends on IPv6 localhost resolution.
- Shared `@concertable/web` source changes require rebuilding that workspace before standalone SPAs consume its `dist`
  exports.
- Booked dev applications must have contracts because production writes the contract aggregate directly during
  acceptance and the API advertises its action. The seeder does not write event-owned projections or external-provider
  rows.
- Phase A.8 changed only disposable seeded application state: one Venue decline and one Artist withdrawal.

## Resume prompt

```text
cd C:\Users\tommy\source\repos\Concertable\.worktrees\Feature-launch-dashboard-b2b-consumer
Read @plans/launch/MANAGER_FRONT_PAGE_PLAN.md and @plans/launch/MANAGER_FRONT_PAGE_PROGRESS.md and do what its `## Next Steps` says.
```
