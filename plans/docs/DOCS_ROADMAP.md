# Guidance-docs roadmap

> **Roadmap** for settling the agent/developer guidance corpus — root `AGENTS.md`, `api/agents/*`,
> `app/agents/*`, `docs/*`, `api/docs/*`, and the repo-local skills. This is the living epic tracker,
> not an implementation plan. Each buildable item spins off its own `_PLAN.md` and `_PROGRESS.md`; see
> [`../agents/ROADMAP.md`](../agents/ROADMAP.md).
>
> **Goal:** every rule has exactly one home, contradictions cannot survive unnoticed, the auto-loaded
> weight is proportionate to what a prompt actually needs, and the portable half can be lifted into a
> shared .NET conventions repo consumed by every project — by `git mv`, not by rewriting.
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
| [ ] | `docs/guidance-restructure` | Move `api/agents/*` and `app/agents/*` into `conventions/portable/` + `conventions/local/`, split the oversized files, and collapse every duplicated rule to one home | docs/guidance-reconcile |
| [ ] | `docs/guidance-autoload` | Cut the auto-loaded floor: drop the three `@`-imports, and reduce the always-loaded merge and Docker blocks that `/merge` and `scripts/e2e.ps1` already automate | docs/guidance-restructure |
| [ ] | `docs/analyzer-pushdown` | Set `EnforceCodeStyleInBuild` so `severity = error` style rules actually fail a build, move what prose re-argues into `.editorconfig`, and document the rules currently enforced with no written home (`MA0053`, file-scoped namespaces) | docs/guidance-restructure |
| [ ] | `docs/conventions-extraction` | Lift `conventions/portable/` into a shared .NET conventions repo consumed by every project, leaving a pointer behind | docs/guidance-restructure; docs/analyzer-pushdown |

## Standing principles

These outlive any single item and belong in `docs/INDEX.md` rather than here:

- One rule, one home; everywhere else links and never restates.
- No file straddles `portable/` and `local/` — that is what keeps extraction a move.
- If a machine can enforce it, the doc gets one line and the diagnostic or test name.
- Never name violation sites in a rule doc; they get fixed and the citation rots silently.
- A doc is either `@`-imported or summarized, never both.
- Check the code before writing the rule down. Several rules taught things the codebase had already
  moved past, and every one of them read as maintained.
