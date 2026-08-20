---
name: package-cutover
description: Execute a BREAKING change to a type that lives in a cross-service PUBLISHED package — a rename, a namespace/folder move, a signature change, or moving a DTO between packages — as a deterministic expand→publish→sync cut-over, instead of discovering the merge sequence by trial-and-error. Consumers compile against the published package, not source, so the change can NOT be atomic. This skill scans the package topology up front to fix the exact merge count, sequences the merges, and runs the mechanical consumer migration (global + file-local usings, EF re-scaffold, grep gate) through an automated build→fix→build loop instead of hand-iterating. Use whenever a refactor changes a type that another service consumes as a PackageReference (Concertable.Kernel / Concertable.Contracts / any *.Contracts / *.Seed.Contracts / *.Client), when a build hits `CS7069 "claims it is defined in '<pkg>' but could not be found"`, or for any "boundary-blocked" multi-merge refactor. Generic over the type and the packages — you pass it the old and new identity; it is NOT hard-coded to any one refactor.
---

# package-cutover

Turn a **breaking change to a type exposed by a published, cross-boundary package** into a
**planned, mechanical cut-over** — not the trial-and-error slog it becomes when you just start
editing and let `dotnet build` teach you the topology one failure at a time.

Read [`api/ARCHITECTURE.md`](../../../api/ARCHITECTURE.md) ("Cross-service contract distribution" +
"the publish→sync loop") and the `plans` skill ("Breaking published-contract changes") first — this skill is the *execution procedure* for exactly the situation those describe.

## When this applies (the diagnosis)

Use it when **both** are true:

1. You are changing a type's **identity**, not just its internals — a rename, a namespace/folder
   move, a changed public signature/return type, or relocating a DTO to a different package. (A
   purely *additive* change — a new method, a new optional member — is back-compat and needs none of
   this.)
2. That type is **exposed in the public API of a package that other services/projects consume as a
   `PackageReference`** (they bind the *published* assembly from the feed, not the source next to
   them). In this repo that means `Concertable.Kernel`, `Concertable.Contracts`, any module
   `*.Contracts`, `*.Seed.Contracts`, `*.Client`, etc.

**The tell at the keyboard:** a consumer fails with
`error CS7069: Reference to type 'X' claims it is defined in '<Package>', but it could not be found`.
That is *always* "the published package still carries the old shape" — never a source bug in the
consumer. Do **not** try to fix it in the consumer's source; it clears only when `<Package>`
republishes with the new shape.

**Why it can't be atomic:** there is no back-compat shim for an identity change — a type is in
exactly one place with one shape. A consumer can't adopt the new shape until the package that
carries it republishes. So it is a **republish-then-consume** cut-over across ≥2 merges.

## Step 0 — Topology scan (do this BEFORE editing anything; it sets the merge count)

The single biggest time-saver: learn the shape of the blast radius up front so you know how many
merges you're in for and what is expected-red in each. Parameterize everything by the old identity.

```bash
OLD='Genre'                       # the simple type name (adapt)
OLDNS='Concertable.Contracts'     # its OLD fully-qualified namespace / package identity (adapt)
# 1) Everywhere it's referenced:
grep -rniE "\b$OLD\b" api --include=*.cs | grep -viE "[/\\](bin|obj)[/\\]" | wc -l
# 2) Which PACKABLE projects own/expose it (these are what republish):
grep -rl "<IsPackable>true</IsPackable>" api --include=*.csproj | grep -viE "[/\\](bin|obj)[/\\]"
# 3) Which packages are consumed CROSS-BOUNDARY as PackageReference (the layers that must sync):
grep -rniE "PackageReference Include=\"Concertable\." api --include=*.csproj \
  | grep -viE "[/\\](bin|obj)[/\\]"
grep -rniE "PackageVersion Include=\"Concertable\." api --include=Directory.Packages.props
```

From that, write down — **as an explicit list** — every **distinct published package whose public
API carries the type** and who consumes each. Then:

- **Merges = 1 expand + one sync hop per package layer.** Layers that republish *together* in one
  publish collapse into one sync. A package that re-exposes the type onward to a *further* service
  (e.g. a `*.Seed.Contracts` used by two other services, or a `*.Client`) is its **own** later
  layer — that is the hop teams miss and rediscover via `CS7069`. The scan is where you catch it.
- **Owned entity / owned-type moves** (EF) add a re-scaffold step per affected service (below).

Record the plan (which merge does what, what is expected-red in each) in the feature's plan file so a
a context reset can't lose it.

## Prepare independent consumers in parallel

The publish→sync order is the delivery DAG, not automatically the implementation DAG. After the
topology scan, dispatch every consumer whose source can be migrated and verified against an exact local
producer package. Record the producer commit, package version, package hashes, and reproducible feed
location in the owning ledgers. Use temporary command-line restore inputs or temporary pins only; never
commit a machine-specific path, local-only NuGet source, or disposable version.

A prepared consumer is **delivery-ready**, not merge-ready: commit and review its real source changes,
restore all temporary inputs, and retain the published-package revalidation as its next delivery gate.
After publication and generated sync, update to the real version, rerun the full affected verification,
and only then mark it merge-ready. A consumer is implementation-blocked only when no exact artifact or
required API/design exists—not merely because the producer PR has not merged.

## The cut-over pattern (expand → publish → sync, repeated per layer)

1. **Expand merge** — change the source in the *owning* package(s) **only**; touch nothing that
   still binds the old pinned version. Everything on the old package stays green; the only red is
   projects in the same repo that mix new source with the old published package (harnesses,
   cross-service seed) — that red is **structural and expected**, not fixable here. Merge it (see
   "admin vs queue" below). The publish republishes the package with the new shape.
