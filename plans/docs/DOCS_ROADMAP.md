# Guidance-docs roadmap

> **Roadmap** for settling the agent/developer guidance corpus — root `AGENTS.md`, `api/agents/*`,
> `app/agents/*`, `docs/*`, `api/docs/*`, and the repo-local skills. This is the living epic tracker,
> not an implementation plan. Each buildable item spins off its own `_PLAN.md` and `_PROGRESS.md`; see
> [`../agents/ROADMAP.md`](../agents/ROADMAP.md).
>
> **Goal:** every rule has exactly one home, contradictions cannot survive unnoticed, each consumer
> loads only the topics it can act on, and the generic half lives in a separate repo mounted at a fixed
> path — so carving a service out of this monorepo rewrites no imports.
>
> **Scope:** guidance and architecture markdown plus the hooks that police it. Product docs
> (`docs/USP.md`, `docs/OVERVIEW.md`) and plan/review working docs are out of scope except where a
> guidance doc points at them.

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
| [ ] | `docs/conventions-repo` | Create the shared `conventions` repo (`dotnet/`, `typescript/`, `process/`), mount it at repo root as a pinned submodule, and add `submodules: true` to CI checkout | docs/guidance-reconcile |
| [ ] | `docs/guidance-restructure` | Split `api/agents/*` and `app/agents/*` into one-topic-per-file under `conventions/`, compose per consumer via `@`, and collapse every duplicated rule to one home | docs/conventions-repo |
| [ ] | `docs/guidance-autoload` | Cut the auto-loaded floor: drop the three `@`-imports, and reduce the always-loaded merge and Docker blocks that `/merge` and `scripts/e2e.ps1` already automate | docs/guidance-restructure |
| [ ] | `docs/analyzer-pushdown` | Set `EnforceCodeStyleInBuild` so `severity = error` style rules actually fail a build, move what prose re-argues into `.editorconfig`, and document the rules currently enforced with no written home (`MA0053`, file-scoped namespaces) | docs/guidance-restructure |

## Standing principles

These outlive any single item and belong in `docs/INDEX.md` rather than here:

- One rule, one home; everywhere else links and never restates.
- Scope is set by the import edge, not by folder position: a topic file is imported only by consumers
  that have the thing. A folder cannot express scope.
- Generic topics live in the shared repo; Concertable specifics live in the consumer's own `agents/`.
  Nothing straddles, which is what keeps a carve-out import-neutral.
- If a machine can enforce it, the doc gets one line and the diagnostic or test name.
- Never name violation sites in a rule doc; they get fixed and the citation rots silently.
- A doc is either `@`-imported or summarized, never both.
- Check the code before writing the rule down. Several rules taught things the codebase had already
  moved past, and every one of them read as maintained.
