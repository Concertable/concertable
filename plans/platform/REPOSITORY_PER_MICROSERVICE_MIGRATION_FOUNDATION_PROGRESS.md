# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Plan-RepoSplit-6B-Topology`
- Branch: `Plan/RepoSplit-6B-Topology`
- PR: not opened
- Dependency/package gates: package inventory and ACLs require a credential with `read:packages`; private-repository
  merge-queue rulesets are unavailable on the current GitHub entitlement.
- Last reconciled: 2026-09-04 — live repository, policy, and extraction-map preflights after checkpoint 6A.

## Current state

Checkpoint 6A is terminal: `.github` PRs #1 and #2 merged, all eleven reusable workflows passed from the
public fixture, and shared policy was applied and read back. Checkpoint 6B is active. Existing private
`auth`, `b2b`, `customer`, `payment`, `search`, `infra`, and `config` repositories are legacy/bootstrap
inputs under final names; they must be preserved and reconciled, never overwritten. Missing targets are
`platform-dotnet`, `platform-web`, and `system`.

## Next Steps

- Obtain a package-read credential, then record package ACLs and repository linkage without reading secret values.
- Prove and record the reviewed branch-protection substitute on a private target while private-repository
  merge-queue rulesets remain unavailable.
- Preserve the seven legacy final-name staging repositories by renaming each to its dated
  `<name>-staging-archive-<date>` identity, then create the ten fresh private `*-next` targets. The exact
  rename set is `auth`, `b2b`, `customer`, `payment`, `search`, `infra`, and `config`; no force-push or
  repository overwrite is allowed.
- Reconcile the existing `config` bootstrap Terraform into the sole `infra` ownership boundary before the
  filtered-history handoff. Then resolve all 70 extraction-map claims and generate the 6C audit reports.

## Completed work

- Checkpoint 6A closed through `.github` PR #1 (`ab2a127cdba9bacd73411fba8cca2b6a20fc02c0`) and policy repair
  PR #2 (`a2f574a1f4fad3df5e3ec8aa0dd552d717c95728`); fixture acceptance run 33894314188 passed.
- Live 6B inventory established the eleven-repository topology using `infra` and `config` names.
- Extraction-map preflight found no duplicate claims but 70 unclaimed tracked paths; 6C is not ready.

## Verification

- `eng/repository-split/validate_map.py` currently fails only on the 70 unclaimed paths; no duplicate claims.
- No open red generated platform-sync PR at branch creation.

## Reviews

Not started for this planning candidate.

## Decisions, discoveries, blockers, and deviations

- Do not create `configuration-next` or `infrastructure-next`: live `config` and `infra` are established
  bootstrap repositories and duplicating their identities would destroy the cutover's recoverability.
- The current GitHub entitlement returns 403 for private-repository ruleset/merge-queue reads. 6B will use
  a reviewed branch-protection substitute until that entitlement changes; it must be proven and recorded
  before a target is treated as protected.
