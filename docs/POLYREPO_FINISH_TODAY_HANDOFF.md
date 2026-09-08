# Polyrepo finish-today handoff

This branch is the laptop continuation point for the repository-per-microservice migration. It was cut from `origin/main` at `32fc63edc93bd73022845c43d7b471648a07ae5c` after the M1 platform-expansion and package-publication fixes landed.

Do not work in the normal checkout if it is dirty. Create or reuse isolated worktrees. Do not create or import repositories. Preserve the corrective topology: `auth`, `b2b`, `customer`, `payment`, `search`, `infra`, and `config` keep their identities; shared packages split into `platform-dotnet` and `platform-frontend`; web/mobile are package tiers; `system` is the black-box container-composition boundary.

## Phase 1 — finish the monorepo cutover

First finish this phase. It is the producer and compatibility work that makes the external repository cutovers safe.

Current landed baseline:

- PR #942 (M1 Platform Expand): `8899eae3355cb3f47c4ac23acb5e2ba89b31cd62`; its merge-group UI E2E was green.
- PR #953 (package-publication collision fix): `32fc63edc93bd73022845c43d7b471648a07ae5c`; causal publish run `34203007892` published all 58 packages at `0.1.0-alpha.0.1330` and restored that exact version.
- Generated version-sync PR #954: `chore/platform-sync-0.1.0-alpha.0.1330`, head `aefafb4b84eb595de7dbe2bee505d3cfeb83ecc2`. It pins the seven consumer package files to 1330. Auto-merge is armed. Finish and record its exact landed main SHA before restacking dependent branches.

Required staged order (do not squash):

1. Restack and deliver M1 P2, PR #943, `Refactor/M1-Owner-Hosting-Sync` (remote head `cbca8d48f109dd56116ae77362d764eb90c5b661`) onto the exact main after #954. Preserve its eight staged commits. Validate package-clean B2B/Customer hosting and review before merge.
2. Let P2 publish its package/image outputs. Record its exact published packages and immutable Auth image digest.
3. Restack and deliver M1 P3, PR #944, `Refactor/M1-AppHost-Sync` (remote head `6895b13cbd1aee9edff87b9aaa0e87bea42ac80e`) onto the P2-published baseline. Pin the exact Auth digest; validate standalone AppHost composition, then review and merge.
4. Restack and deliver M1 P4, PR #945, `Refactor/M1-Platform-Contract` (remote head `64f0c4dc12f592a4654f1cc0227bf405783608dc`) after P3. Validate frontend hosting boundaries, package-clean builds and contract closure; review and merge.
5. Deliver M2 PR #947, `Refactor/RepoSplit-M2-Owner-Operations` (head `6d5cf971594192b4636e445cb67372ffa9ac2e6f`) from exact current main, after its required M1 compatibility points are landed.
6. Complete M4 only after terminal M1/M2 dependencies. Its prepared branch is `Refactor/RepoSplit-M4-Closure-Repair`, remote head `f29310f07028f1023c2799af1a6fcbf6c558b2ca`; preserve its intentional local review artifact if that existing worktree is used.

Paste this into the laptop agent for Phase 1:

```text
Continue Concertable’s repository-per-microservice migration, Phase 1 monorepo cutover, from the canonical remote state. Start in an isolated worktree, never the dirty normal checkout. Read root AGENTS.md and load concertable:plan-execution, concertable:open-worktree, concertable:git-branching, concertable:plans, concertable:plan-checkpoint, concertable:remote-validation, concertable:review, concertable:merge, dotnet:microservice-boundaries, dotnet:microservices-architecture, and dotnet:package-cutover when entering published-package boundaries. Read docs/POLYREPO_FINISH_TODAY_HANDOFF.md plus plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md, plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_FOUNDATION_PROGRESS.md, and plans/platform/POLYREPO_ROADMAP.md. Fetch origin, finish generated platform-sync PR #954 to terminal, then use the exact landed origin/main to deliver the M1 stack strictly P2 #943 -> publish/Auth digest -> P3 #944 -> P4 #945, preserving staged commits and explicit force-with-lease pushes only where a restack rewrites history. Then deliver M2 #947 and M4 in canonical dependency order. Reconcile active ledgers against corrective branch Plan/RepoSplit-6B-Corrective-Topology at 3f550ced1 (82bf5dbbb, bb59d9ba3), preserve newer user work, and run python .agents/hooks/plan_graph.py --root <absolute-worktree> after plan metadata changes. Own every remote CI, queue, publication and generated-sync gate with a background monitor. Do not create/import repositories.
```

## Phase 2 — finish the polyrepo delivery

Begin only when Phase 1 is terminal. These are independent preparations that can be validated in parallel where their package producer is already published, but their delivery order remains fixed.

- B2B frontend queue repair, PR #952, `Fix/FrontendFullE2EQueueDependencies`, head `5f6051507d1802115f3682d8814f16ec98918292`.
- B2B producer, PR #951, `Refactor/B2bPackageTopologyPhase3-Producer`, head `420345df04f7cd31e39015c92d6309cb5e0ff491`.
- B2B consumers, PR #950, `Refactor/B2bPackageTopologyPhase3-Consumers`, head `5414d4cccb3867bfff8a79988474135f04067ba1`; it follows the producer publication and must be revalidated against the real published baseline.

Use the canonical roadmap to enumerate every newly unblocked independent workstream only after M1/M2/M4 terminal gates open. Launch separate self-contained owners for eligible streams; do not cross authorization/repository-creation gates because work is parallelizable. Preserve package-publication dependencies and final service-cutover order.

Paste this into the laptop agent for Phase 2:

```text
Continue Concertable’s repository-per-microservice migration, Phase 2 polyrepo delivery, only after the monorepo cutover recorded in docs/POLYREPO_FINISH_TODAY_HANDOFF.md is terminal. Use isolated worktrees and read root AGENTS.md; load concertable:plan-execution, concertable:open-worktree, concertable:git-branching, concertable:plans, concertable:plan-checkpoint, concertable:remote-validation, concertable:review, concertable:merge, dotnet:microservice-boundaries, dotnet:microservices-architecture, and dotnet:package-cutover. Read the three authoritative platform plan records and the handoff document. Fetch remote state and reconcile the active ledger against corrective topology branch Plan/RepoSplit-6B-Corrective-Topology at 3f550ced1. Finish the B2B sequence in order: #952 queue repair, #951 producer, producer publication, then #950 consumers against the actual published baseline. Then inspect POLYREPO_ROADMAP.md and launch/return separate self-contained parallel owners for every newly unblocked independent workstream, preserving all package publication and final service cutover dependencies. Do not create/import repositories, do not touch a dirty normal checkout, use explicit pushes only, and run plan_graph after plan-metadata edits. Own all remote gates through terminal with background monitors; finish the entire authorized migration rather than stopping at prepared branches.
```
