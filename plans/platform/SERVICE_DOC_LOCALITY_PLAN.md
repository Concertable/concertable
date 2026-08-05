# Service documentation & guidance locality

**Next steps live in @plans/platform/SERVICE_DOC_LOCALITY_PROGRESS.md → `## Next Steps`.**

## Problem

Guidance and architecture docs are only partly co-located with the service that owns them. A mirror of a
service folder is therefore *not* a self-describing repo, and an agent working one service loads
root-level noise instead of that service's own rules. The pattern is already established (`TECH_DEBT.md`
and `README.md` are per-service; `Concertable.Payment` has a thin `CLAUDE.md → @AGENTS.md` service-root
pair) — this finishes it.

## Governing rule (decide once, apply everywhere)

**Each artifact lives at the lowest node that fully contains its concern.** Single-service → the service
folder (and it rides the mirror when run). Multi-service or monorepo-orchestration → root. A service-root
guidance file is **thin**: only rules genuinely specific to that service, inheriting root + `api/` upward —
never a restated copy. **Lazy creation:** create a file only where real service-specific content exists;
no filler to fill a slot.

## Scope

**In:** the ownership rule, plus the per-service `AGENTS.md` / `ARCHITECTURE.md` gaps below.
**Out (deferred):** relocating the cross-cutting `plans/` tree (organized by initiative — `launch`,
`typed-result`, `marketplace` span every service and can't live in one folder). That move is entangled
with the undecided polyrepo end-state layout (`services/<x>/{api,web,mobile}` vs. mirror assembler) and
waits for that decision.

Gap map (verified 2026-08-05):

| Service | service-root `AGENTS.md` | `ARCHITECTURE.md` |
|---|---|---|
| Payment | ✅ has one | 🔴 missing |
| B2B | 🔴 missing | ✅ has one |
| Customer | 🔴 missing | ✅ has one |
| Auth | 🔴 missing | ✅ has one |
| Search | 🔴 missing | 🔴 missing |
| Messaging | 🔴 missing | 🔴 missing (shared infra lib — may not warrant one) |

## Nature of this plan

**Docs-path, doc-only** — pure markdown, no code, so no build/unit/integration/E2E gate; the verification
gate per phase is a coherence read. Exempt from branch hygiene (work on the current branch or a light
`Feature/platform_service-doc-locality`; no dedicated worktree needed). Merges via `/merge-docs`. The
three phases have no inter-phase build/publish gate, so they may land in one PR or incrementally.

## Phases

### Phase 1 — Establish the ownership rule ✅ done (working tree)
- **Change:** add the "lowest fully-containing node" rule (above, condensed to ≤2 lines) to root
  `AGENTS.md`, and a one-phrase cross-reference from `api/AGENTS.md`. Name the thin-service-file pattern
  with `Concertable.Payment` as the template.
- **Why:** it's the decision that governs every move below; single-source it so per-service files stay
  thin and two copies can't drift.
- **Constraint:** root `AGENTS.md` loads every prompt — minimum words, no examples/restating.
- **Gate:** the rule reads coherently and is stated once (not duplicated per service).

### Phase 2 — Service-root `AGENTS.md` for the 5 missing services ✅ done (working tree)
- **Verdict:** CREATE thin `CLAUDE.md → @AGENTS.md` for B2B, Customer, Auth; SKIP Search + Messaging (all guidance already upward, no local footgun).
- **Change:** for B2B, Customer, Auth, Search, Messaging, add a thin `CLAUDE.md` (`@AGENTS.md`) +
  `AGENTS.md` pair holding only genuinely service-specific rules — consolidated from where they're
  currently scattered (module/test `AGENTS.md`, service-specific notes in `api/AGENTS.md`), not
  duplicated from upward guidance.
- **Why:** an agent in a service loads that service's rules; a mirror carries them.
- **Investigation per service (part of the phase):** enumerate what is genuinely service-specific before
  writing; if a service has nothing beyond what `api/AGENTS.md` already says, do **not** manufacture a
  file (lazy creation) — record that it was intentionally skipped.
- **Gate:** each new file is service-specific, inherits upward, restates nothing.

### Phase 3 — Service `ARCHITECTURE.md` for the missing services ✅ done (working tree)
- **Verdict:** CREATED Payment + Search `ARCHITECTURE.md` and extended Payment `AGENTS.md`; Messaging skipped (shared library, not a data/adapter service).
- **Change:** add `ARCHITECTURE.md` where a genuine service-local architecture exists to document:
  **Payment** (Stripe Connect Express, escrow/settlement ledger, adapter-service model), **Search**
  (index/projection model). **Messaging** is a shared infra library, not a data/adapter service — decide
  during the phase whether it warrants a full `ARCHITECTURE.md` or a shorter note, and skip rather than
  pad if not.
- **Why:** same locality principle; document the service's own shape where one exists.
- **Gate:** each doc describes real service-local architecture; nothing manufactured.
