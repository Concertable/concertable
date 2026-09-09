# Code review — Refactor/B2bPackageTopologyPhase3-Producer

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `8ecd0aceee2bc5c394f22a6859ae20e548ac8e6f`  `(2026-09-09)`
**Security-reviewed up to commit:** `8ecd0aceee2bc5c394f22a6859ae20e548ac8e6f`  `(2026-09-09)`
**Judgment:** `approved`

## Review pass — 2026-09-08 — full

**Candidate base:** `ef8d505fdb0133d8b967d58634022192169e90e1`
**Candidate head:** `6474cec75d05fecf1dcd0cbc4a4c21a42bc6b9ac`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:852872e992669e8c181638554697cd1831a682828d6dc6b657aae54441a2e6db` `(17 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable--worktrees-Refactor-B2bPackageTopologyPhase3-Producer\faa2f74d-3e01-4532-9f20-974aa4d8839d\scratchpad\review-bundle-951`
**Candidate bundle identity:** `sha256:4e16ca982f52ffa60a4dbaa63604854eb45b9bd5abed3858ff83137d2745314b`
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Routed rules re-read against the frozen tree: `app-tiers`, `react-standards:tiered-shared-code`,
`server-state`, `react-standards:client-state`, `react:client-state`, `react:identity`,
`react:permissions`, `react-standards:typescript-style`, `react:typescript-style`, `frontend-testing`,
`plans`. No frozen path matches the merge gate's generic or repository `security_paths` inventory, so this
pass carries no security marker.

### Findings

- [x] **TS1 — MEDIUM — correctness/concurrency** — `app/b2b/shared/src/features/tenant/tenantSession.ts:56`
  `select()` now defers its `activeTenantId` commit until the storage write settles and serializes those
  writes through `enqueue`, but `configure()` and `resolve()` — the session's two other writers of
  `activeTenantId` and of `TenantStorage` — still run outside that queue. `useTenant`'s memberships effect
  calls `resolve()` on every membership change, so a `resolve()` landing while a `select()` save is in
  flight reads the pre-commit `activeTenantId`, and `synchronizeTenant` + `persistSelection` can write a
  different tenant to storage concurrently with `saveActiveTenantId`, leaving the store and the persisted
  tenant disagreeing after reload. A `configure()` re-hydration in the same window is likewise clobbered by
  the later-settling select. Fix: route `configure`'s load + `hydrateTenant` and `resolve`'s
  `synchronizeTenant` + `persistSelection` through the same `enqueue`, so one queue orders every write to
  `activeTenantId` and to storage.

- [x] **TS2 — MEDIUM — correctness** — `app/b2b/shared/src/features/artists/hooks/useMyArtist.ts:139`
  `useArtistQuery`/`useVenueQuery` gained `enabled: tenantId !== undefined`, but `useMyArtist` and
  `useMyVenue` still return `isLoading: query.isLoading`. A disabled TanStack Query v5 query is
  `status: "pending"` with `fetchStatus: "idle"`, so `isLoading` is `false` while `data` is `undefined` and
  `isError` is `false`. Every consumer therefore sees the settled-and-empty result while the tenant is still
  resolving and renders the "no profile yet" / create-profile branch before falling back. Fix: treat an
  unresolved tenant as loading — `isLoading: tenantId === undefined || query.isLoading` in both hooks
  (`app/b2b/shared/src/features/venues/hooks/useMyVenue.ts:170`).

- [x] **TS3 — MEDIUM — published contract** — `app/b2b/shared/src/features/artists/hooks/useArtistQuery.ts:11`
  `useArtistQuery(tenantId?: string)`, `useVenueQuery(tenantId?: string)`,
  `useUpdateArtistMutation(tenantId?: string)` and `useUpdateVenueMutation(tenantId?: string)` take the
  tenant optionally, while `useMyArtist`/`useMyVenue` take it as a required positional. A consumer of the
  published package that omits it still compiles: the query is silently disabled forever, and the mutation
  writes its result to `["artist", "my", undefined]`, a key no reader observes, so a saved profile silently
  never refreshes. This publication boundary exists precisely so a mis-scoped consumer fails to compile.
  Fix: make the parameter required (`tenantId: string | undefined`) on all four, matching `useMyArtist`.

### Remediation

All three fixed on this branch before delivery.

- TS1 — `enqueue` is now generic and returns its operation's value; `configure`'s
  `loadActiveTenantId` + `hydrateTenant` and `resolve`'s `synchronizeTenant` + `persistSelection` run
  inside it, so one queue orders every write to `activeTenantId` and to `TenantStorage`. `resolve` also
  captures the selection generation and yields to a `select`/`clear` that superseded it. New test
  `orders a route resolve behind an in-flight selection` fails against the pre-fix implementation, which
  cleared storage while a selection save was still in flight.
- TS2 — `useMyArtist` and `useMyVenue` report `isLoading: tenantId === undefined || query.isLoading`.
  Verified against the pinned runtime: `@tanstack/query-core` computes `isLoading = isPending && isFetching`,
  so a disabled query reports `false`.
- TS3 — `useArtistQuery`, `useVenueQuery`, `useUpdateArtistMutation` and `useUpdateVenueMutation` now take
  `tenantId: string | undefined` as a required parameter. The emitted `dist` declarations carry the tenant
  argument, which is the exact shape the previous publication round was missing.

## Review pass — 2026-09-08 — incremental

**Candidate base:** `6474cec75d05fecf1dcd0cbc4a4c21a42bc6b9ac`
**Candidate head:** `997deab6360bd6fd97a4c012a3f3c0f544d28378`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:84299898ee70d5bdf58e10cabd341748a8448f8640272adf9cdbcfe4f77a6e1b` `(10 paths)`
**Candidate bundle:** derived in place from the frozen range; the pass-1 disposable bundle was removed on
completion
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No issues found in the remediation delta. Checked the queue's failure and ordering paths (`enqueue`'s
chain still cannot reject the queue itself, while the caller's promise still rejects for the latest
selection), the generation guard's interaction with `select` and `clear`, the `isLoading` derivation
against the pinned `query-core` implementation, and the four widened signatures against every caller in
the workspace. Every remaining `void`ed session call was already unawaited before this branch. No
security-sensitive path is touched.

