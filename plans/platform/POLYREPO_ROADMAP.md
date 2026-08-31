# Polyrepo Roadmap — services as genuinely standalone repos

> **Roadmap** for the polyrepo epic — the living progress tracker, not a plan (no `_PROGRESS.md`, never
> deleted, lives until the epic ships). Each buildable item spins off its own feature plan; the
> roadmap tier is the `plans` skill.
>
> **North star:** [`ARCHITECTURE.md`](../../ARCHITECTURE.md) — *"The monorepo is a convenience only — the
> backend services are independently-owned microservices and are designed to split into separate repos."*
> Design every change as if that split already happened: would this still work if this service lived alone?
> This roadmap tracks the work that makes that literally true.
>
> **Definition of done for the epic:** each service (`B2B`, `Customer`, `Auth`, `Payment`, `Search`,
> `Shared`) builds, tests, **documents, and plans** itself standing alone; every cross-service dependency
> is Contracts/published-package only; and the cut (§6) has run, so each service folder **is** its own
> coherent, self-describing repo.
>
> **Scope note:** this tracks only the polyrepo/services-as-repos streams. Other `plans/platform/` plans
> (deployment, DNS, pipeline redesign, E2E strategy) are separate concerns and are **not** tracked here.
>
> **The cut itself — "repository per microservice" — is §6, and its plan is
> [`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`](REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md), with its
> exclusive active-stream ledgers listed in §6.** It is named for the target shape rather than for this epic,
> so a search for "polyrepo" does not find it — start at §6.
>
> **Companion / standing docs:**
> [`../../api/docs/MICROSERVICES_ARCHITECTURE.md`](../../api/docs/MICROSERVICES_ARCHITECTURE.md).

---

## Status — what's shipped vs. what's left

**Shipped — verified, don't rebuild:** in-monolith decomposition (god-`ConcertEntity` split, `Shared.*`
collapsed to Kernel+Contracts, User TPH dismantled, Auth identity-only) · first cross-process extraction
(Customer on its own host + DB) · the **backend carve** (feed `PackageReference`s, per-folder CPM,
`EnforceServiceBoundary`, `carve-*` CI, `platform-sync`) · the extraction mechanism, proven end to end on
Payment with `git-filter-repo` (802 commits, whole `src/` runtime compiled clean off the feed).

**In flight:** the **cut** (§6) — stages 1 and 2 delivered, extraction unblocked; and the **frontend
full-stack carve** (`POLYREPO_FULLSTACK_PLAN`, Phase 3 left).