2. **Sync merge** — after the publish lands: bump each consumer's pinned package version to the
   just-published one, migrate consumer source to the new shape, re-scaffold if the model changed,
   grep-gate to zero. This one is **green** → normal merge queue.
3. If a consumer package **re-exposes** the type onward, its consumers are still red until *it*
   republishes → repeat expand/publish/sync for that layer. (Step 0 predicted this.)

## The mechanical migration (per sync merge) — automate, don't hand-iterate

### a) Global usings first (covers most files in one shot)
Every project with a `GlobalUsings.cs` that imports the old namespace: **add** the new namespace
alongside the old (don't remove the old — the flat/base namespace usually still holds other types):

```bash
NEWNS='Concertable.Contracts.Enums'   # adapt
grep -rlE "global using $OLDNS;" api --include=GlobalUsings.cs | grep -viE "[/\\](bin|obj)[/\\]" \
  | while read -r f; do sed -i "/global using $OLDNS;/a global using $NEWNS;" "$f"; done
```

### b) The automated build→fix→build loop (the core token-saver)
Dependent projects hide their own errors until their dependency compiles, so errors surface in
**layers** — never hand-iterate this. Loop: build, fix exactly the files in the error set, repeat.
Erroring files never already have the new using, so this can't create duplicates. Adapt the
old→new mapping and the "is the base namespace still needed?" guard to your case.

```bash
SLN=api/Concertable.slnx
BASE_TYPES='\b(IEntity|IHasName|IHasLocation|Pagination|PageParams|DomainException)\b'  # types still in OLDNS — adapt from Step 0
for i in $(seq 1 8); do
  dotnet build "$SLN" > /tmp/pc.log 2>&1
  grep -cE "error CS(0246|0103)" /tmp/pc.log | grep -q '^0$' && { echo "GREEN"; break; }
  grep -E "error CS(0246|0103)" /tmp/pc.log | sed -E 's/\(.*//;s/^[[:space:]]*//' | sort -u \
  | while read -r f; do
      [ -f "$f" ] || continue
      grep -qE "\b$OLD\b" "$f" && grep -qE "using $OLDNS;" "$f" && ! grep -qE "using $NEWNS" "$f" && {
        if grep -qE "$BASE_TYPES" "$f"; then sed -i "/using $OLDNS;/a using $NEWNS;" "$f";   # add: base ns still used
        else sed -i "s/using $OLDNS;/using $NEWNS;/" "$f"; fi                                  # replace: base ns not needed
      }
    done
done
```

For **no-`GlobalUsings` projects** (test / seed / some Api projects) this loop is what fixes them —
they carry file-local usings the global step never touched.

### c) Re-scaffold EF migrations (only if an entity / owned-type moved namespace)
The model snapshots reference the CLR namespace as a string, so a moved entity/owned-type leaves
`b.OwnsOne("<OLDNS>.X", ...)` in every snapshot. Re-scaffold — never hand-edit:
`./initial-migrations.ps1` from `api/`. (No model change = a free, lossless re-scaffold — see
`api/AGENTS.md` "Migrations".)

### d) Grep gate = definition of done
The old identity must grep to **zero** (the only allowed survivors are generated migration
snapshots *between* the move and the re-scaffold):

```bash
grep -rniE "$OLDNS\.$OLD\b" api --include=*.cs | grep -viE "[/\\](bin|obj)[/\\]|$NEWNS"
```

## Gotchas that cost real time (baked in so you don't rediscover them)

- **The published version must ADVANCE.** Publish uses `--skip-duplicate`; if the version doesn't
  bump, the *old* package stays on the feed and consumers never see the new shape. Confirm the
  publish pushed a new version before bumping consumers to it.
- **Don't put source changes on the auto-sync branch.** If this repo auto-opens a platform-sync /
  version-bump PR, do the consumer migration on your **own** branch, not by piggybacking the sync
  branch — a cascade guard treats a commit on the sync branch as "just a pin bump" and **suppresses
  the next auto-sync PR**, forcing you to bump by hand. Own branch → the bot opens the next hop for
  free.
- **Admin-merge only the *expand* merges** (they carry the structural expected-red). *Sync* merges
  build green → normal merge queue, no admin.
- **Errors surface in layers** — a green build after one fix pass doesn't mean done if a whole
  service was skipped behind a failed dependency; keep looping until the *full* solution builds.
- **Keep the logs quiet** — parse only `error CS` lines from the build; a raw solution build log and
  `git`'s per-file CRLF warnings are pure noise that burn context.

## Verification (every merge)

- `dotnet build api/Concertable.slnx` — 0 errors (for a sync merge; an expand merge is green except
  the documented structural red).
- Old-identity grep gate = 0 (allowlist only in-flight migration snapshots).
- Affected unit + integration tests via `integration-debug`; a red run → drive that skill, don't
  report red. E2E only if the change is behaviourally risky (a pure move/rename is not — see
  `plans/AGENTS.md` "Merge-queue E2E tier").
- Re-scaffold done where the model moved.

## Scope guard

Before reporting any completed, blocked, published, merged, or synchronized cut-over stage, if the
work is plan-managed, read and apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md).

This is for **cross-boundary published-package** identity changes. A type that is **service-internal**
(only its own service references it, cross-service contact is Contracts-only) is a plain single-PR
refactor — do it directly, no cut-over. Step 0's topology scan tells you which you have: if no
*other* service consumes the owning package, you're internal → skip all of the above.

