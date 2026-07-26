# Concertable

Concertable is a monorepo (a convenience, not the architecture) with a `.NET` microservices backend in `api/` and frontend surfaces in `app/`. The backend services own their runtime; cross-service deps are Contracts-only; standalone AppHosts are canonical. **Read [`api/ARCHITECTURE.md`](./api/ARCHITECTURE.md) before designing anything that crosses a service boundary.** Forgetting this leads to re-monolithing the system.

## Autonomy — act on reversible work, don't ask

Decide and act on reversible work (doc/plan edits, isolated commits, retrying a transient failure), then report — no check-ins. Research: run end-to-end, update the relevant docs, commit in isolation. Pause only when an action is irreversible or contradicts what you find (e.g. unrelated work already staged) — flag it in one line and take the safe path, don't ask permission.

**Never gate a reversible local (working-tree) change behind a "should I?" — just make it.** Editing / writing / refactoring a file, or running a plan's code steps, is the default action, never a question and never a "just report / do nothing" menu; the *only* thing that waits for an explicit instruction is `git commit` / `git push` (full rule: root `~/.claude/CLAUDE.md`).

## Agent instructions vs human docs — `agents/` vs `docs/`

This repo is read by more than one coding agent (Claude Code, Codex, …). `AGENTS.md` is the file both
read; every directory's `CLAUDE.md` is a one-line `@AGENTS.md` stub kept only so Claude Code's
auto-loader finds it — the real content always lives in `AGENTS.md`, so link to that, never to a
`CLAUDE.md` path. Per top-level area, `agents/` (e.g. `api/agents/`, `app/agents/`) holds the
*normative* rules any agent must obey (naming, patterns, seeding, module boundaries); `docs/` holds
*explanatory* material for a human reader (product overview, positioning, architecture rationale,
runbooks) — never conventions an agent is expected to follow.

## Per-area guidance

- **Backend (.NET, `api/`)** — seeding, migrations, DTOs, module rules, C# conventions: [`api/AGENTS.md`](./api/AGENTS.md).
- **Design patterns the codebase commits to** (keyed strategy resolvers, and the anti-patterns they replace — branching on `DealType` in agnostic code, service location, throwaway DTOs): [`api/agents/CODE_PATTERNS.md`](./api/agents/CODE_PATTERNS.md). Read it before adding any rule that varies by a closed key.
- **Web SPA (`app/web/`)** — [`app/web/AGENTS.md`](./app/web/AGENTS.md).
- **Customer cross-platform core (`app/customer/shared`, npm `@customer/shared`)** — consumed ONLY by the customer web + mobile apps: [`app/customer/shared/AGENTS.md`](./app/customer/shared/AGENTS.md).

## Git branch — branch first, capitalized type prefix, always

**Before starting any work, create a relevant branch for it if you're not already on one** — never commit to `master` or an unrelated branch.

**Fetch first, and branch from `origin/master` — never from local `master`.** Local `master` silently
drifts behind, and branching off it builds and tests everything against a stale tree. That is how work
already merged gets reinvented (a hand-rolled `IScoped` test refactor was written here against a
13-commits-stale checkout, while `EventHandlerIntegrationTest` already existed on `master`), and how a
PR later trips the auto-merge currency rule below — by which point the wasted work is already done.
The staleness is invisible locally: the build is green, because it is green *against the old tree*.

```bash
git fetch origin --quiet && git checkout -b <Type>/<Name> origin/master
```

**Don't branch to refactor code from the feature you're already on.** If the code only lives on the current feature branch (not yet in `master`), the refactor is part of that feature — stay on the branch and commit there. A new `Refactor/<Name>` branch is only for code **already merged to `master`**. Branching off an in-flight feature fragments it across two PRs and orphans the original.

Branches are named `<Type>/<Name>` with the type prefix **capitalized**: `Feature/`, `Refactor/`, `Bug/`, `Fix/`, etc. Never create a lowercase variant (`feature/...`). Windows' case-insensitive filesystem cannot hold two casings of the same ref, so a remote with both `feature/x` and `Feature/x` breaks `git fetch`/`git pull` for everyone ("cannot lock ref ... File exists"). Before creating a branch, match the casing of any existing branch of the same name exactly.

**Docs and plans are exempt from branch hygiene.** Non-code markdown — `plans/*.md`, any `TECH_DEBT.md`, scratch notes — is non-breaking and never affects a build or another PR, so just commit it on whatever branch you're already on. Don't branch for it, don't split it into its own commit, and don't worry if `git add -A` sweeps a stray plan/doc into a feature commit — bundling doc-only changes is fine, not worth a force-push to tidy up.

