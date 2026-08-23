# Polyrepo-ready guidance corpus

> **Next steps live in @plans/docs/POLYREPO_READY_PROGRESS.md → `## Next Steps`.**

Finish the split the polyrepo cut requires, **before** the cut rather than after it. The restructure
divided the corpus by portability — generic rules out to `dotagents`/`react-agents`, this system's roster
to `agent-standards`, and a third bucket it called "the floor", left in-repo. Right axis, wrong number of
destinations: that third bucket is the one §6 deletes.
[`plans/platform/POLYREPO_ROADMAP.md`](../platform/POLYREPO_ROADMAP.md) records the ruling that it does
not: services become independently-developed repos, so `api/` and a shared `plans/` tree are destinations
with no future.

**Sequencing is the point.** Land this before the cut and eight repos inherit a correct corpus on day
one. Land it after and it is eight repos to fix — the copy-and-drift failure this epic exists to kill,
reintroduced at repo scale.

## What is actually wrong, measured

| Problem | Measure |
|---|---|
| ~~Generic plan process sits in a repo with no future~~ — **moved, Phase 1** | `plans/agents/PLAN.md` 233 lines · `PROMPTS.md` 57 · `plans/agents/ROADMAP.md` 34 — **324 lines**, of which 32 carried a Concertable-specific name or command (the 259 first recorded here counted non-blank lines; these are `wc -l` at the time of the move) |
| The route table has no home after the cut | 37 rows in root `.agents/skill-routes.json`. `agent-standards` **vendors the hook** (`vendor-hooks.ps1`, provenance-hashed) but ships **no** table — so the table is per-repo data, and 3 rows (`^api/…`, `^app/…`, `^plans/…`) name paths a service repo does not have. The *convention* those 37 rows follow has no owner anywhere |
| The hub docs are in the deleted root | root `AGENTS.md` (147 lines) and `docs/INDEX.md` — not "they open by describing a monorepo", which is the wording; the problem is every rule in them needs a destination |
| The 28 workflow skills are in the deleted root | **3,285** lines in `.agents/skills/`, every family platform-wide — N1 |

Six sibling process docs — branching, committing, merging, remote validation, docs-and-debt,
failing-tests — already moved to `standards/process/`. Plans moved 78 lines and left 259. There is no
reason for the asymmetry beyond the restructure scoping `PLANS.md` narrowly and nobody re-deriving it,
which is the same failure that kept the `concertable-` prefix alive on an argument already dead when it
was read.

**Rows keyed on architecture port; rows keyed on location don't.** The four layer routes
(`.Application/`, `.Api/`, `.Domain/`, `.Infrastructure/`) mean the same thing in a service repo; the three
area floors name monorepo directories and mean nothing there. That is the distinction the convention has to
encode — not a reason to keep one table alive in two shapes.

## Decisions taken, so no phase waits on a question

1. **Generic process stays in `Concertable/agent-standards`, and its charter is reworded to admit it.**
   That repo opens with "everything that is true of **Concertable specifically**", then holds seven
   generic process docs — its own README concedes "branching and merging are neither stack". Every repo
   the cut produces is a Concertable service, so a Concertable-scoped plugin carrying generic process
   costs nothing. **Rejected: a fourth `process-agents` repo.** It is the only option that would let the
   DevOps there), and three-repo coordination already produced two stale-standards incidents in a single
   session. Four is worse. Revisit only if a work repo actually wants the plan method.
2. **Copying is not the answer, at any tier.** A developer works on two or three services — B2B and
   Payment, or Customer alone — so the standards must be identical across the repos they clone.
   Per-machine plugin install already gives that. Committing `plans/agents/` into eight repos means one
   copy is edited and seven rot.
3. **Out of scope: `POLYREPO_ROADMAP` item 4c.** Where a `launch` plan spanning four services physically
   lives is contentious and gated on that roadmap's §6. How to *write* a plan has no locality question,
   so it does not wait behind it.
