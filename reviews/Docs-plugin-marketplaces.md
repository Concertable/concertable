# Docs review — Docs/plugin-marketplaces

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `7557290e68de9a43ce1bd1b95cb61e6b9812bd22`  _(2026-08-20)_

> Range reviewed: `c4c83ee1..7557290e` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked accuracy vs reality, cross-doc contradiction, doc home & convention,
harness-reloaded concision, dangling references, and followable instruction.

The single-file diff (`.claude/settings.json`) adds `extraKnownMarketplaces` (agent-standards,
dotagents, react-agents, each a real GitHub `source: github` entry matching the repos AGENTS.md
already names) and `enabledPlugins` (the five plugins those marketplaces publish: agent-process,
dotnet, react, dotnet-standards, react-standards). Verified live via `claude plugin marketplace add`
/ `claude plugin install --scope project`, which reported success for all eight entries and produced
this exact diff. `.claude/settings.json` is the schema-correct, checked-in home for a project-scoped
plugin declaration — no prose to contradict, nothing to restate, no transient reference.
