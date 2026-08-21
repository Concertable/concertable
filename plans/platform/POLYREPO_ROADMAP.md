# Polyrepo Roadmap — services as genuinely standalone repos

> **Roadmap** for the polyrepo epic — the living progress tracker, not a plan (no `_PROGRESS.md`, never
> deleted, lives until the epic ships). Each buildable item spins off its own feature plan; the
> roadmap tier is the `plans` skill.
>
> **North star:** [`api/AGENTS.md`](../../api/AGENTS.md) — *"The monorepo is a convenience only. Each
> service is independently owned and will split into its own repo with its own developers. Design every
> change as if that split already happened: would this still work if this service lived alone?"* This
> roadmap tracks the work that makes that literally true.
>
> **Definition of done for the epic:** each service (`B2B`, `Customer`, `Auth`, `Payment`, `Search`,
> `Shared`) builds, tests, **documents, and plans** itself standing alone; every cross-service dependency
> is Contracts/published-package only; a mirror (or a true cut) of any service folder is a coherent,
> self-describing repo.
>
> **Scope note:** this tracks only the polyrepo/services-as-repos streams. Other `plans/platform/` plans
> (deployment, DNS, pipeline redesign, E2E strategy) are separate concerns and are **not** tracked here.
>
> **Companion / standing docs:** [`POLYREPO.md`](POLYREPO.md) (mirror runbook),
> [`../../api/docs/MICROSERVICES_ARCHITECTURE.md`](../../api/docs/MICROSERVICES_ARCHITECTURE.md),
> [`../../api/ARCHITECTURE.md`](../../api/ARCHITECTURE.md).

---

## Status — what's shipped vs. what's left

**Shipped — verified, don't rebuild:** in-monolith decomposition (god-`ConcertEntity` split, `Shared.*`
collapsed to Kernel+Contracts, User TPH dismantled, Auth identity-only) · first cross-process extraction
(Customer on its own host + DB) · the **backend carve** (feed `PackageReference`s, per-folder CPM,
`EnforceServiceBoundary`, `carve-*` CI, `platform-sync`) · six standalone mirror repos exist and the split
mechanism (prefix `git subtree split`) is proven.

**In flight:** the **frontend full-stack carve** (`POLYREPO_FULLSTACK_PLAN`, Phase 2 done on branch, Phase
3 left).