**Partly shipped:** per-service **doc & guidance locality** (§4) — the ownership rule + per-service
`AGENTS.md`/`ARCHITECTURE.md` gaps landed (PR #383); only **4c** (plans-tree relocation, gated on §6) remains.

**Retired:** mirroring (§5) — the six mirror repos, both workflows and the runbook are deleted.

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

## 5. Mirror automation — ✅ retired 2026-08-27

**The stream is closed by deletion, not by delivery.** §6 chose a true one-way cut, which makes read-only
mirrors a dead end. The six mirror repos no longer exist on GitHub (verified twice on 2026-08-27:
`gh repo list Concertable` returns only `agent-standards`, `concertable`, `docs`, `config`, `infra`), so
`mirror.yml` and `mirror-parity.yml` were force-pushing to deleted repositories. Stage 2 round-trip 3 of the
§6 plan deleted both workflows and the `POLYREPO.md` runbook. Neither workflow was a required status check.
The bootstrap role mirrors once had is superseded: the §6 plan extracts with `git-filter-repo`, proven end
to end on Payment.

## 6. End-state shape — ✅ decided 2026-08-18: a true one-way cut

**Tommy's ruling: the monorepo goes.** Services become independently-developed repos, not buildable
read-only mirrors. This unblocks §4c and settles the guidance question that was blocked behind it: there
is no `api/` node in a polyrepo, so `api/agents/` and `api/AGENTS.md` are destinations with no future.
Everything in them re-homes to `standards/` (platform-wide, inherited by every service repo) or to the
owning service's repo. **Done 2026-08-19:** `api/agents/` and `app/agents/` are deleted, the generic half
lives in `tomjseery/dotagents` and `tomjseery/react-agents` and this system's roster in
`Concertable/agent-standards`, all delivered as plugins; `api/AGENTS.md` is 78 lines of pointers with no
`@`-imports. `docs/INDEX.md` maps topic to owner.

- [ ] 🟡 **The cut itself** `platform/polyrepo-cut` — owned by
  [`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`](REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md) /
  the active-stream records below. Git history retains the deleted umbrella ledger; it does not own
  execution.
  Approved and in execution 2026-08-26. Nine repositories (five services, `platform-dotnet`,
  `platform-web`, `fleet`, `.github`); nine stages; the Payment extraction is proven end to end.
  **Stages 1–2 delivered** (all 45 test-tier cross-repository `ProjectReference`s are now packages).
  Preparation and delivery have separate dependency graphs: private service-repository preparation runs in
  parallel, while canonical rename, publication, system consumption, source removal, deployment, and archive
  remain ordered and require explicit authorization.

  | Stream | State and exclusive owner | Durable record |
  |---|---|---|
  | Stage 3 RT3 | In flight only on `Plan/RepoSplit-Stage3-Hosting-rt3`; no sibling may edit its AppHosts, composition tests, review work order, or stream state. | [`REPOSITORY_PER_MICROSERVICE_MIGRATION_RT3_PROGRESS.md`](REPOSITORY_PER_MICROSERVICE_MIGRATION_RT3_PROGRESS.md) |
  | Customer | Active in the existing private `customer` checkout; package access and exact-head CI are green, and this stream owns only checkpoint-13 repository preparation. | [`REPOSITORY_PER_MICROSERVICE_MIGRATION_CUSTOMER_FRONTEND_PROGRESS.md`](REPOSITORY_PER_MICROSERVICE_MIGRATION_CUSTOMER_FRONTEND_PROGRESS.md) |
  | Stage 4 Fleet E2E | Paused on draft [PR #896](https://github.com/Concertable/concertable/pull/896); owns only fleet/TestKit/E2E source-boundary removal. | PR #896 carries `REPOSITORY_PER_MICROSERVICE_MIGRATION_STAGE4_FLEET_PROGRESS.md`. |
  | Auth-next | Paused but implementable in the existing private `auth-next` checkout; owns only checkpoint-10 repository preparation. | [`REPOSITORY_PER_MICROSERVICE_MIGRATION_AUTH_NEXT_PROGRESS.md`](REPOSITORY_PER_MICROSERVICE_MIGRATION_AUTH_NEXT_PROGRESS.md) |
  | Payment-next | Reserved exclusively to the Payment preparation stream at `C:\Users\tommy\source\repos\payment-next` / `Chore/payment-promotion-preparation`; no open PR exists. | [`REPOSITORY_PER_MICROSERVICE_MIGRATION_PAYMENT_PROMOTION_PROGRESS.md`](REPOSITORY_PER_MICROSERVICE_MIGRATION_PAYMENT_PROMOTION_PROGRESS.md) |
  | Search-next | Reserved exclusively to the Search preparation stream at `C:\Users\tommy\source\repos\search-next` / `Chore/search-promotion-preparation`; no open PR exists. | [`REPOSITORY_PER_MICROSERVICE_MIGRATION_SEARCH_PROMOTION_PROGRESS.md`](REPOSITORY_PER_MICROSERVICE_MIGRATION_SEARCH_PROMOTION_PROGRESS.md) |

  Agents read this table and the named ledger before acting. One stream never edits a sibling ledger or
  worktree. B2B-next preparation remains unassigned until its existing
  `wip/b2b-frontend-fold-handoff` checkout is reconciled; system/fleet extraction remains with Stage 4 until
  that composition boundary lands.

  **Post-cut development-fixture terminology.** Keep the current `SeedCatalog`, `Seed.Contracts`,
  `SeedState`, and `Seed.Simulator` names stable while repository ownership and publication boundaries are
  moving. After the polyrepo cut is complete, run a dedicated cross-repository naming redesign around the
  `DevFixture` vocabulary. That follow-up may rename types, packages, and simulator images, but it must
  preserve the architecture: owners seed only canonical state, reaction-only projections are populated by
  production-shaped events, and standalone consumers use producer-owned lightweight event publishers rather
  than another data service's runtime. Decide the exact name mapping in that follow-up instead of spreading
  opportunistic renames across promotion PRs.

**The launch gate below is WITHDRAWN (2026-08-26.)** It is kept for the reasoning it records, but it
inverted the trade-off: the monorepo taxes every launch PR — full E2E, full checkout, full migration,
blast radius over untouched services — so the cut accelerates launch rather than delaying it. The
sequencing insight replacing it: the monorepo survives as the fallback for local development and
cross-service E2E until the final stage, so a service can be extracted before its AppHost and E2E
story is perfect.

**~~When the cut runs — gated on the launch plan.~~** Executing the cut — creating the service repos and
deleting the monorepo — does **not** begin until the entire launch plan
([`plans/launch/LAUNCH_ROADMAP.md`](../launch/LAUNCH_ROADMAP.md)) is delivered; that is months out. What ran
first was the polyrepo-*ready* corpus work (tracked by `docs/polyrepo-ready` in
[`../docs/DOCS_ROADMAP.md`](../docs/DOCS_ROADMAP.md), now **DONE** — plan and ledger deleted, git history the
archive): re-homing every rule out of the doomed nodes so the eventual repos inherit a correct corpus on day
one. `api/AGENTS.md` is one of those
nodes (its N3) and re-homes **well before** the cut — it is not itself launch-gated; only the physical
split is. N3 re-homes its content to `Concertable/agent-standards` (the shared-is-the-intersection rule into
`SERVICE_BOUNDARIES.md`; every other section was already skill-owned) and deletes `api/AGENTS.md` +
`api/CLAUDE.md`; the backend floor is thereafter the `.agents/skill-routes.json` routes over the `dotnet` plugin.

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

- **2026-08-31 — Defer seed vocabulary redesign until after the cut.** The current behaviour remains
  load-bearing, but the `SeedCatalog`/`Seed.Contracts`/`SeedState`/`Seed.Simulator` terminology will be
  redesigned around `DevFixture` once all repository and publication boundaries are stable. Promotion
  streams must not mix that naming migration into the polyrepo cut.
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
  for the entire launch plan to ship. The polyrepo-ready corpus re-home (incl. `api/AGENTS.md` N3) was the
  prerequisite that ran first and is not launch-gated.
- **2026-08-24 — polyrepo-ready corpus re-home DONE.** All nodes shipped and N8 proved a carved service stands
  alone; its plan/ledger are deleted (tracked done as `docs/polyrepo-ready`). The only residual is §4c
  plans-locality — relocating the plan *documents* — which is cut-time work owned here, still gated on the §6
  colocation sub-decision below.
- **2026-08-27 — §6 stages 1–2 delivered; §5 retired.** The test-tier package boundary is complete: 45 → 0
  cross-repository `ProjectReference`s in the test tier, `*.Hosting` published and consumed from the feed,
  both the in-monorepo swap-back and the carved feed-restore proven on the same commit. §5 closed by
  deletion — the six mirrors no longer exist, so `mirror.yml`, `mirror-parity.yml` and `POLYREPO.md` are
  gone. Extraction is unblocked: `blockingRuntimeEdges` is 1 repo-wide and all five carve gates are green.
- **2026-08-27 — the launch-gate trigger is deleted, not archived.** `LAUNCH_ROADMAP` §8b carried a
  two-condition trigger (a codebase milestone AND a second engineer owning a service) whose core claim was
  that with one developer the monorepo is *strictly better*. Tommy's ruling: the monorepo has cost real
  development time — repeated setbacks, not a theoretical tax — so that claim is refuted by delivery and
  the trigger is removed rather than kept for reference. It also rested on dead facts: the mirrors are gone,
  so "make the mirror writable" is not the mechanism. §8b is now a pointer to §6 and this ruling.