## Before enabling auto-merge — the branch MUST be current with base

**Enabling auto-merge on a branch that's behind `master` is the miss to never repeat.** GitHub either
holds the PR `BLOCKED`/`BEHIND` (branch protection requires it current) so it silently never merges, or
it merges code that was never built against current `master`. **Update first, then enable — always.**
This is a mandatory pre-step to the confirm loop below; `/merge` does it for you, so do it by hand only
when you merged another way. Run it in the branch's **own checkout/worktree**, never the main checkout —
a session sitting in the wrong checkout (e.g. reviewing a worktree PR from `main`) is exactly how the
staleness goes unnoticed.

```bash
git fetch origin --quiet
behind=$(git rev-list --count HEAD..origin/master)
[ "$behind" -gt 0 ] && { echo ">>> $behind commits behind master — update before enabling auto-merge"; \
  git merge origin/master --no-edit && <rebuild affected projects to 0 errors> && git push; }
# only when $behind is 0 AND the rebuild is green → gh pr merge <PR> --auto
```

An `api/**` branch that's behind also risks a stale `<ConcertablePlatformVersion>` pin — the merge of
`origin/master` brings the current pin with it, so updating first keeps that correct too.

## Confirming a PR merge — Bash background until-loop, never `Monitor`

After enabling auto-merge (or when a merge lands async via the merge queue), confirm it with a **Bash `run_in_background` until-loop** that exits the instant `gh pr view <PR> --json state -q .state` is `MERGED` or `CLOSED` — one immediate completion notification.

**Also catch the silent stall — not just the merge.** A `CLEAN`, auto-merge-ON PR that GitHub never admits to the queue (`mergeQueueEntry == null`) sits `OPEN` forever: never merges, never errors, queue idle. Cause: auto-merge was enabled while a required check was still pending, then that check resolved to `skipped` (the non-code classifier skips e2e/unit/integration on the PR) and GitHub failed to re-evaluate admission. **This once burned an hour.** The loop must detect it and self-heal: `OPEN` + not-in-queue for a few polls ⇒ **toggle auto-merge** (`gh pr merge --disable-auto <PR>` then `--auto <PR>`) to force admission. (`.github/workflows/auto-merge.yml` now re-asserts on `check_suite: completed`, so it should be rare — the monitor is the backstop.)

- **Never use the `Monitor` tool** for a single "tell me when it merges" — it's for streaming many events, and its detached poller silently missed merges here (it timed out instead of firing).
- **Never swallow poll errors** (`2>/dev/null || continue`) — a broken `gh` then looks identical to "still waiting". Capture stderr into the state (`2>&1`), echo the state every poll, and **cap** the loop so a persistent failure surfaces instead of hanging forever.