**Partly shipped:** per-service **doc & guidance locality** (§4) — the ownership rule + per-service
`AGENTS.md`/`ARCHITECTURE.md` gaps landed (PR #383); only **4c** (plans-tree relocation, gated on §6) remains.

**Deferred by decision:** mirror automation is off the hot path (manual `workflow_dispatch`); the
end-state shape (buildable mirrors vs. a true cut) is undecided.

---

## 1. Backend decomposition & extraction — 🟠 mostly done

Owned by [`MICROSERVICE_STEPS_PLAN.md`](MICROSERVICE_STEPS_PLAN.md) (+
[`MICROSERVICE_STEPS_CONT_PLAN.md`](MICROSERVICE_STEPS_CONT_PLAN.md)) and the leftover-coupling list in
[`TECHNICAL_DEBT.md`](TECHNICAL_DEBT.md).

- [x] ✅ **Phase 1 — in-monolith decomposition.** `ConcertEntity` split, `Concertable.Shared.*` → Kernel +
  Contracts, `SharedDbContext`/Genre relocated, User TPH dismantled, Auth identity-only, Search upstream
  refs cleaned.
- [x] ✅ **Phase 2 — first extraction (Customer).** Customer API + Workers on their own host + DB; bus,
  outbox/inbox introduced.
- [x] ✅ **Phase 3–4.** (Steps 12–16 per the plan's status header.)
- [ ] 🟠 **Phase 5 — event schema versioning** (Step 17). Outstanding.
- [ ] 🟠 **IVT / legacy-coupling retirement** — `A1`–`A7` in [`TECHNICAL_DEBT.md`](TECHNICAL_DEBT.md):
  internal-visibility grants and legacy-host consumption of module internals, retired as the owning steps
  land.

## 2. Backend carve — ✅ done

Cross-service deps go through published `Concertable.*` packages, not project references: feed
`PackageReference`s, per-folder Central Package Management, the `EnforceServiceBoundary` guard, `carve-*`
CI jobs, and `platform-sync` (MinVer bump + `<ConcertablePlatformVersion>` sync PR on every `api/**`
merge). This is the backend half of "builds alone from a feed." Documented in
[`../../api/ARCHITECTURE.md`](../../api/ARCHITECTURE.md) ("Cross-service contract distribution" /
"Per-folder build closures").

## 3. Frontend full-stack carve — 🟡 in progress

Owned by [`POLYREPO_FULLSTACK_PLAN.md`](POLYREPO_FULLSTACK_PLAN.md) /
[`POLYREPO_FULLSTACK_PROGRESS.md`](POLYREPO_FULLSTACK_PROGRESS.md). Makes `customer` and `b2b` genuine
full-stack units (their `api/` service **plus** web + mobile), each restoring shared FE code from a package
feed — the npm analogue of the backend carve.

- [x] ✅ **Phase 0** — scoped npm registry + PAT.
- [x] ✅ **Phase 1** — publish the universal core `@concertable/shared` (published, restorable).
- [x] ✅ **Phase 2** — package the four remaining tiers + cut consumers over (done on branch, PR pending).
- [ ] 🟡 **Phase 3** `platform/polyrepo-fullstack` — prove each surface feed-restores its shared deps, `carve-fe-{customer,b2b}` CI, FE
  import-boundary rule, and close the Phase-2 metro/nativewind/tailwind + carve-CSS runtime deferrals.

- [ ] **B2B package topology** `platform/b2b-package-topology` - separate the manager-web tier as
  `@concertable/web-b2b`, retain `@concertable/b2b` as the cross-platform B2B core, and migrate web
  and mobile consumers through [`B2B_PACKAGE_TOPOLOGY_PLAN.md`](B2B_PACKAGE_TOPOLOGY_PLAN.md).

## 4. Per-service doc & guidance locality — 🟠 4a + 4b shipped; 4c deferred

**The stream this roadmap was created to drive.** Guidance was only partly co-located with the service that
owns it, so a mirror of a service folder was *not* a self-describing repo and an agent working one service
loaded root-level noise instead of that service's own rules. **4a + 4b shipped in PR #383** (ownership rule
+ per-service `AGENTS.md`/`ARCHITECTURE.md` gaps); their plan (`SERVICE_DOC_LOCALITY_*`) is deleted, git
history is the archive. **4c** (relocating the cross-cutting `plans/` tree) remains — gated on the §6
end-state decision.

**Ownership rule to establish first (the design decision):** *each artifact lives at the lowest node that
fully contains its concern.* Single-service → the service folder (and it rides the mirror when run);
multi-service or monorepo-orchestration → root. This is the rule that decides every move below and must be
written into [`../../AGENTS.md`](../../AGENTS.md) + [`../AGENTS.md`](../AGENTS.md) before any files move.
The existing `Concertable.Payment` thin `CLAUDE.md → @AGENTS.md` pair (service-specific rules only,
inheriting root + `api/` upward) is the template.

Gap map (verified 2026-08-05):

| Artifact | State | Outstanding |
|---|---|---|
| `TECH_DEBT.md` | ✅ per-service | — |
| `README.md` | ✅ per-service | — |
| `ARCHITECTURE.md` | ✅ B2B, Customer, Auth, AppHost, **Payment, Search** | Messaging skipped (shared library, not a data/adapter service) |
| service-root `AGENTS.md` | ✅ Payment, **B2B, Customer, Auth** | Search + Messaging skipped (nothing beyond upward guidance) |
| `plans/` | 🔴 centralized by *initiative* | see the seam decision below |

- [x] ✅ **4a — Ownership rule.** "Lowest fully-containing node" written into root + `api/` `AGENTS.md`, single-sourced; `Concertable.Payment` named as the thin-file template.
- [x] ✅ **4b — Fill the cheap gaps.** Thin `CLAUDE.md`/`AGENTS.md` for B2B, Customer, Auth; `ARCHITECTURE.md` for Payment + Search. Search + Messaging `AGENTS.md` and Messaging `ARCHITECTURE.md` skipped (lazy creation — nothing service-specific).
- [ ] 🔴 **4c — Plans locality (the contentious part).** `plans/` is organized by *initiative*, and many initiatives (`launch`, `typed-result`, `marketplace`) span every service and **cannot** live in one service folder. So this is not "push everything down": a single-service plan moves into its service; a cross-service/orchestration plan stays at root. Settle this seam **with §6** before moving live plans, and never relocate an in-flight plan with a live worktree/ledger (e.g. `POLYREPO_FULLSTACK`) mid-flight.

**Sequencing:** 4a + 4b are done (PR #383); 4c holds until the end-state seam (§6) is decided.

## 5. Mirror automation — ⏸ deferred by decision

[`mirror.yml`](../../.github/workflows/mirror.yml) is `workflow_dispatch`-only (taken off the hot path as
tech-debt N7: "a full 6-service history rewrite for a split-repo future nothing consumes yet"). A nightly
`mirror-parity.yml` flags drift. Re-enable/automate when a mirror repo is actually consumed downstream —
gated on §6. (`mirror.yml` still references a `POLYREPO_COMPLETION.md` that no longer exists; fix or drop
that reference as part of this stream.)

## 6. End-state shape — ✅ decided 2026-08-18: a true one-way cut

**Tommy's ruling: the monorepo goes.** Services become independently-developed repos, not buildable
read-only mirrors. This unblocks §4c and settles the guidance question that was blocked behind it: there
is no `api/` node in a polyrepo, so `api/agents/` and `api/AGENTS.md` are destinations with no future.
Everything in them re-homes to `standards/` (platform-wide, inherited by every service repo) or to the
owning service's repo. **Done 2026-08-19:** `api/agents/` and `app/agents/` are deleted, the generic half
lives in `tomjseery/dotagents` and `tomjseery/react-agents` and this system's roster in
`Concertable/agent-standards`, all delivered as plugins; `api/AGENTS.md` is 78 lines of pointers with no
`@`-imports. `docs/INDEX.md` maps topic to owner.

**When the cut runs — gated on the launch plan.** Executing the cut — creating the service repos and
deleting the monorepo — does **not** begin until the entire launch plan
([`plans/launch/LAUNCH_ROADMAP.md`](../launch/LAUNCH_ROADMAP.md)) is delivered; that is months out. What
runs first, and now, is the polyrepo-*ready* corpus work
([`plans/docs/POLYREPO_READY_PLAN.md`](../docs/POLYREPO_READY_PLAN.md)): re-homing every rule out of the
doomed nodes so the eventual repos inherit a correct corpus on day one. `api/AGENTS.md` is one of those
nodes (its N3) and re-homes **well before** the cut — it is not itself launch-gated; only the physical
split is. Until N3 lands, `api/AGENTS.md` stays as the retained 78-line pointer floor.

The remaining sub-decision is still open: whether a true cut restructures to per-service colocation
(`services/<x>/{api,web,mobile}`) or uses a multi-source mirror assembler.

### Original framing (kept for the trade-off it records)

The open architecture decisions (D-A / D-B in [`POLYREPO_FULLSTACK_PLAN.md`](POLYREPO_FULLSTACK_PLAN.md)):

- **Buildable read-only mirrors** (monorepo stays source of truth, today's model) **vs. a true one-way
  cut** to independently-developed repos.
- If a true full-stack cut: **restructure to per-service colocation** (`services/<x>/{api,web,mobile}`) **vs.
  a multi-source mirror assembler** (`git subtree split` takes one prefix, so it can't fuse a service's
  api + web + mobile + shared as-is).

This gate governs how much to invest in §5, and whether §4c's plan-locality moves should also anticipate a
`services/<x>/` layout. **Resolve at the root architecture level, not inside a feature PR.**

---

## Decision log

- **2026-08-05 — Roadmap created.** The polyrepo epic existed as a roadmap-less cluster
  (`MICROSERVICE_STEPS`, backend carve, `POLYREPO_FULLSTACK`, deferred mirroring); this roadmap unifies it
  and adds per-service doc & guidance locality (§4) as a tracked stream. Anchor for the doc-locality work
  Tommy raised.
- **2026-08-05 — §4 4a + 4b shipped (PR #383).** Ownership rule ("lowest fully-containing node") written
  into root + `api/` `AGENTS.md`; thin service-root `AGENTS.md`/`CLAUDE.md` added for B2B, Customer, Auth
  and `ARCHITECTURE.md` for Payment + Search (Search + Messaging `AGENTS.md` and Messaging `ARCHITECTURE.md`
  skipped — nothing service-specific). Docs-reviewed (3 accuracy findings fixed). Owning plan
  `SERVICE_DOC_LOCALITY_*` deleted; git history is the archive. **4c** (plans-tree relocation) remains, gated on §6.
- **2026-08-20 — Cut execution gated on launch.** The one-way cut (repo creation + monorepo deletion) waits
  for the entire launch plan to ship. The polyrepo-ready corpus re-home (`POLYREPO_READY_PLAN.md`, incl.
  `api/AGENTS.md` N3) is the prerequisite that runs first and is not launch-gated.