### Carried to the consumer PR

Neither is a defect on this branch, and neither is an open finding here; both are checks PR #950 owns
because it owns the files.

- **C1** — `useMyArtist`/`useMyVenue` now report
  `isLoading: true` while no tenant is resolved, which is correct only because a consumer never mounts
  them for a manager who has no matching membership. The web SPAs satisfy that through their
  `requireArtist`/`requireVenue` route guards. The mobile B2B screens have no equivalent guard, so #950's
  review must confirm `RootNavigator`/`TenantChooser` keep `MyArtistScreen`/`MyVenueScreen` unmounted
  until a membership resolves, rather than leaving a membership-less user on a permanent spinner.
- **C2** — `app/web/b2b/artist/src/features/artist/guards.ts` calls `artistApi.getArtist()` directly from
  `beforeLoad`, outside React Query. The `server-state` standard puts every server read behind a
  `useQuery`/route loader that shares the cache; as written the guard refetches the profile on every
  navigation and its result never seeds `artistKeys.myForTenant`. Raise it against #950, which owns that
  file.

## Review pass — 2026-09-08 — incremental

**Candidate base:** `997deab6360bd6fd97a4c012a3f3c0f544d28378`
**Candidate head:** `db82bf40333620dae19e3320a05e085fa89cce17`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:a01edcbb0370f7279027f0b22c61f86071fe2b0149f6e80fe369125e270f6313` `(34 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No issues found. The range is the completed review work order from the previous pass plus a second
conflict-free merge of `origin/main`, taken because platform-sync PR #961 landed on base and the merge
skill treats a landed version bump as a required base update. `git diff d78d4708a..db82bf403 -- app/`
is empty, so the merge changed nothing this branch authors; the incoming `api/`, `eng/` and `reviews/`
content is `main`'s own, already reviewed on PRs #959 and #961. This branch's delta against `origin/main`
is unchanged at the 16 `app/b2b/shared` files plus its ledger and this work order, and it still matches no
`security_paths` pattern.

## Review pass — 2026-09-08 — incremental

**Candidate base:** `844fb94368e5fe8cdd71b01350289ed376373600`
**Candidate head:** `f3c5d9ed4e1bbc24a64a65254ee6e19b3290c62f`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:d70d10ad0c489c2725a2e1604ffd85f2596cf208e908a06a0c1c44ea72aff1e3` `(15 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No issues found. The range is a third conflict-free merge of `origin/main` plus one ledger entry. The
merge was taken because PR #964 landed the carve rework on base, replacing the moving `alpha` tag with a
standalone per-surface `package-lock.json` that resolves it to one frozen version — a change to the exact
mechanism this chain's consumer stage is gated on, not incidental drift. `git diff 844fb9436..HEAD --
app/b2b` is empty, so the merge changed nothing this branch authors, and the b2b package rebuilt green at
the merged head (9 files/35 tests). The only authored change is the ledger entry recording that PR #950
must run `npm run lock:carve` after this producer publishes. This branch's delta against `origin/main`
still matches no `security_paths` pattern.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `4ced8ff376ea213ee2f031921c3a3dc2e9699001`
**Candidate head:** `5edf8db4ad01052165ae5d18d76cf69b7c9e5cda`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:8bb06ecf88e26f242f724ec2fef2ec11de7f00cb993de63d4841ea3b77be0615` `(2 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No issues found. The range repairs the merge-group ejection: `VenueManagerSteps.DraftConcertCreated` was
the last caller still waiting on Playwright's default `WaitUntil = Load` for a client-side route change,
and the concert page holds connections open, so the wait could exhaust its 30s even after the route
changed. It now calls the repository's own `WaitForSpaUrlAsync` (`WaitUntilState.Commit`), the helper
written for this exact failure and already used across the Customer UI suite. The scenario ends on that
step, so the committed URL is the assertion and no readiness is being skipped; the scenarios that continue
past it gate readiness through `MyConcertPage` element waits. Checked against `e2e-scenarios`,
`csharp-style` and `csharp-naming`; the explicit 30s timeout is dropped because the helper takes none and
Playwright's default already is 30s. Release build of `Concertable.B2B.E2ETests.Ui` succeeds. The changed
path matches no `security_paths` pattern.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `43c2bf0acc2f2bf5bfe895d141eba292530b1f3e`
**Candidate head:** `6508faea1e06c2f36f854a09ff674a98831ffcc2`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:b74f5074bf50cb0864a91c9736e602477fb8ff8c6d0877dac290a7a5166d8089` `(10 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No issues found. The range is a fourth conflict-free merge of `origin/main` plus one ledger correction.
The merge was taken because PR #965 refreshed every standalone surface lockfile from
`0.1.0-alpha.0.6314` to `0.1.0-alpha.0.6462` — again the exact mechanism this chain is gated on, so this
branch must carve against the current pins rather than the ones it forked from.
`git diff 43c2bf0ac..HEAD -- app/b2b api/` is empty, so the merge changed nothing this branch authors.
The only authored change corrects the version this ledger cites for that pin. This branch's delta against
`origin/main` still matches no `security_paths` pattern.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `6508faea1e06c2f36f854a09ff674a98831ffcc2`
**Candidate head:** `32a9010ef0118f4beff2cc35a36bdea3dba22485`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:6a77a620dff020f3191b15c4af7b11b414e20afca49c4b57c59bd7ab961345a0` `(2 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No issues found. The range is ledger and work-order text only — it records the merge-queue blocker and its
mechanism. No code changed.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `e040df5f8fdb8abed85caa86ed164cf59cec2414`
**Candidate head:** `e0309bba2c7a2628662baa46c1bf5f46af4d0cdf`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:df282427c003f57c170e6c73ccc9eacd765790acf77fa0fa4b5515479166dc08` `(1 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No issues found. Ledger text only. It corrects an earlier wrong characterisation of the merge-queue
blocker: `e2e-ui-tests` has never executed in CI in the retained window, so the two failures here are its
first CI executions and are deterministic rather than flaky, and the suite was last proven by a local run
around the PR #633 merge. No code changed.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `e0309bba2c7a2628662baa46c1bf5f46af4d0cdf`
**Candidate head:** `c65180db32e9c15d89e28e759b50d3e8079534af`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:4219f95396fd02537d0169cf5d6bc7e86f480062440c2ae0a1949d7dc258ad41` `(11 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`
**Security marker:** required - the delta matches `(^|/)Concertable\\.Payment` in `.agents/merge-gate.json`.

### Findings

No open issues.

The delta resolves the merge-queue blocker recorded above. `PaymentOperationResolver` treated a rejected
transition evaluation as `PaymentOperationError.ProviderUnavailable`, which threw
`PaymentProviderUnavailableException` out of `FinancialOperationHandler`; the receiver abandons with capped
exponential backoff and Azure Service Bus then dead-letters, leaving an authorized escrow uncaptured with no
rejection reaching the consumer. The resolver now resolves from the canonical attempt, so a concurrent
observation that already reached `Authorized` proceeds and anything else is rejected with its own typed
error.

Reviewed for correctness, and for security because the delta touches `Concertable.Payment`:

- **Authorization is unchanged.** `ResolveCurrentAttemptAsync` still rejects a mismatched
  `operation.PayerOwnerKey`, and `ResolveAuthorization` still requires `SessionKind == Authorization`,
  `State == Authorized` and a non-empty `ProviderObjectId`. The change admits no state the previous code
  would have refused; it only stops reporting a false provider outage. The capture itself remains a real
  Stripe call that fails if the intent is not capturable, so persisted state is not the only gate.
- **The new log leaks nothing.** `RejectedSessionTransition` carries the provider object id, provider
  status, rejection reason and the two states - no client secret, payment method, card detail or owner
  identity. `ClientSecret` is never passed to it, consistent with
  `PaymentSessionServiceTests.PersistenceModel_ContainsNoSecretColumns`.
- **`FakeStripeSessionClient.RewindObservation` is not a production path.** It is `internal` and only
  reachable through the fake, which `ExternalServices:UseRealStripe=false` wires for dev and E2E, alongside
  the existing `SetStatus`, `SetDeclined` and `FailOnce` knobs.

Coverage added: the 3DS webhook sequence, the same sequence through the real `CaptureEscrowCommand` handler,
a capture arriving before the consumer acts, and the stale-observation regression, which fails with the
exact production exception without the resolver change. 30 focused integration tests and 552 Payment unit
tests pass.

Not fixed here, recorded as a new HIGH in `api/Concertable.Payment/TECH_DEBT.md`: nothing re-drives a
dead-lettered financial operation, `NextReconcileAt` is written but never read, and
`PaymentSessionReconciliationSource.Sweep` has no implementation, so a genuine provider outage lasting
longer than the delivery budget still strands an authorized escrow.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `c65180db32e9c15d89e28e759b50d3e8079534af`
**Candidate head:** `8ecd0aceee2bc5c394f22a6859ae20e548ac8e6f`
**Candidate branch:** `Refactor/B2bPackageTopologyPhase3-Producer`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:f0821d4525c3218bbcc79e6a1ce23ff4773572768939b56890d42c3a8a15367b` `(1 paths)`
**Candidate bundle:** derived in place from the frozen range
**Work-order path:** `reviews/Refactor-B2bPackageTopologyPhase3-Producer.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No issues found. Ledger text only. It replaces the superseded blocker record with the diagnosed cause and
its fix, lists the six hypotheses ruled out with their evidence, notes that the recorded top suspect PR #959
was not implicated, and carries forward the unfixed recovery gap now held as a HIGH in
`api/Concertable.Payment/TECH_DEBT.md`. No code changed.
