# Docs review — Docs/launch_admin-console-plan

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `d023c9b56b78ff7c4c8ea88ac3d2eb8afca6d8a1`  _(2026-08-16)_

> Range reviewed: `b633d79a..d023c9b5` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **HOME1 — MEDIUM — Lens C (right home)** — `plans/launch/ADMIN_CONSOLE_PLAN.md:7`
  The plan's Context section cited `plans/launch/LAUNCH_ROADMAP.md` by path (plus section numbers). Per
  `plans/agents/ROADMAP.md`: "A plan's file must not cite the roadmap — but an agent working the plan
  may read it." Fixed: dropped the citation; the roadmap link legitimately lives in the ledger's
  `Roadmap:`/`Roadmap item:` headers instead.

- [x] **ACC1 — MEDIUM — Lens A (accuracy vs reality)** — `plans/launch/ADMIN_CONSOLE_PLAN.md:73-74`
  Claimed `plans/platform/CONFIG_AND_DEPLOYMENT_PLAN.md` "doesn't exist yet" — the file exists (it's the
  roadmap's `launch/production deployment + config/secrets` plan doc); only the secret store it designs
  is unbuilt. Fixed: reworded to "the still-open ... gate, planned in ... but not yet built".

- [x] **INST1 — LOW — Lens F (followable instruction)** — `plans/launch/ADMIN_CONSOLE_PLAN.md:165` (Phase 2)
  "OIDC client id `admin-web`... — no, reuse `ClientIds.Admin`..." read as a leftover visible
  self-correction rather than a clean instruction. Fixed: rewritten as a direct statement, with the
  Venue/Artist naming parallel made explicit instead of implied by the discarded alternative.
