# Concertable

Concertable is a monorepo (a convenience, not the architecture) with a `.NET` microservices backend in `api/` and frontend surfaces in `app/`. The backend services own their runtime; cross-service deps are Contracts-only; standalone AppHosts are canonical. **Read [`api/ARCHITECTURE.md`](./api/ARCHITECTURE.md) before designing anything that crosses a service boundary.** Forgetting this leads to re-monolithing the system.

## Always take the scalable, long-term approach — never the hacky quick fix

**When two solutions present themselves, take the one that is correct for the long term, even when it is harder, larger, or slower to land.** Never reach for the quick hack, the shim, the special-case, the timeout/retry bumped to ride out a flake, or the "just make it work for now" — a workaround that unblocks today becomes the landmine someone trips on later (this very tech-debt backlog is full of exactly those). The proper, scalable fix is the default and the expectation, not a nice-to-have to weigh against effort.

- **Multiple PRs, cross-package cut-overs, publish-first migrations, extra scaffolding — all fine.** Scope is never a reason to pick the worse design. If the right fix needs three PRs or crosses a package boundary, do it in three PRs; say so in one line and proceed. Splitting the *delivery* of the correct solution is encouraged; substituting a *worse* solution to fit one PR is not.
- **A shortcut is only acceptable when it is genuinely, provably the right call** (e.g. deferring live tax-ID verification that overlaps Stripe) — and then it is *logged* in the owning `TECH_DEBT.md` with the reasoning, never left silent.
- **If effort/complexity is pushing you toward the lesser option, surface that as a trade-off for Tommy to decide — do not quietly downgrade the solution.** The bias is always toward the durable, maintainable, architecturally-honest answer.

## Questions come before actions

When Tommy asks a question, answer it directly before taking any action. Discussion of possible work,
numbered options, prompts, branches, or plans is not authorization to execute it. If one message both
asks a question and explicitly requests an action, answer the question first, then perform only the
explicitly requested action.

## Autonomy — act on reversible work, don't ask

Decide and act on reversible work (doc/plan edits, isolated commits, retrying a transient failure), then report — no check-ins. Research: run end-to-end, update the relevant docs, commit in isolation. Pause only when an action is irreversible or contradicts what you find (e.g. unrelated work already staged) — flag it in one line and take the safe path, don't ask permission.

**Never gate a reversible local (working-tree) change behind a "should I?" — just make it.** Editing / writing / refactoring a file, or running a plan's code steps, is the default action, never a question and never a "just report / do nothing" menu; the *only* thing that waits for an explicit instruction is `git commit` / `git push` (full rule: root `~/.Codex/AGENTS.md`).

**Completed docs/meta-only work is the exception to that push gate:** once reviewed, commit, push, and
merge it through `/merge-docs` without waiting for another instruction, keeping agent-loaded guidance current via the no-E2E docs path.

**If requested work depends on a PR that does not exist, create it and do the work; never hand back the same blocked prompt.**

## Per-area guidance

**Doc locality — a guidance/architecture doc lives at the lowest node that fully contains its concern:** single-service → that service's own folder (thin, inheriting root + `api/` upward, never restating — e.g. [`api/Concertable.Payment/AGENTS.md`](./api/Concertable.Payment/AGENTS.md)); cross-service or orchestration → root. Create one only where genuine service-specific content exists.

- **Backend (.NET, `api/`)** — seeding, migrations, DTOs, module rules, C# conventions: [`api/AGENTS.md`](./api/AGENTS.md).
- **Design patterns the codebase commits to** (keyed strategy resolvers, and the anti-patterns they replace — branching on `DealType` in agnostic code, service location, throwaway DTOs): [`api/agents/CODE_PATTERNS.md`](./api/agents/CODE_PATTERNS.md). Read it before adding any rule that varies by a closed key.
- **Web SPA (`app/web/`)** — [`app/web/AGENTS.md`](./app/web/AGENTS.md).
- **Customer cross-platform core (`app/customer/shared`, npm package `@concertable/customer`, exported as `@concertable/customer/shared/*`)** — consumed ONLY by the customer web + mobile apps: [`app/customer/shared/AGENTS.md`](./app/customer/shared/AGENTS.md).

## Git branch — branch first, capitalized type prefix, always

**Before starting any work, create a relevant branch for it if you're not already on one** — never commit to `main` or an unrelated branch.

**Worktree identity gate — before any edit.** State whether the task matches the current branch/PR directly or is branch-local work because it changes code not yet in `main`; verify service ownership, the dirty paths, and other worktrees rather than matching on a shared refactor name. If neither basis holds or anything contradicts it, **STOP and ask**.

