# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Also delivered by this ledger: roadmap item `docs/agent-standards`, now checked off
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637 — **open working branch, NOT for merge** (see the standing constraint in `## Next Steps

**The model is settled and written down; the trees do not match it yet.** Authoritative statement:
`dotagents/AGENTS.md` (mirrors to `~/AGENTS.md`) and `dotagents/ARCHITECTURE.md`; file-by-file target in the
plan's "target structure — four tiers" section. Three superseded models were purged — do not reintroduce a
merged standards repo, `dotagents` holding both stacks, or a `platform/`/`concertable/` folder.

Audit findings live in `reviews/Docs-GuidanceDocsRestructure-AuditFindings.md` (16 defects). Several move
depending on which tier a doc lands in, so the split comes first.

1. **Create `tomjseery/react-agents` and move `standards/react/` into it** — 9 docs plus their routers, out
   of `dotagents`. `dot` is dotNET; React never belonged there. Port `sync-generated.ps1`, `payloads.json`,
   a marketplace, a `react-standards` plugin and the CI `-Check` job; drop the `react` domain from
   `dotagents`' payload map. `deploy-skills.ps1` gains the third standards root, and its duplicate-domain
   guard already covers the collision.

2. **Add `standards/dotnet/` and `standards/react/` to `agent-standards`, and re-home the 480 lines** still
   in `api/agents/` (`CODE_CONVENTIONS`, `CODE_PATTERNS`, `INTEGRATION_CONVENTIONS`, `MODULE_STRUCTURE`,
   `RESULT_PATTERN`, `SEEDING_CONVENTIONS`) and `app/agents/` (`CODE_CONVENTIONS`, `CODE_PATTERNS`). Target
   docs are named in the plan. `api/agents/` is then **deleted** — the polyrepo cut leaves no `api/` node to
   host it. Per-service and per-module `AGENTS.md` stay, and may name sibling docs (`CODE_CONVENTIONS.md`)
   where a module has its own conventions.

3. **Cut the process corpus over (P0 in the findings).** `standards/process/` was copied, not moved: its
   Concertable originals sit at full length and nothing references the extracted docs — zero hits for any
   process skill name across Concertable markdown, and `MERGING.md` duplicates root `AGENTS.md`'s poll loop
   near-byte-for-byte. Slim the Concertable originals to Concertable-only procedure and point at the skills,
   exactly as the React half already does.

4. **Fix the 16 audit defects**, P0 correctness first: the `[LoggerMessage]` carve-out (the corpus currently
   contradicts itself), the `XMappers` examples that demonstrate a banned form, and `PERSISTENCE.md`'s
   impossible repository signature. Then the paraphrase-losses — `axios`, `Reqnroll`/`Playwright`, `Aspire`
   and its four extension methods, `Docker`/`pre-login handshake`, `Monitor`, the `NAMING.md` precedent
   column, `[Collection]`/`InitializeAsync`, `Environments`/`IHostEnvironment`, the raw-hook litmus members,
   the TanStack API names, `silenceErrors`, the retry cap, `grep -rniE`.

5. **Move Concertable's four remaining hooks to `agent-standards`** — `plan_handoff_stop.py` + launcher,
   `plan_graph.py`, `docs_reachability.py`, `merge-review-gate.py`. They enforce standards that moved, so
   enforcement now sits apart from its rule with nothing watching for drift. **Verify a plugin `Stop` hook
   fires with zero repo wiring first** — that was proven only for `PreToolUse`.

6. **Then the remaining phases**: 5c (the discovery pass — conventions that exist only in code, e.g. B2B's
   stance taxonomy), 3c (markdown outside the conventions folders), 4 (the last duplication rows), and the
   deferred auto-load thinning of root `AGENTS.md`.

**#637 does not merge until steps 1-3 land.** It deletes 2,662 lines whose replacement is now organized and
installable (the plugin install was proven in both harnesses, 2026-08-18) but not yet correctly *tiered* —
`react-agents` does not exist and `agent-standards` has no stack sections, so the corpus it points at is not
the one the model describes.

**Tommy's, not agent work:** approve the Codex `PreToolUse` hook once in a Codex session in this worktree;
archive `agent-starter-kit`; rule on `GenreController` in a shared library
(`api/Concertable.Shared/TECH_DEBT.md:70`); decide whether React Hook Form is adopted (in no `app/` workspace
today); settle the Shouldly-for-unit-tests open call now recorded in `dotnet/testing/UNIT.md`.

## Also Tommy's, not blocking

`tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private, the
same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