```bash
pr=<PR>; repo=$(gh repo view --json nameWithOwner -q .nameWithOwner); max=120; i=0; stuck=0
while :; do i=$((i+1))
  state=$(gh pr view "$pr" --json state -q .state 2>&1)
  inq=$(gh api graphql -f query="{repository(owner:\"${repo%/*}\",name:\"${repo#*/}\"){pullRequest(number:$pr){mergeQueueEntry{state}}}}" -q '.data.repository.pullRequest.mergeQueueEntry!=null' 2>&1)
  echo "poll $i: state=[$state] inQueue=[$inq]"
  case "$state" in MERGED|CLOSED) echo ">>> PR #$pr $state"; exit 0;; esac
  if [ "$state" = OPEN ] && [ "$inq" != true ]; then stuck=$((stuck+1)); else stuck=0; fi
  if [ "$stuck" -ge 3 ]; then
    echo ">>> PR #$pr STUCK (auto-merge on, not admitted to queue) — toggling to force admission"
    gh pr merge --disable-auto "$pr" >/dev/null 2>&1 || true; gh pr merge --auto "$pr"; stuck=0
  fi
  [ "$i" -ge "$max" ] && { echo ">>> PR #$pr still [$state] after $max polls — surfacing"; exit 1; }
  sleep 30
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

This section is **how** to run E2E safely. **Whether** to run it for a given change is a judgment
call — reserved for massive or behaviorally-risky changes, skipped for stage-1/zero-behavior-change
work — governed by [`plans/AGENTS.md`](./plans/AGENTS.md). Don't run the full suites by reflex.

**That same skip-judgment sets the CI merge-queue tier — via a commit token, not just local runs.**
The merge queue runs the full E2E suite on every code change *by default*. When a change is in the
skip category (behaviour-preserving, small/isolated, well-covered by unit + integration), put
**`[skip-e2e]`** in a commit message so the queue skips it too — otherwise it burns ~25-30 min of E2E
that catches nothing. This is the common case for a refactor; **default to `[skip-e2e]` for any
zero-behaviour-change PR** — letting the queue run E2E on it is the reflex to avoid. `[skip-tests]`
drops to the compile floor (build + carve only) for a genuinely trivial/mechanical change; build +
carve are never skippable. Tokens are read from any commit message in the PR range — full tier table
in [`.github/workflows/test.yml`](./.github/workflows/test.yml).

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

Avoid introducing tech debt wherever possible. But when a quick fix is the right call, or you notice or introduce debt the user is aware of, log a line in the `TECH_DEBT.md` nearest the area you touched (there's one per area — use the closest, not the root).

**Reach for the proper fix, not a hacky workaround.** A workaround that needs a paragraph of comment to justify it — a cold-start warm-up, a sleep-to-avoid-a-race, a magic ordering, a defensive re-try papering over a root cause — is the code telling you to refactor, not to explain. Find and fix the actual cause. If a workaround is *genuinely* unavoidable (an upstream bug, a library limitation you can't route around), keep it minimal, and log a line in the nearest `TECH_DEBT.md` naming the root cause and why it couldn't be avoided — the debt entry is the record, not a long inline comment.

**Don't default away a failure.** A `?? fallback`, empty `catch`, `?.` past a null that shouldn't be null, or an ignored `TryParse` that turns an absent/invalid value into a benign one hides the case that should surface — and in a test it downgrades a real failure into a false pass. If a value is genuinely optional, handle it explicitly; if it should always be present, let its absence throw.

## Code comments — default to none; the commit message is the archive

Default to **zero** comments. The diff shows *what* changed; the commit message is where *why* lives (the incident, the root cause, the alternatives). A code comment is the exception, not the habit — **≤2 lines**, and only for a *why* a reader needs *at this line* and can't get from well-named identifiers. Anything longer belongs in the commit message, not the file; big inline explanations rot in place.

A comment is **wrong**, not merely long, if it:
- **restates reasoning already in a `CLAUDE.md`/`docs` file** — link it in a phrase, or omit it (two copies drift the day one changes);
- **cites a transient artifact** (a plan filename, "Phase N", a ticket) that will be deleted — the reference is engineered to dangle;
- **narrates the *what*** — well-named code already does that.

The one comment that always earns its place: a **single-line footgun/invariant warning** at the exact site a future edit would break something ("don't put X here — it would Y").

And if a comment needs a paragraph to justify the code below it, that's usually the *code* telling you it's hacky — do the proper fix, or if a quick fix is genuinely right, log it in the nearest `TECH_DEBT.md` and keep the comment short.

## Plans (`plans/*.md`)

Plans are working docs for unfinished work, **not** an archive — git history is the archive. A finished plan kept "for reference" is just rot that misleads the next reader into thinking the work is still pending.

**Opening a `plans/*.md` to work from obliges you to read [`plans/AGENTS.md`](./plans/AGENTS.md) in the same breath** — phases, verification gates, when to run E2E, and how to shape the handoff all live there, and the plan's own prose is not a substitute for them. Reading only the plan is how its rules get skipped.

- **When you land the commit that completes a plan's work, `git rm` the plan file in that same commit.** Completion = work committed AND its verification passed (build + the affected unit/integration tests always; E2E only when the change is massive/risky per `plans/AGENTS.md`). Deletion belongs to that commit — never defer it to a later cleanup pass.
- A plan **superseded** by a newer plan, or describing a design that was **rejected**, is deleted the moment that's decided — don't leave a tombstone.
- A **partially-done** plan stays, but strike/check off the sections that shipped (in the same commit as the work) so what remains is only the outstanding work.
- **A completed + verified phase is a HARD STOP.** Hand off the resume prompt and END THE TURN. Do **not** start the next phase in the same session unless the user explicitly names it *and* says to do it now — a vague "continue"/"why stop?"/"yeah" means re-show the handoff, not start coding. Never append "want me to continue?" or a continue-vs-review fork. Full rule: [`plans/AGENTS.md`](./plans/AGENTS.md) "Before a clear."

## Throwaway working markdown — in the repo, then deleted

Ad-hoc markdown — investigation prompts, scratch analysis, handoff notes for another tool/agent — goes **in the repo**, never in a temp/scratchpad directory. The scratchpad is invisible to the user and to other tools operating on the repo, so a doc written there is effectively lost. Put it where it can be seen and used.

These are working docs, not an archive (same rule as plans): **delete the file once it's served its purpose** — the handoff happened, the question was answered, the analysis landed in code/commit. Don't let throwaway markdown accumulate as rot.