**Fetch first, and branch from `origin/main` — never from local `main`.** Local `main` silently
drifts behind, and branching off it builds and tests everything against a stale tree. That is how work
already merged gets reinvented (a hand-rolled `IScoped` test refactor was written here against a
13-commits-stale checkout, while `EventHandlerIntegrationTest` already existed on `main`), and how a
PR later trips the auto-merge currency rule below — by which point the wasted work is already done.
The staleness is invisible locally: the build is green, because it is green *against the old tree*.

```bash
git fetch origin --quiet && git checkout -b <Type>/<Name> origin/main
```

**Reusing an existing branch/worktree?** At session start, `git fetch` + check `git rev-list --count HEAD..origin/main`; sync before working — never build on a stale tip. Don't reflex-merge `origin/main` every prompt — it won't refresh already-loaded docs (only a fresh session does), and mutating a dirty tree mid-task risks conflicts; merge only when behind with a clean tree.

**Don't branch to refactor code from the feature you're already on.** If the code only lives on the current feature branch (not yet in `main`), the refactor is part of that feature — stay on the branch and commit there. A new `Refactor/<Name>` branch is only for code **already merged to `main`**. Branching off an in-flight feature fragments it across two PRs and orphans the original.

Branches are named `<Type>/<Name>` with the type prefix **capitalized**: `Feature/`, `Refactor/`, `Bug/`, `Fix/`, etc. Never create a lowercase variant (`feature/...`). Windows' case-insensitive filesystem cannot hold two casings of the same ref, so a remote with both `feature/x` and `Feature/x` breaks `git fetch`/`git pull` for everyone ("cannot lock ref ... File exists"). Before creating a branch, match the casing of any existing branch of the same name exactly.

**Docs and plans are exempt from branch hygiene.** Non-code markdown — `plans/*.md`, any `TECH_DEBT.md`, scratch notes — is non-breaking and never affects a build or another PR, so just commit it on whatever branch you're already on. Don't branch for it, don't split it into its own commit, and don't worry if `git add -A` sweeps a stray plan/doc into a feature commit — bundling doc-only changes is fine, not worth a force-push to tidy up.

## Before enabling auto-merge — the branch MUST be current with base

**Enabling auto-merge on a branch that's behind `main` is the miss to never repeat.** GitHub either
holds the PR `BLOCKED`/`BEHIND` (branch protection requires it current) so it silently never merges, or
it merges code that was never built against current `main`. **Update first, then enable — always.**
This is a mandatory pre-step to the confirm loop below; `/merge` does it for you, so do it by hand only
when you merged another way. Run it in the branch's **own checkout/worktree**, never the main checkout —
a session sitting in the wrong checkout (e.g. reviewing a worktree PR from `main`) is exactly how the
staleness goes unnoticed.

```bash
git fetch origin --quiet
behind=$(git rev-list --count HEAD..origin/main)
[ "$behind" -gt 0 ] && { echo ">>> $behind commits behind main — update before enabling auto-merge"; \
  git merge origin/main --no-edit && <rebuild affected projects to 0 errors> && git push; }
# only when $behind is 0 AND the rebuild is green → gh pr merge <PR> --auto
```

An `api/**` branch that's behind also risks a stale `<ConcertablePlatformVersion>` pin — the merge of
`origin/main` brings the current pin with it, so updating first keeps that correct too.

## Confirming a PR merge — Bash background until-loop, never `Monitor`

After enabling auto-merge, confirm the outcome with a **Bash `run_in_background` until-loop** that resolves
to exactly ONE of three terminal states and reports it automatically — no reprompt needed. It **never
retries and never toggles**: a failed check is a real failure to surface and debug, not something to poke.

The three outcomes:
1. **Merged** — report `✓ landed as <sha>` and stop.
2. **A check failed** — report `✗ CI failed: <job/check>`, point at the run/log, and **stop. Do not
   retry** (re-running a genuinely-failing e2e just fails again). Hand off to debugging by emitting a
   ready-to-paste `/e2e-ui-debug` | `/e2e-api-debug` **dispatch prompt** — worktree path, branch, PR,
   failing scenario(s), and the failure signature — for a dedicated debug session, rather than driving
   a heavy local E2E run inline. Emit it **as soon as the failure is obviously genuine** (a targeted or
   deterministic failure in the changed area); only a *flake-signature* failure (the whole suite dead
   at startup/auth) waits for a fresh-stack re-run to fail again before you dispatch.