4. **Cross-harness standards delivery is an immediate prerequisite, not work deferred to N8.** Deleting
   the in-repo skill copies exposed that Claude Code and Codex did not share an installation mechanism:
   Claude had stale plugin snapshots and Codex had none of the standards plugins. Each standards repo now
   authors the manifest schema its harness requires; one provisioning command installs or refreshes all
   five plugins in both harnesses; and the router verifies every skill named by a repo's route table against
   the active harness only. This is independent of N2's per-repo table generator and was delivered early so
   ordinary code work can load its conventions now. N8 still proves the final carved repo end to end.

## The only two destinations

[`POLYREPO_ROADMAP.md`](../platform/POLYREPO_ROADMAP.md) §6, decided 2026-08-18: *"there is no `api/` node
in a polyrepo, so `api/agents/` and `api/AGENTS.md` are destinations with no future. Everything in them
re-homes to `standards/` (platform-wide, inherited by every service repo) or to the owning service's repo."*

**Every phase below answers one question per artifact: platform-wide → `agent-standards`, or single-service
→ that service's repo. There is no third answer.** "It names this repo's scripts", "it is the in-repo
floor", "it is genuinely per-repo" are not destinations — the root is being deleted, so anything left there
is deleted with it or replicated eight times, which is the failure this epic exists to end. The test is
**common across services**, never *does it mention Concertable*: `/merge` names the queue and
platform-sync and is still platform-wide, because all eight repos merge and all eight own a sync.

### Phase 1 — move the generic plan process out ✅ **done**

