# Guidance-docs roadmap

> **Roadmap** for settling the agent/developer guidance corpus — root `AGENTS.md`, `api/agents/*`,
> `app/agents/*`, `docs/*`, `api/docs/*`, and the repo-local skills. This is the living epic tracker,
> not an implementation plan. Each buildable item spins off its own `_PLAN.md` and `_PROGRESS.md`;
> the roadmap tier is the `plans` skill.
>
> **Goal:** every rule has exactly one home, contradictions cannot survive unnoticed, each consumer
> loads only the topics it can act on, and the generic half lives in a separate repo mounted at a fixed
> path — so carving a service out of this monorepo rewrites no imports. Generic standards are
> distributed as versioned, load-on-demand Claude Code plugin skills from `Concertable/agent-standards`,
> mirroring `Infonetica/standards-docs`; a repo pays no context cost for a standard it never uses.
>
> **Scope:** guidance and architecture markdown plus the hooks that police it. Product docs (relocated by
> polyrepo N6 to the central [`Concertable/docs`](https://github.com/Concertable/docs) repo) and plan/review
> working docs are out of scope except where a guidance doc points at them.

## Why this epic exists

The corpus grew by accretion, and the file names encode no decision rule — `CODE_CONVENTIONS.md` vs
`CODE_PATTERNS.md` declares a clean naming/structure axis that both sides then violate, so "where does
this rule go?" has no answer and rules landed wherever was open. The measurable results, all verified
against `origin/main` at `dc037f477`: ten contradictions between simultaneously-loaded docs, seeding
rules in five places with the copies already drifting, twelve dangling references, and ~1,700 lines
auto-loaded on any `api/**` prompt.

Upstream work already solved *reachability* — `.agents/hooks/docs_reachability.py` enforces that every
`*/agents/*.md` is loaded from somewhere. What nothing checked was whether the loaded docs were
**correct** or **non-duplicated**; `docs-review` discards pre-existing issues on unchanged lines, so a
standing contradiction was structurally invisible to the one process meant to find it.

## Items

| Done | Item | What it delivers | Depends on |
|---|---|---|---|
| [x] | `docs/guidance-reconcile` | Reconcile the ten contradictions and every dangling reference; delete the two obsolete north-star docs; extend the reachability hook to fail on dead and root-absolute links | — |
| [x] | `docs/agent-standards` | Move the generic conventions out as `.agents`-canonical, load-on-demand skills — 35 total, split by whether the rule names this product: 28 generic in `tomjseery/dotagents` (`~/.agents/skills/`), 7 Concertable process ones in `Concertable/agent-standards` | docs/guidance-reconcile |
| [x] | `docs/guidance-restructure` | Reduce `api/agents/*` and `app/agents/*` to the in-repo hard floor (2,681 lines → ≈330), give each service a thin `CODE_CONVENTIONS.md`/`CODE_PATTERNS.md` of its own precedents, and collapse every duplicated rule to one home | docs/agent-standards |
| [x] | `docs/guidance-autoload` | Delivered by `docs/guidance-restructure` rather than separately: an `api/**` prompt loads 78 lines with zero `@`-imports (from 1,428 at `dc037f477`), root `AGENTS.md` is 150 (from 300), and the merge loop and Docker block are one-line pointers at the skills that automate them | docs/guidance-restructure |
| [x] | `docs/polyrepo-ready` | **DONE.** The readiness corpus is fully re-homed so the future service repos inherit a correct corpus on day one: Phase 1 + N1–N7a (plan process, all 28 workflow skills, the route-table convention + generator, `api/AGENTS.md`, `api/ARCHITECTURE.md` + `MICROSERVICES_ARCHITECTURE.md`, root `AGENTS.md`, `docs/`, `plans/AGENTS.md` — each re-homed to the plugins, values left behind), and N8 proved a carved Payment service stands alone (#764) — surfacing and fixing a real generator drift (agent-standards #34 repointed the CANONICAL to `\.ArchitectureTests`; the live table was regenerated, #766, so `gen --check` is clean), landing the two `PLANS.md` follow-ups, and homing `auto-memory` in the agent-process plugin. Plan + ledger deleted (git history is the archive). The one residual — relocating the plan *documents* (was "N7b") — is **cut-time** work, not readiness work: it lives on [`POLYREPO_ROADMAP`](../platform/POLYREPO_ROADMAP.md) §4c (plans-locality), gated on the cut's still-open colocation sub-decision. The cut itself is a separate, launch-gated roadmap. | docs/guidance-restructure |
| [ ] | `docs/analyzer-pushdown` | Set `EnforceCodeStyleInBuild` so `severity = error` style rules actually fail a build, move what prose re-argues into `.editorconfig`, and document the rules currently enforced with no written home (`MA0053`, file-scoped namespaces) | docs/guidance-restructure |

## The corpus is not polyrepo-ready, and that is the next item

**Recorded 2026-08-19, after the restructure shipped.** `docs/guidance-restructure` split the corpus by
*portability* — generic rules out to `dotagents`/`react-agents`, this system's roster to
`agent-standards`, the floor in-repo. That was the right axis, but it was applied as if this repo
survives. [`plans/platform/POLYREPO_ROADMAP.md`](../platform/POLYREPO_ROADMAP.md) records the ruling that
it does not: services become independently-developed repos, so `api/` and a shared `plans/` tree are
destinations with no future.

What that leaves, measured rather than estimated:

- ~~**Generic plan process sits in a repo that is going away.**~~ **Fixed** — 324 lines by `wc -l`
  (`plans/agents/PLAN.md` 233, `PROMPTS.md` 57, `plans/agents/ROADMAP.md` 34; 32 of them naming anything
  Concertable-specific; the 259 recorded earlier was the same files counted non-blank) now
  live in `standards/process/PLANS.md` and `HANDOFF.md`. Six sibling process docs had already moved; the
  asymmetry was the restructure scoping `PLANS.md` narrowly and nobody re-deriving it.
- **Three route rows are anchored on the monorepo layout** — the `^api/` and `^app/` area floors and
  `^plans/`. None of those path prefixes exist in a standalone service repo. The four layer routes
  (`.Application/`, `.Api/`, `.Domain/`, `.Infrastructure/`) key on architecture rather than location and
  survive the cut unchanged, which is the shape the re-anchor should follow.
- **The hub docs open by describing a monorepo**, so a carved service repo inherits a premise that is
  false there.

**Why copying is not the answer.** A developer works on two or three services, not one and not all — so
the standards must be identical across the repos they clone. Plugin delivery already gives that: install
once per machine, not per repo. Committing `plans/agents/` into eight repos means one copy is edited and
seven go stale, which is the drift this epic exists to remove, reintroduced at repo scale.

**Not to be confused with `POLYREPO_ROADMAP` item 4c**, which asks where a *plan document* spanning four
services physically lives. That is genuinely contentious and gated on its §6. How to *write* a plan is
not; it is generic process with no locality question at all.

## Standing principles

These outlive any single item and belong in `docs/INDEX.md` rather than here:

- One rule, one home; everywhere else links and never restates.
- Scope is set by the load trigger, not by folder position. A skill's `description` is the router, so it
  must name both the content and the occasion; a vague one means the skill silently never loads.
- Sort a rule by the cost of missing it, not by topic. Load-on-demand is only safe for rules the task
  itself will summon; a rule whose violation is silent and costly stays in the repo's `AGENTS.md`.
- Generic topics live in the shared repo; Concertable specifics live in the consumer's own `agents/`.
  Nothing straddles, which is what keeps a carve-out import-neutral.
- If a machine can enforce it, the doc gets one line and the diagnostic or test name.
- Never name violation sites in a rule doc; they get fixed and the citation rots silently.
- A doc is either `@`-imported or summarized, never both.
- Check the code before writing the rule down. Several rules taught things the codebase had already
  moved past, and every one of them read as maintained.
