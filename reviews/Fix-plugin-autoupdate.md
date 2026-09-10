# Docs review — Fix/plugin-autoupdate

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `e6a4ed8a15b4e6ad3edb3ffd0ce9be28e2b54932`  _(2026-08-26)_

> Range reviewed: `e4c91fe2..e6a4ed8a` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked accuracy vs reality, cross-doc contradiction, doc home & convention,
harness-reloaded concision, dangling references, and followable instruction.

The single-file diff (`.claude/settings.json`) adds `"autoUpdate": true` as a sibling of the existing
`source` field on each of the three `extraKnownMarketplaces` entries (agent-standards, dotagents,
react-agents). `autoUpdate` is a real field on Claude Code's own marketplace-declaration schema (verified
directly against the installed CLI binary, not assumed); the shape mirrors `agent-standards`' own
same-day `.claude/install.ps1::Enable-MarketplaceAutoUpdate`, which sets the identical field for user
scope. No prose elsewhere describes marketplace auto-update behavior to contradict, no other doc names
this block, and the change is additive JSON with no runtime, package, or schema surface.