3. **Green but never admitted** — the PR is `CLEAN`, all checks pass, auto-merge is on, yet GitHub never
   adds it to the queue. This is a GitHub auto-merge **re-evaluation glitch** (enable-while-pending, then
   it never looks again — observed live on pr-229), **not** a test failure. Surface it as its own state;
   the remedy is a **one-time** human/agent action — re-assert auto-merge once (`gh pr merge --disable-auto <PR>`
   then `--auto <PR>`) or break-glass admin-merge — never an automated loop.

**Telling #2 from #3 requires inspecting the actual run results, not just PR state** — this is the trap.
After a `merge_group` run FAILS, GitHub ejects the PR back to `OPEN`/`CLEAN`/not-queued, which looks
**identical** to the never-admitted glitch (#3). The failure lives on the `gh-readonly-queue/...pr-<N>-...`
run, not on the PR head's checks, so `gh pr checks <PR>` alone won't show it. The loop must also scan
`merge_group` run conclusions for this PR — a failed one means #2 (debug it), none-ever-dispatched means #3
(nudge it). Conflating them is how a real failure gets mistaken for a stall (and vice versa).

- **Never use the `Monitor` tool** for a single "tell me when it merges" — it's for streaming many events, and its detached poller silently missed merges here (it timed out instead of firing).
- **Never swallow poll errors** (`2>/dev/null || continue`) — a broken `gh` then looks identical to "still waiting". Capture stderr into the state (`2>&1`), echo the state every poll, and **cap** the loop so a persistent failure surfaces instead of hanging forever.

```bash
pr=<PR>; repo=Concertable/concertable; max=90; i=0; cleanpolls=0
while :; do i=$((i+1))
  # Read state + mergeStateStatus into SEPARATE vars — never a joined string. `case "$st"` must compare
  # the bare state ("MERGED"), or it silently never matches "MERGED UNKNOWN" and the loop times out
  # instead of reporting the merge (the "monitored for ages, missed the merge" bug).
  read -r st mss < <(gh pr view "$pr" --json state,mergeStateStatus -q '.state+" "+.mergeStateStatus' 2>&1)
  inq=$(gh api graphql -f query='{repository(owner:"'"${repo%/*}"'",name:"'"${repo#*/}"'"){pullRequest(number:'"$pr"'){mergeQueueEntry{state}}}}' -q '.data.repository.pullRequest.mergeQueueEntry.state // "no"' 2>&1)
  fail=$(gh pr checks "$pr" 2>/dev/null | awk -F'\t' '$2=="fail"{print $1}' | paste -sd, -)
  mgfail=$(gh run list --event merge_group -L 15 --json conclusion,headBranch --jq '.[]|select(.headBranch|contains("pr-'"$pr"'-"))|.conclusion' 2>/dev/null | grep -c failure)
  echo "poll $i: [$st/$mss] queue=[$inq] pr-checks-failing=[${fail:-none}] merge_group-failures=[$mgfail]"
  case "$st" in
    MERGED) echo ">>> #$pr ✓ MERGED"; exit 0;;
    CLOSED) echo ">>> #$pr CLOSED without merging"; exit 0;;
  esac
  if [ -n "$fail" ] || [ "$mgfail" -gt 0 ]; then
    echo ">>> #$pr ✗ CI FAILED (pr:[$fail] merge_group-failures:$mgfail) — inspect the run, do NOT retry"; exit 2; fi
  # green + mergeable + never admitted, sustained past normal latency -> the re-eval glitch (#3)
  if [ "$st" = OPEN ] && [ "$mss" = CLEAN ] && [ "$inq" = no ]; then cleanpolls=$((cleanpolls+1)); else cleanpolls=0; fi
  if [ "$cleanpolls" -ge 6 ]; then
    echo ">>> #$pr ⚠ GREEN but unadmitted ~6min (GitHub re-eval glitch, NOT a failure) — re-assert auto-merge once or break-glass"; exit 3; fi
  [ "$i" -ge "$max" ] && { echo ">>> #$pr still [$st/$mss] after $max polls — surfacing"; exit 1; }
  sleep 60
done
```

## Platform sync is a live gate — a package merge isn't done until its sync PR is green

Any merge that touches `api/**` makes `publish-packages` republish and `platform-sync` open a
`chore/platform-sync-*` PR that bumps every service's `<ConcertablePlatformVersion>` to the new
version (MinVer bumps it on every merge). **Non-breaking → the sync PR auto-merges green in minutes.
Breaking** — a published type's shape/namespace moved and a consumer no longer compiles against the
new pin — **→ the sync PR goes RED, and until it's fixed every service is stranded on a broken
platform pin.** This is the failure that keeps recurring; treat it as a first-class part of merging,
not an afterthought:

- **Whoever merges owns the sync.** After merging an `api/**` change, follow its `chore/platform-sync-*`
  PR to green/merged — or, if it's red, migrate the failing consumer(s) **in that PR** (legal now: the
  new version is on the feed), build `api/Concertable.slnx` to 0 errors, and push. `/merge` step 6
  automates this; do it by hand if you merged another way. **Never leave a red sync PR behind.**