Target `Concertable/agent-standards` (PR [#5](https://github.com/Concertable/agent-standards/pull/5)):

- [x] `plans/agents/PLAN.md` (233) → merged into `standards/process/PLANS.md`, which went 78 → 248. It
  already owned the lifecycle; this added the *method* — phases, verification gates, the four-line
  blocker schema, the ledger format.
- [x] `plans/agents/ROADMAP.md` (34) → same file, as the roadmap tier.
- [x] `PROMPTS.md` (57) → `standards/process/HANDOFF.md` (57), a new node. It is the continuation
  pointer's exact shape and nothing else defines it, so it earns its own doc rather than a section.
- [x] `plans` skill description widened; `handoff` router added; the README charter reworded to say which
  domain is a roster and which is method, and why a fourth process repo was rejected.

**What stayed in-repo — and owes the two-destination test.** `plans/AGENTS.md` (71 → 75 lines) kept the
`plans/<epic>/` layout, `plan_graph.py` and `plan_handoff_stop.py`, `worktrees.ps1 close -PlanManaged`, the
debug-skill names by tier, `/resume-plan` and `/continue-roadmap`, `initial-migrations.ps1`, the
merge-queue E2E tier, and the carve's instance of the breaking-contract rule. Calling that "genuinely
per-repo" was the mistake above in miniature: **every one of those is platform-wide.** All eight repos run
a plan graph, close plan-managed worktrees, debug by tier, and own a merge queue; only the *values* differ
(this service's script path, this service's suite names). So the content belongs in `standards/` with the
values named by each repo's own thin `AGENTS.md` — not as 75 lines copied eight times, which is what the
ledger's "generated or hand-kept?" open question was really asking. N1 and N7 settle it with the rest.

**Gate — met.** `plan_graph.py` 0 errors, `docs_reachability.py` 0 errors (26 pre-existing warnings, all
in `plans/`); the `plans` route still fires on a `plans/**/*.md` write; hook tests 14/14 here and 161/161
in `agent-standards`; no guidance doc links a moved file (the only surviving mentions are historical
records in spent ledgers and review files).

## The remaining work is one node at a time

Phase 1 proved the shape: measure a node, move the platform-wide content to `agent-standards`, leave the
per-repo values behind, land producer then consumer. Everything left is that same operation applied to the
next node, and the nodes are known and measured (`origin/main` at `1d15a7920`):

| # | Node | Size | Destination | Leaves behind |
|---|---|---|---|---|
| ~~N1~~ **done** | `.agents/skills/` — all 28 skills moved (6 of 6 families) | 3,285 lines | platform-wide, across `agent-process` + `dotnet` plugins | per-repo values: script paths, suite names |
| ~~N2~~ **done** | `.agents/skill-routes.json` — 37 rows | 37 rows | the *convention* (`SKILL_ROUTES.md`, skill `skill-routes`) + a generator (`gen_skill_routes.py`) | the table itself: per-repo data; `_comment` repointed |
| ~~N3~~ **done** | `api/AGENTS.md` + `api/CLAUDE.md` deleted | 78 | agent-standards (shared-is-the-intersection → `SERVICE_BOUNDARIES.md`; every other section was already skill-owned) | nothing |
| ~~N4~~ **done** | `api/ARCHITECTURE.md` + `api/docs/MICROSERVICES_ARCHITECTURE.md` deleted, refs repointed | 62 + 525 | agent-standards (cross-service by definition) | nothing |
| ~~N5~~ **done** | root `AGENTS.md` thinned 149 → 23 (#745) | 149 | rules → plugins (floor hook + routes) | a thin per-repo `AGENTS.md` (product/monorepo line, per-area pointers, values) — deleted only at the cut, not here |
| ~~N6~~ **done** | `docs/` — product narrative → `Concertable/docs` (#750); `INDEX`/root repointed; `REMOTE_VALIDATION.md` kept | 554 | mixed — see N6 | `INDEX.md` + `REMOTE_VALIDATION.md` (this-repo nav/validation) + the thin root — all deleted at the cut |
| N7a **in progress** | `plans/AGENTS.md` thinned 68 → 31 | 68 | rules already homed (Phase 1 + N1); pointers only | a thin per-repo `plans/AGENTS.md` (layout, hook/script paths, suite names) |
| N7b | relocating the plan *documents* | tree | — | gated on roadmap §4c |
| N8 | *(gate)* prove one carved service standalone | — | — | — |

**One node per slice, in this order.** ~~N1 first~~ and ~~N2~~ are **done**: every hub below N1 points *at*
skills, so re-homing a hub before its targets have addresses writes pointers to nowhere; N2 was independent
and ran alongside. ~~N3~~ **done**; ~~N4~~ **done**; N5–N6 + N7a follow. N7b waits on an external decision. N8 is last and is the only evidence.

### N1 — the 28 workflow skills (3,285 lines)

Under §6 each skill is platform-wide or single-service. Applying that test rather than "does it name
Concertable" (which is what produced a keep-bucket in an earlier draft of this plan):

| Family | Lines | Why it is common |
|---|---|---|
| ~~`review` · `docs-review` · `big-review` · `incremental-review` · `address-review` · `big-review-all`~~ **done** | 813 | Every repo reviews before merge. Landed as seven `standards/process/review/` docs — `reviews/AGENTS.md` joined the family, since the review-file lifecycle is the same rule in every repo |
| ~~`merge` · `merge-docs` · `pr-preflight` · `create-gh-pr`~~ **done** | 634 | Every repo has a queue, a docs bypass, a preflight, PRs. Landed as four `standards/process/merge/` docs. `create-gh-pr` became `open-pr`: a plugin installs per machine, and that name already belonged to a contradictory work-repo procedure |
| ~~`e2e-ui-debug` · `e2e-api-debug` · `e2e-ui-regress` · `e2e-debug` · `integration-debug` · `reset-test-explorer`~~ **done** | 1,022 | Every repo debugs a red suite by tier. Landed as six `standards/process/testing/` docs with every name kept. `docker-health.ps1` vendored through a new second tier of `vendor-hooks.ps1`; `e2e.ps1` and `integration.ps1` stayed, their invocation grammar stated and their own usage listings used as the discovery mechanism |
| ~~`commit` · `commit-all` · `push` · `pull` · `sync` · `worktree`~~ **done** | 429 | Every repo commits, pushes, pulls and isolates a branch. Landed as six `standards/process/git/` docs. **"Zero lines name this repo" was wrong** — five skills held none, but `worktree` named Concertable in its own description and carried `scripts/worktrees.ps1`, the platform-sync gate and two sibling skill names; the script turned out repo-invariant and was vendored, so the doc kept the citation as a constant. `sync` → `sync-checkout` and `worktree` → `open-worktree` on the `create-gh-pr` precedent |
| ~~`resume-plan` · `continue-roadmap` · `update-roadmap` · `techdebt`~~ **done** (+ `auto-memory` deferred) | 203 | The executable counterparts of the `plans` skill Phase 1 already moved. Landed as four `standards/process/` docs — `plan/RESUME.md`, `plan/CONTINUE_ROADMAP.md`, `plan/UPDATE_ROADMAP.md`, `TECHDEBT.md` — plus a fifth, `plan/CHECKPOINT.md`, routed by a **new** `plan-checkpoint` skill: the 138-line checkpoint procedure (was `resume-plan/references/plan-progress-checkpoint.md`) with the progress-ledger template folded in, since the generator routes every doc. `auto-memory` stays in-repo for now; the cross-harness delivery removes its former Codex-plugin blocker, so criterion 1 must still give that Codex-only utility a durable home before close-out. |
| ~~`package-cutover`~~ **done** | 184 | Published-contract cut-over is the carve's own mechanic, identical in every repo consuming the feed. Landed as `standards/dotnet/PACKAGE_CUTOVER.md` in the **dotnet** plugin (not agent-process) — a .NET/NuGet/EF mechanic irrelevant to React repos, and the runnable counterpart to `dotnet:packages`. Cross-references re-pointed to `PACKAGES.md`, `process/PLANS.md`, `data/MIGRATIONS.md`, `process/plan/CHECKPOINT.md`; name kept, no route row (invoked by name only) |

**Single-service: none.** That is the finding. What a carved repo keeps is *values* — its `scripts/e2e.ps1`,
its suite names, its hook and migration paths — named in a thin `AGENTS.md` on the `Concertable.Payment`
model. The same applies to `plans/AGENTS.md` (N7).

One family per slice, ordered by cost of duplication: ~~review~~ → ~~merge/PR~~ → ~~test-debug~~ → ~~git~~ →
~~plan-workflow~~ → ~~`package-cutover`~~. **All six N1 families are moved.**

**Gate per family:** a simulated carved tree loses no rule, the router resolves the moved skill from a
fresh install, and what stays at root is only values this repo owns.

**The review, merge/PR and test-debug families all answered "how do you parameterise a workflow?" — you don't.** Neither
leaves a values file behind. Every repo-specific input is resolved mechanically at run time: the review
family reads that repo's route table through `skill_router.py --skills-for`, every `AGENTS.md` in a touched
directory, and whichever architecture doc the root `AGENTS.md` names; the merge family reads the slug from
`gh repo view`, the check set from `gh pr checks`, the queue config from the rulesets API, and "does this
merge publish?" from the repo's own publish path filter — which is also where its one genuinely
monorepo-shaped value (`api/**`) got re-anchored. Where a value is identical in every repo — the tier
labels, the vendored hook paths, `scripts/worktrees.ps1` — **state it rather than invent a parameter with
one value.** The remaining families try that shape first and reach for a named value only where a *path a
script lives at* genuinely cannot be discovered — which for test-debug is settled below rather than open.

**Test-debug's script question, settled and then shipped: sort each script by whether its *body* is
repo-invariant, not by whether its path is.** The family names three repository scripts, not the two this plan first counted —
`scripts/e2e.ps1` (35 citations), `scripts/integration.ps1` (8, the `integration-debug` entrypoint),
`scripts/docker-health.ps1` (4):

- `docker-health.ps1` (115 lines) is repo-invariant in full: no suite name, no project path, no `api/`
  anywhere, and its one Concertable token is a probe container's name. **Vendor it** beside the hooks, and
  its path becomes a constant every debug doc may name outright — the `merge_review_gate.py` treatment.
  `vendor-hooks.ps1` filters `*.py` into `.agents/hooks` today, so this adds a second source→target tier
  under the same provenance manifest.
- `e2e.ps1` (332) and `integration.ps1` (161) are lists of *this* monorepo's suite projects across four
  services; neither body ports. **What ports is the invocation grammar** —
  `./scripts/e2e.ps1 <ui|api> <run|regress|trace>` and `./scripts/integration.ps1` — stated literally on the
  `scripts/worktrees.ps1` precedent, with each repo owning the body behind it.
- The per-service subcommands (`ui b2b`, `api customer`) are the only genuinely monorepo-shaped part, and in
  a carved repo they **cease to exist** rather than needing a value: one service means `ui run` *is*
  `ui b2b`. The same dissolution as the merge family's `api/**`.

**No values file, a fourth time.** The standard states a contract each repo's entrypoint must satisfy; it
does not read a variable.

**A skill's name is machine-scoped, not corpus-scoped.** The merge family found `create-gh-pr` already taken
on the same machine by a contradictory work-repo procedure reached through junctions, and a plugin installs
per machine. Check a new name against every repo on the machine, not just the three standards repos and the
harness built-ins — and expect it again for `worktree`, which family 4 shares with both `dotagents` and the
work repos. **Family 4 confirmed it, twice over.** `sync` collides with a user-global skill doing a
narrower job under the same name, and `worktree` collides three ways at once — user-global, `dotagents`,
and the work repos. It is not a theoretical hazard: authoring the family, the user-global `worktree` fired
instead of this repo's and junctioned sixteen untracked skill directories into a fresh checkout, every one
a stale copy of a skill that now ships from the plugin. `sync-checkout` and `open-worktree` resolve it, and
`git/WORKTREE.md` states the junction hazard as a rule.

### N2 — the route table's convention (37 rows) ✅ **done**

**Shipped** (producer agent-standards #12, consumer this repo). The convention landed as
`standards/process/SKILL_ROUTES.md` (skill `skill-routes`), and the "template or generator" question was
decided — **generator**: `.agents/gen_skill_routes.py`, a carve-time tool (the routing-table analog of
`vendor-hooks.ps1`) that carries the canonical rows once and emits a repo's table for its `--kind`,
re-anchoring the one `.cs` area floor and dropping the other stack's rows. Its gate test replays a simulated
carved dotnet-service tree through the real matcher; the strongest check is that `--kind monorepo` reproduces
this repo's live 37-row table exactly. The consumer only repoints the table's `_comment` at the convention.

**Two corrections to the framing below.** (1) `^plans/` is **not** an area floor that "ceases to exist" — it,
`^reviews/` and `^.agents/skills/` name directories every repo has, so they port verbatim; only `^api/` and
`^app/` are location floors, and only the `.cs` one re-anchors for a .NET service (the react floor and rows
are dropped). (2) The react rows carry `app/` *mid-pattern*, not just as a prefix, so a carved frontend
repo's table is genuinely blocked on the frontend carve seam (§6/§4c) — the generator refuses `--kind
react-app` rather than emit wrong rows, an input added to N8's dependencies.

The mechanism already splits the way §6 requires: `agent-standards` vendors `skill_router.py` through
`vendor-hooks.ps1` (provenance-hashed) and ships **no** table, so the hook is platform-wide procedure and
the table is per-repo data. The three area-floor rows (`^api/`, `^app/`, `^plans/`) are therefore values
that cease to exist with the root — nothing to re-anchor.

What has no owner is one tier up: **the convention those 37 rows follow.** That every source file is gated
by an area floor plus a layer route, that every matching row fires rather than the first, that a row keyed
on location cannot port while one keyed on architecture can, what a row's `note` is for — all of it lives
today inside the table's own notes and `docs/INDEX.md`, both in the deleted root. Eight repos would
hand-write eight tables from no stated rule.

- Publish the convention from `agent-standards` beside the vendored hook, plus a template or generator that
  emits a repo's table from its own layout, so a carved table is derived rather than copied.
- Keep the layer rows as the portable core; each repo's floors name its own top-level directories.

**Gate:** generate the table for a simulated carved tree from the published convention, replay every
tracked path in that tree, require 100% coverage and no row naming a path outside the repo.

### N3 — `api/AGENTS.md` + `api/CLAUDE.md` (78 lines) ✅ **done**

§6 names this node explicitly: *"there is no `api/` node in a polyrepo."* It was already pointers-only with
no `@`-imports, so this was a re-home, not a rewrite.

**The finding: four of the five floor sections were already skill-owned.** Microservices/roster →
`microservice-boundaries` (`SERVICE_BOUNDARIES.md`), seeding → `seeding` (`SEEDING.md`), migrations →
`migrations` (`MIGRATIONS.md`), source-generated logging → the generic `logging` skill in `dotagents`. Only
**"Shared code is the intersection, never the union"** had no canonical home — it was referenced by
`MULTITENANCY.md` and `HTTP_CLIENTS.md` but stated only in `api/AGENTS.md`, and no route row fired on the
shared tier, so its "silent and expensive" trigger lived only in the always-loaded floor.

- **Producer (agent-standards #15):** the Concertable-specific statement of the rule lands in
  `SERVICE_BOUNDARIES.md` (the `microservice-boundaries` roster doc); the generic principle already lived in
  the paired `dotnet-standards` `SERVICE_BOUNDARIES.md`. A CANONICAL route row fires
  `microservice-boundaries` on `Concertable\.(Kernel|Contracts)/.*\.cs$` — the universal shared tier only,
  not a service's own `*.Contracts` — so the rule keeps its trigger at the violation site. Carve-clean.
- **Consumer (this repo):** the same route row added to `.agents/skill-routes.json` (37 → 38);
  `api/AGENTS.md` + `api/CLAUDE.md` deleted; every inbound guidance-doc link repointed (root `AGENTS.md`,
  `docs/INDEX.md`, `api/ARCHITECTURE.md`, the four service `AGENTS.md` inheritance lines, Deal/Search/Payment
  `ARCHITECTURE.md`, the roadmap north star). The backend floor is thereafter the route table over the
  `dotnet` plugin.

### N4 — `api/ARCHITECTURE.md` (62) + `api/docs/MICROSERVICES_ARCHITECTURE.md` (525) ✅ **done**

Cross-service by definition — the carve, the publish→sync loop, Contracts-only dependencies. Platform-wide
in full, and the largest single document left. It is also what every service repo needs on day one to know
what it may depend on, so it cannot be the last node moved.

- **Producer (agent-standards #18 + #19):** the microservices architecture record homed platform-wide, plus
  host-composition validation; #19 repointed the AppHost route note off the deleted `api/ARCHITECTURE.md`.
  Both MERGED.
- **Consumer (this repo):** split by branch type — `Docs/docs_polyrepo-ready-n4-architecture` (#715) deleted
  both docs and repointed every guidance-doc link (landed through the merge queue, `skip-e2e`); the non-doc
  citations (five service `Directory.Build.props`/`.targets`, five `*.Hosting.csproj`, `claude-review.yml`)
  repointed to the `packages` skill on `Chore/polyrepo-n4-arch-ref-repoint` (#713). Both MERGED; publishes
  succeeded and the final `chore/platform-sync-0.1.0-alpha.0.1128` sync (#724) merged green, non-breaking.

### N5 — root `AGENTS.md` (149 lines)

Split by the same test. The monorepo-only lines (that this *is* a monorepo, where `api/` and `app/` sit) do
not port and die with the root; everything else — the scalable-fix rule, the autonomy rules, the merge and
platform-sync invariants, doc locality, the review gates — is platform-wide. Rewording its opening sentence
so the monorepo reads as "current packaging" is the cosmetic tier and is explicitly **not** the work.

**Producer — DELIVERED (agent-standards #25 + #27, both MERGED into `main` at `13fcef1c0`).** The three
behavioral sections are *always-loaded* rules with no route path to fire on, so re-homing them to a doc a
route can name would silently drop them in a carved repo. #25 instead delivers them as
`standards/process/FLOOR.md`, injected at every session start by a new `session_floor.py` SessionStart hook
and owned by a name-invokable `floor` skill; §7's ready-for-review CI-workflow invariant homed in `MERGING.md`
in the same PR. #27 hardens `skill_router` to fail **CLOSED** — an un-routed write is blocked when NOT ONE
routed skill resolves (the plugin didn't load) — making criterion-2's "a convention is never missing" an
enforced gate. Every other deleted section already had its destination from an earlier slice: the merge
invariants + platform-sync → `MERGING.md`, git-branch / worktree-identity / durable-guidance → `BRANCHING.md`,
worktree-cleanup → `git/WORKTREE.md`, validation/E2E → `REMOTE_VALIDATION.md`, plans → `PLANS.md`,
one-owning-doc / doc-locality / reachability → `DOCS_AND_DEBT.md`.

**Consumer — thin root `AGENTS.md` to per-repo values/pointers only.** KEEP: the product line; the monorepo
clause (rewording it is the cosmetic tier, **not** N5's work); the skill roster + per-area bullets thinned to
this repo's own surfaces; pointers to `docs/INDEX.md`, `plans/AGENTS.md`, `docs/REMOTE_VALIDATION.md`; per-repo
script/suite values; a one-line worktree-identity-gate pointer. DELETE everything homed above.

### N6 — `docs/` (554 lines across five files)

Three different answers in one folder, so it is one node with three outcomes:

- `INDEX.md` (188) — the topic→owner map. The process rows were repointed to their owning skills in N5
  (the CON1 fix); N6 repoints the Product rows to the new central docs repo. It stays this repo's
  topic→owner index; it is **not** folded away.
- `REMOTE_VALIDATION.md` (27) — **kept as this repo's standalone validation doc** (deviation from the
  original "folds into the per-repo floor"). Folding it into root `AGENTS.md` would grow the root we are
  emptying; a per-repo validation doc is the natural home and is what a carved service repo keeps. Root
  already points at it.
- `OVERVIEW.md` (55), `USP.md` (203), `DEEP_RESEARCH_PROMPT_GUIDE.md` (81) — product narrative, **RESOLVED**:
  Tommy chose a dedicated central docs repo. Relocated to **[`Concertable/docs`](https://github.com/Concertable/docs)**
  (private), the standard polyrepo pattern for cross-cutting product/system narrative that belongs to no
  single service. Deleted from this repo; root `AGENTS.md` line 1 and `INDEX.md`'s Product rows repointed at
  the new repo; the moved docs' repo-relative links rewritten as absolute GitHub URLs.

**Root `AGENTS.md` still cannot be fully deleted here.** Even with the product docs gone, `INDEX.md` and
`REMOTE_VALIDATION.md` (this repo's own navigation + validation docs) still need a root anchor to stay
reachable, and they are genuinely this-repo, not standard or service-specific. Literal "nothing at root"
is therefore a **cut-time** step (when the monorepo dissolves and each service repo takes its own
`INDEX`/validation), which is the separate `POLYREPO_ROADMAP` — out of scope here. N6 leaves root at its
minimum: a thin anchor of per-repo pointers.

### N7 — the `plans/` tree and `plans/AGENTS.md` (75 lines)

Two parts on different gates, so it splits:

- **N7a — `plans/AGENTS.md`'s content (not gated). In progress — consumer-only.** Every rule in it is
  platform-wide with per-repo values, so its re-home runs with N3–N6, **not** behind §4c. A rule-by-rule
  mapping found **every** platform rule already homed by Phase 1 + N1, so nothing needs to move out: the file
  thins (68 → 31) to this repo's values (layout, hook/script paths, suite names) + pointers to the owning
  skills — the answer to the old "generate it eight times or hand-keep it?" question is *neither, the content
  leaves*. Two non-blocking `PLANS.md` follow-ups fall out and are deferred to the epic close-out (the
  N5-logged read-before-acting obligation; promoting the bare-stem naming from an assumed category to a stated
  rule) — not opened as a third concurrent PLANS.md PR while agent-standards #20 is in flight.
- **N7b — relocating the plan *documents* themselves.** Gated on roadmap §4c (where a cross-service plan
  physically lives), which is gated on §6's remaining sub-decision. Nothing in N7a waits on it.

### N8 — prove one carved service standalone, or none of this is done

Take the smallest independently-carvable service and check its guidance alone: every route fires, every
skill resolves, no doc links a path outside the service, no doc asserts a monorepo. Payment is the
candidate — an adapter service with the fewest inbound dependencies.

**This is the only node that produces evidence rather than edits**, and it depends on N1–N6: a carved
Payment repo with no review, merge or debug skill cannot prove anything.

## Acceptance criteria — polyrepo-ready, and usable from both harnesses

Done when both hold, **verified rather than asserted**:

1. **Nothing agent-based is left in root.** Every node N1–N7a is re-homed to `agent-standards`, `dotagents`,
   or `react-agents`; what remains in this repo is only per-repo *values* named by a thin `AGENTS.md`.
   `api/AGENTS.md`, `api/CLAUDE.md`, root `AGENTS.md`/`CLAUDE.md`, and `plans/AGENTS.md`'s rules are gone
   from the tree. (N7b — the plan-document tree — and the cut itself are separate, gated on roadmap §4c/§6.)
2. **Every moved rule still resolves from Codex *and* Claude Code, no less accessibly than before.**
   Accessibility is lazy by design: opening a file fires the write-time router over
   `.agents/skill-routes.json`, which names the matching skill before the first write. A convention is never
   allowed to be *missing*: the route remains blocked on every attempt until the active harness can resolve
   every owning skill. The per-node gate is therefore three mechanical facts:
   - **Routing fires:** `python .agents/hooks/skill_router.py --skills-for <path>` returns the expected
     skill(s) for a representative path of each moved family — e.g. a `*.UnitTests/*.cs` still resolves
     `unit-testing`, a repository still resolves `persistence`. The always-loaded floor (root `AGENTS.md`,
     the `api/AGENTS.md` pointers, the former `CODE_CONVENTIONS`/`RESULT_PATTERN` content now in
     `dotnet-standards`) is covered by its own route rows.
   - **Both active installations are complete:**
     `scripts/provision-agent-standards.ps1 -VerifyOnly -Repository <repo>` succeeds for Claude and Codex,
     proving every unique skill named by that repo's table resolves from that harness's installed plugins.
   - **Both plugin formats validate:** the Claude marketplace/plugin manifests and the Codex
     marketplace/plugin manifests pass their own harness validators; one generated schema is not passed to
     two incompatible loaders.

   A rule that stops resolving, resolves only through the other harness's cache, or is absent from either
   active installation fails the node.

N8 demonstrates criterion 1 on a carved service and repeats criterion 2 against that service's generated
table. Live Claude and Codex session smokes remain final evidence, but plugin packaging, provisioning, and
fail-closed routing are already delivered prerequisites rather than work left until N8.

## Explicitly not in scope

- Moving plan *documents* (4c above).
- The polyrepo cut itself, its seam decision, or any repo creation.
- `docs/analyzer-pushdown`, which is independent of this.
- ~~The workflow skills in `.agents/skills/`~~ — silently out of scope until N1 named them, and first
  classified with a "genuinely local, keep" bucket that §6 does not allow. Phase 1 measured three documents
  and moved three documents; nothing in it examined the skill roster.
