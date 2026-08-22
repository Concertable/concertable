# Manager Front Page progress

- Plan: `plans/launch/MANAGER_FRONT_PAGE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/manager-front-page`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Feature-launch-dashboard-b2b-consumer`
- Branch: `Feature/launch_dashboard-b2b-consumer`
- PR: [#563](https://github.com/Concertable/concertable/pull/563)
- Dependency/package gates: producer packages are published; this API-changing consumer merge must complete its generated platform-sync follow-through
- Last reconciled: 2026-08-22 against `origin/main` `1c63e4f6b` in merge `6b48d0e5f`

## Current state

Implementation and authenticated Phase A.8 acceptance are complete. PR #563 was current, freshly green, and entered
the merge queue at `8a6086b07f1185bc4338c22d6e1cd844fce232ce`; API E2E and both UI E2E blocks passed, but a Venue
integration runner wedged inside `actions/cache@v4` before downloading artifacts or executing tests. The capped wait
was exhausted and the run was cancelled once. `origin/main` then advanced 16 commits to `7f107d98b`; that base was
merged cleanly in `2a0db5198`, preserving every dashboard change and known local fix. Proportional validation and the
mandatory native/security incremental review are green at exact reviewed work head
`98b526b57d1c5d9ea6f609e290f982094bff2d8b`. The exact reviewed work head was pushed from `8a6086b07`; refreshed
remote-tracking and PR heads both equalled `98b526b57d1c5d9ea6f609e290f982094bff2d8b`. Review transport and the verified
push checkpoint were then transported at `fcaa35f5d`; fresh exact-head CI run `32538773760` passed 73 checks with 3
expected merge-group-only skips. A final two-commit main advance was then merged cleanly at `6b48d0e5f`; it only moves
all five service pins to the already-green `0.1.0-alpha.0.1128` platform package. Incremental review found and fixed
one stale ledger statement; exact reviewed work head is now `b4eb94e5005a264bb575dcb91a7d8a2a2261f32d` and its
native/security correction-tail re-reviews are clean. That exact work head was pushed from `fcaa35f5d`; refreshed
remote-tracking and PR heads both equal `b4eb94e5005a264bb575dcb91a7d8a2a2261f32d`. The review transport is the only
remaining local tail. The delivered work includes:

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
reviewed work head. Review transport `b741b6123` and push checkpoint `77b23dfc4` were then transported and verified
with local, remote-tracking, and PR heads equal. PR #563 is ready for review and current with `origin/main`
`a364bebbd`. Fresh CI run `32521884372` failed `carve-fe (web/b2b/venue)` at job `96895902261`: the standalone
carve flattened the SPA subtree and omitted `app/scripts/vite-development-https.ts`, so Venue's legitimate
app-relative helper import could not resolve on Linux. The correction now preserves the isolated `app/<surface>`
layout, archives the shared helper from the same Git tree, covers all five web SPAs with a no-network regression,
and adds Admin to the authoritative carve matrix. The exact reviewed correction work head `c2a69d062` was pushed
from starting remote/PR head `97765c2b1`; refreshed remote-tracking and PR heads both equal
`c2a69d062d79685c59590e4f94569949fc9d88a9`. Review transport and the verified-push checkpoint were subsequently
pushed and verified with local, remote-tracking, and PR heads equal at `6e17b5cdf0067833bda18d6d396e240a31f91b6a`.

`origin/main` then advanced to `20012d1a8`. The reconciliation preserves current main's `InsertAsync` conversions
where each insert is the only staged write, while `MessageService` deliberately retains `AddAsync` inside the
ambient outbox unit of work because the message and `TenantActivityRecordedEvent` must commit atomically. The merged
unit test now pins that exception explicitly. Incremental review also found that current main's platform-sync
supersession guard treated every clean PR as in-flight. It now preserves only a clean PR with auto-merge actually
armed, with the state decision isolated in a repository-owned helper and exercised by the required CI aggregate.
The exact reviewed work head `652fd3aac91e5ba6689530efe6ad113c84a42772` was pushed from starting remote/PR
head `6e17b5cdf0067833bda18d6d396e240a31f91b6a`; refreshed remote-tracking and PR heads both equal the reviewed
work head. Review transport and the verified-push checkpoint were then transported and verified at
`3d4f9c5ac3669a8e9ebb8087fddaa3e1f46a51a3`. Fresh exact-head CI run `32528668315` proved every frontend
carve green, then failed the solution build because two dashboard unit-test arrangements still instantiated the
removed `FlatFeeDeal` type. Both now use the current `FlatFeeDealDto` contract, and the complete Concert unit suite
passes locally. The exact reviewed correction work head `4c28ab7f7305f4fec2ffa91f0674cc99fc81cb47` was pushed
from starting remote/PR head `3d4f9c5ac3669a8e9ebb8087fddaa3e1f46a51a3`; refreshed remote-tracking and PR heads
both equal the reviewed correction.

Compiler-correction review transport `7484a8d3a` and push checkpoint `3ebc4722f` are now transported; local,
remote-tracking, and PR heads were verified equal at `3ebc4722f160ed69b724d7f46e44cb6fb76c5f03`. Fresh CI run
`32529834454` has a green solution build, workflow policy tests, hooks, frontend boundaries, platform pack, every
frontend carve, and its completed backend jobs, with no failures; remaining unit/composition jobs are still pending.
This run cannot authorize queue admission because refreshed `origin/main` is now six commits ahead.

## Next Steps

1. Transport the final review and verified-push checkpoint, then require fresh exact-head CI green.
2. Recheck base currency, re-enqueue with `full-e2e`, complete `/merge`, follow the generated
   package/platform-sync PR to green and
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
- Reconciled with current main `20012d1a8`; the sole conflict was `MessageService.cs`, resolved by retaining the
  dashboard's atomic message-plus-outbox transaction while accepting main's `InsertAsync` conversions everywhere
  they satisfy the single-staged-write contract.
- Reconciled cleanly with current main `2323c77e7` in merge `99412536c`; the incoming six commits are architecture
  guidance cleanup plus the green `0.1.0-alpha.0.1124` platform sync, and all known dashboard fixes remain intact.
- Reconciled cleanly with current main `7f107d98b` in merge `2a0db5198`; the incoming runtime delta splits the Admin
  repository by entity capability and the remaining changes repoint split-repo build metadata. No dashboard file
  conflicted or was overwritten.
- Preserved and committed the Payment provider inventory and Venue activity integration URL corrections in
  `276360edb`; the supplied pre-resume runs were Payment unit tests 489/489 and Venue integration tests 31/31.
- Phase A.8 Browser acceptance passed for authenticated seeded Venue and Artist dashboards at 1440×1000, 834×1112,
  and 390×844. Every dashboard card loaded without an error state or page-level horizontal overflow. Venue decline
  reduced applications-to-review and produced the counterparty inbox item; Artist withdrawal reduced pending
  applications and produced `Application withdrawn.`; the seeded concert contract action completed without the prior
  404 after contract seeding was corrected.

## Verification

- `python .agents/hooks/plan_graph.py --root <worktree>` — 0 errors and 0 warnings after the review checkpoint.
- Current-main reconciliation: Conversations unit tests passed 46/46; the B2B AppHost build succeeded with 0 errors
  (two pre-existing vulnerability-feed availability warnings); the plan graph remained at 0 errors and 0 warnings.
- Current-main `2323c77e7` reconciliation: the B2B AppHost restored platform `0.1.0-alpha.0.1124` and built with 0
  errors; Conversations tests passed 46/46; Concert tests passed 233/233; the plan graph passed with 0 errors and 0
  warnings. The known Venue activity URL and both dashboard payout-status inventory calls remain present.
- Current-main `7f107d98b` reconciliation: B2B AppHost built with 0 errors; Admin passed 32/32, Conversations 46/46,
  and Concert 233/233; plan graph passed 0 errors/0 warnings. An initial concurrent local test invocation caused one
  shared-`obj` Windows file lock; the affected Conversations suite passed when rerun alone.
- Platform-sync supersession policy tests passed 4/4, both touched workflow YAML files parsed successfully, and the
  helper passed Node syntax validation.
- Fresh CI run `32528668315`: workflow tests, hooks, frontend boundaries, platform pack, and all seven frontend carves
  passed; the solution build failed on two stale `FlatFeeDeal` test arrangements. After replacing them with
  `FlatFeeDealDto`, Concert unit tests pass 233/233 locally.
- Fresh CI run `32529834454` at exact PR head `3ebc4722f`: all completed jobs are green, including the corrected full
  solution build; backend unit/composition jobs remain pending. Base currency was rechecked during the run and found
  `origin/main` six commits ahead at `2323c77e7`, so this run is evidence only and will not be used to enqueue.
- `dotnet build api/Concertable.B2B/src/Concertable.B2B.AppHost/Concertable.B2B.AppHost.csproj` — restored the current
  `0.1.0-alpha.0.1120` platform pin and succeeded with 0 errors; pre-existing CS0628 and vulnerability-feed warnings
  remain.
- `dotnet test api/Concertable.B2B/tests/Concertable.B2B.CompositionTests/Concertable.B2B.CompositionTests.csproj
  --no-build --no-restore` — 6/6 passed, including the new complete SPA-origin regression.
- `npm run build --workspace @concertable/web` followed by Customer, Venue, Artist, Business, and Admin web builds —
  all succeeded; existing chunk-size warnings only.
- `npm test --workspace @concertable/web` — 5 files and 31 tests passed after the review correction.
- `npm run test:boundaries` — 7/7 frontend-tooling tests passed, including all five web carve layouts resolving the
  shared Vite HTTPS helper; `npm run lint:boundaries` passed every workspace boundary.
- A full local Venue feed restore reached GitHub Packages but the workstation `gh` token lacks `read:packages` and
  received 403. No credential or gate was weakened; exact-head CI carries the package-scoped token and remains the
  authoritative standalone restore/build verification.
- Live readiness probes returned Auth discovery 200 with Venue CORS and B2B `/api/auth/me` preflight with the same
  allowed origin.
- Browser Phase A.8 — passed as recorded under Completed work. The attempted Venue checkout correctly exposed a
  separate seeded fake Stripe client secret and was not treated as a successful checkout; the authorized decline and
  withdrawal mutations were used to verify real application writes.
- Local E2E: intentionally not run; the merge queue owns the tier selected by `/merge`.
- Work-head push: starting remote/PR `0c09697b9906fc3f34a566a25fdac4771cabef50`; pushed range
  `0c09697b..27e51f65c`; refreshed remote-tracking and PR heads both verified at
  `27e51f65c422959ebda09893abeac603c6fb5a1f`.
- Carve-correction work-head push: starting remote/PR `97765c2b1e59c239064f03c643563fb0c4dcb4c5`; pushed
  `97765c2b1..c2a69d062`; refreshed remote-tracking and PR heads both verified at
  `c2a69d062d79685c59590e4f94569949fc9d88a9`.
- Latest reviewed work-head push: starting remote/PR `6e17b5cdf0067833bda18d6d396e240a31f91b6a`; pushed
  `6e17b5cdf..652fd3aac`; refreshed remote-tracking and PR heads both verified at
  `652fd3aac91e5ba6689530efe6ad113c84a42772`.
- Compiler-correction work-head push: starting remote/PR `3d4f9c5ac3669a8e9ebb8087fddaa3e1f46a51a3`; pushed
  `3d4f9c5ac..4c28ab7f7`; refreshed remote-tracking and PR heads both verified at
  `4c28ab7f7305f4fec2ffa91f0674cc99fc81cb47`.
- Final transport at this base: review transport `7484a8d3a` and verified-push checkpoint `3ebc4722f`; local,
  remote-tracking, and PR heads all verified at `3ebc4722f160ed69b724d7f46e44cb6fb76c5f03`.
- Current-main reconciliation work-head push: starting remote/PR `3ebc4722f160ed69b724d7f46e44cb6fb76c5f03`;
  pushed `3ebc4722f..c6cc262b8`; refreshed remote-tracking and PR heads both verified at
  `c6cc262b8dddcec7108d987d04d4940c891d38e4`.
- Exact transported PR head `8a6086b07f1185bc4338c22d6e1cd844fce232ce` passed fresh PR CI run `32531796625`
  (73 pass, 3 expected merge-group-only skips). Merge-group run `32534078615` then passed API E2E and both B2B and
  Customer UI E2E blocks, but one Venue integration runner remained in `actions/cache@v4` for 97 minutes before any
  artifact download or test execution. The capped infrastructure wait was exhausted, the wedged run was cancelled
  once, and GitHub removed #563 from the queue. Main then proved 16 commits ahead at `7f107d98b`; no stale re-enqueue
  will be attempted.
- Second reconciliation work-head push: starting remote/PR `8a6086b07f1185bc4338c22d6e1cd844fce232ce`;
  pushed `8a6086b07..98b526b57`; refreshed remote-tracking and PR heads both verified at
  `98b526b57d1c5d9ea6f609e290f982094bff2d8b`.
- Final platform-sync reconciliation work-head push: starting remote/PR `fcaa35f5d3b14b776f0cf0e69db30654bc245698`;
  pushed `fcaa35f5d..b4eb94e50`; refreshed remote-tracking and PR heads both verified at
  `b4eb94e5005a264bb575dcb91a7d8a2a2261f32d`.

## Reviews

- Review artifact: `reviews/Feature-launch_dashboard-b2b-consumer.md`.
- Incremental ranges through `c531e4f1a..27e51f65c` were reviewed through both mandatory layers and all mechanically
  routed current-main standards. `NAT2` and `NAT3` added Admin Auth/CORS wiring, `CI5` moved Admin onto the shared
  HTTPS/IPv4 setup, and `NAT4` added the AppHost composition regression. No open findings remain.
- Incremental range `27e51f65c..c2a69d062` (5 commits) was reviewed through the mandatory native and security layers,
  all mechanically routed standards, and the six architecture/correctness/test lenses. No findings remain.
- Incremental range `c2a69d062..652fd3aac` (17 commits) produced and resolved NAT5: a clean-but-idle platform-sync PR
  could be mistaken for an in-flight queued PR. The correction tail passed the mandatory native and security
  re-review with no further findings.
- Correctness watermark is `4c28ab7f7305f4fec2ffa91f0674cc99fc81cb47`; the security watermark remains
  `652fd3aac91e5ba6689530efe6ad113c84a42772` because the later delta contains no security-sensitive path.
- Review transport `03cc8ff90` and push checkpoint `3d4f9c5ac` are verified on the remote and PR.
- Incremental range `652fd3aac..4c28ab7f7` (3 commits) was reviewed through the mandatory native layer and all routed
  standards with no findings; Concert unit tests pass 233/233.
- Compiler-correction review transport `7484a8d3a` and checkpoint `3ebc4722f` are verified on the remote and PR.
- Incremental range `4c28ab7f7..c6cc262b8` (13 commits) was reviewed through the mandatory native and security layers,
  all routed current-main standards, and all six repository lenses. `COR4` corrected the ledger's stale claim that
  local, remote, and PR heads still matched after creating the reconciliation tail. No other findings remain; both
  correction-tail re-reviews were clean. Correctness and security watermarks are `c6cc262b8dddcec7108d987d04d4940c891d38e4`.
- Incremental range `c6cc262b8..98b526b57` (22 commits) was reviewed through both mandatory layers, every mechanically
  routed current-main standard, and all six repository lenses with no findings. Correctness and security watermarks
  are `98b526b57d1c5d9ea6f609e290f982094bff2d8b`.
- Incremental range `98b526b57..b4eb94e50` (4 commits) produced and resolved COR5, correcting the final pin-only
  reconciliation state in this ledger. The correction tail passed both mandatory re-review layers with no findings;
  correctness and security watermarks are `b4eb94e5005a264bb575dcb91a7d8a2a2261f32d`.

## Decisions, discoveries, blockers, and deviations

- Manager dashboard URLs are `/`; `/_venue` and `/_artist` are TanStack pathless route identifiers, not browser URLs.
- Local SPA HTTPS is one machine-level ASP.NET development certificate exported into ignored per-surface PEM caches;
  builds and CI never invoke certificate setup. Developers trust it once with `dotnet dev-certs https --trust`.
- Vite binds to `127.0.0.1`; local development no longer depends on IPv6 localhost resolution.
- Shared `@concertable/web` source changes require rebuilding that workspace before standalone SPAs consume its `dist`
  exports.
- Frontend carves preserve the real `app/<surface>` hierarchy inside an otherwise isolated temporary root and archive
  explicit shared build inputs from the same Git tree. Flattening a surface cannot support legitimate app-relative
  build-tool imports. Admin is a first-class web carve alongside Customer, Venue, Artist, and Business.
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