- **Before branching for new feature work, confirm no open red sync PR** — don't build on a mid-break
  platform. This is a **branch-time** check (the cheap checkpoint), *not* a per-prompt one:
  ```bash
  sp=$(gh pr list --state open --json number,headRefName --jq '.[] | select(.headRefName|startswith("chore/platform-sync-")) | .number' | head -1)
  [ -n "$sp" ] && gh pr checks "$sp" | awk -F'\t' '$2=="fail"'   # any output → clear it before starting new work
  ```
- **Automated backstop (no action needed):** `.github/workflows/platform-sync-alert.yml` opens a
  tracking Issue + labels the PR `platform-sync-broken` the moment a sync goes red (and closes the
  Issue when it greens), so a broken sync can't rot unnoticed even when the merge bypassed `/merge`.

## E2E suites — Docker health first, always

This section is **how** to run E2E safely. For a PR, do not duplicate the merge queue's E2E run
locally; the local gate stops at build + unit + integration unless a queue failure needs debugging.
[`plans/AGENTS.md`](./plans/AGENTS.md) carries that local workflow. The merge skill's Step 4 is the
single source of truth for selecting the merge-queue E2E tier.

**Full E2E in the merge queue is the default.** Add `skip-e2e` only when the PR is both small and
demonstrably low-blast-radius, with every one of these true:

- The diff and affected area are small and isolated.
- It touches no package/service boundary, shared infrastructure, build/publish/deployment pipeline,
  CI workflow, or multiple application surfaces.
- It changes no user-facing/runtime flow covered by E2E.
- Unit/integration tests fully cover the affected behaviour.

**Zero intended behaviour change is not sufficient.** Package renames, lockfile/workspace changes,
shared-library moves, broad refactors, and build/publish separation must run full E2E. When in doubt,
do not skip. The labels are the reliable lever: `skip-e2e` drops both E2E suites and `skip-e2e-ui`
drops only the UI suite. Remove stale skip labels when the PR does not qualify; if historical trailers
would opt out a PR that now requires the full tier, add `full-e2e`. Unit tests, integration tests,
build, and carve are never skippable for code/package changes.

A same-named **git trailer** (`Skip-E2E: true` on its own line) works too — parsed structurally by git,
so prose that merely mentions it can't trip the gate (the pr-227 bug) — **but it is fragile in this repo,
so prefer the label.** Git only parses the *last* paragraph of a commit message as trailers, and every
commit here carries a mandated `Co-Authored-By:` trailer, so `Skip-E2E: true` must sit in the **same
contiguous block** as `Co-Authored-By:` — a blank line between them splits the paragraph and git no
longer sees `Skip-E2E`, so the queue silently runs E2E anyway. Unit and integration tests always run
for code/package changes and have no opt-out. The label sidesteps the E2E trailer fragility entirely.
Full tier table in [`.github/workflows/test.yml`](./.github/workflows/test.yml).

Run E2E only through `./e2e.ps1` via the matching skill (`e2e-ui-regress`, `e2e-ui-debug`,
`e2e-api-debug`) — the skill's Step 0 Docker pre-flight is mandatory, every run.

- **`docker ps` answering is NOT proof Docker is healthy.** Docker Desktop can be off, paused, or
  half-started with the engine still answering `docker ps` — even running containers — while
  host→container forwarding of real bytes for NEW containers is dead. The E2E signature of that
  state: every SQL/health connection is accepted then reset (`pre-login handshake` errors), services
  never become ready, and the whole suite dies at fixture startup in a few minutes with **zero
  scenarios executed**.
- **`docker ps`, `docker run hello-world`, and a bare TCP connect are ALL insufficient.** hello-world
  needs no port forwarding, and the host-side `docker-proxy` completes a TCP handshake *locally* even
  when forwarding into the container is dead — so a connect "succeeds" while no data flows (exactly
  the `pre-login handshake` mode). The only valid check is a real **data** round-trip to a fresh
  container: run **`./docker-health.ps1`** (fresh container + published port + HTTP round-trip +
  stability check; exit 1 = unhealthy). `./e2e.ps1` runs it as an automatic gate before booting.
- **A suite that fails at startup is an environment problem until proven otherwise.** STOP after
  the first such run — do not rerun, do not debug application code. Verify Docker with
  `./docker-health.ps1` (and Docker Desktop showing **Running**). Fix, then run once.

## Tech debt (`TECH_DEBT.md`)

Always record tech debt in the `TECH_DEBT.md` belonging to the area that owns the problem. If that
area does not have one, create it there rather than adding the entry to a broader parent file. Once
the debt is addressed, delete the entire entry; do not retain resolved entries as an archive.

## Code comments — default to none; the commit message is the archive

Default to **zero** comments. The diff shows *what* changed; the commit message is where *why* lives (the incident, the root cause, the alternatives). A code comment is the exception, not the habit — **≤2 lines**, and only for a *why* a reader needs *at this line* and can't get from well-named identifiers. Anything longer belongs in the commit message, not the file; big inline explanations rot in place.

A comment is **wrong**, not merely long, if it:
- **restates reasoning already in a `AGENTS.md`/`docs` file** — link it in a phrase, or omit it (two copies drift the day one changes);
- **cites a transient artifact** (a plan filename, "Phase N", a ticket) that will be deleted — the reference is engineered to dangle;
- **narrates the *what*** — well-named code already does that.

The one comment that always earns its place: a **single-line footgun/invariant warning** at the exact site a future edit would break something ("don't put X here — it would Y").

And if a comment needs a paragraph to justify the code below it, that's usually the *code* telling you it's hacky — do the proper fix, or if a quick fix is genuinely right, log it in the nearest `TECH_DEBT.md` and keep the comment short.

## Prompts

Follow [`PROMPTS.md`](./PROMPTS.md) for every continuation, resume, handoff, review, or implementation prompt.

## Plans (`plans/*.md`)

Plans are working docs for unfinished work, **not** an archive — git history is the archive. A finished plan kept "for reference" is just rot that misleads the next reader into thinking the work is still pending.

**Opening a `plans/*.md` to work from obliges you to read [`plans/AGENTS.md`](./plans/AGENTS.md) in the same breath** — phases, verification gates, and when to run E2E live there, and the plan's own prose is not a substitute for them. Reading only the plan is how its rules get skipped.

The convention is **ROADMAP → PLAN → PROGRESS**, folder = roadmap/plan: an epic tracker at `plans/<epic>/<EPIC>_ROADMAP.md` spins off plans at `plans/<epic>/<NAME>_PLAN.md`, each with a same-directory `<NAME>_PROGRESS.md` companion and a worktree/branch named `<Type>/<epic>_<name>` to match. The plan holds the design and outstanding phases; the progress ledger records every project action, result, and state transition plus the current operational truth. Keep both current throughout the work. Legacy plans without a ledger remain valid: reconstruct them from the plan and repository evidence, then create the ledger before recording further progress. Full rules: [`plans/AGENTS.md`](./plans/AGENTS.md) "Companion progress ledger."

- **Cross-plan blockers are two-way handoffs.** The blocked ledger names the owning ledger and exact
  gate; the owning ledger lists the blocked dependent. When the gate opens, the owner updates the
  dependent ledger and surfaces its resume prompt — the waiting plan does not poll or rely on memory.
- **A blocked plan never emits its own resume prompt.** Its ledger and final report name the exact
  blocker, the action that removes it, and the evidence that makes resumption valid. Dispatch the
  resolver or give Tommy the external action; only surface the waiting plan after the gate opens.
- **Keep the plan and its `_PROGRESS.md` companion until the entire lifecycle is terminal — not merely until the final local phase is committed and verified.** They remain the recovery anchor through every required review/fix, PR/check/merge, publication, dependency, and platform-sync gate. When the source PR merges, move that recovery state to a clean `Docs/*_closeout` worktree and delete the feature worktree immediately. Record the final gate outcome there, then delete both artifacts together and land the close-out through `/merge-docs`. If no later delivery or package gate exists, the final phase commit may close them out.
- A plan **superseded** by a newer plan, or describing a design that was **rejected**, is deleted the moment that's decided — don't leave a tombstone.
- A **partially-done** plan stays, but strike/check off the sections that shipped (in the same commit as the work) so what remains is only the outstanding work.

## Throwaway working markdown — in the repo, then deleted

Ad-hoc markdown — investigation prompts, scratch analysis, handoff notes for another tool/agent — goes **in the repo**, never in a temp/scratchpad directory. The scratchpad is invisible to the user and to other tools operating on the repo, so a doc written there is effectively lost. Put it where it can be seen and used.

These are working docs, not an archive (same rule as plans): **delete the file once it's served its purpose** — the handoff happened, the question was answered, the analysis landed in code/commit. Don't let throwaway markdown accumulate as rot.
