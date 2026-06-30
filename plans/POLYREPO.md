# Polyrepo mirroring

The monorepo (`Concertable/concertable`) is the single source of truth. You only
ever work and commit here. Each backend service is **automatically mirrored** into
its own standalone GitHub repo so the services are also browsable as separate repos.

Mirrors are **read-only outputs** — never edit, push to, or develop in them. They are
regenerated on every push to `master`. Nothing flows back from a mirror into the monorepo.

| Source folder              | Mirror repo                      |
| -------------------------- | -------------------------------- |
| `api/Concertable.B2B`      | `thomasseery/concertable-b2b`      |
| `api/Concertable.Customer` | `thomasseery/concertable-customer` |
| _(future, Rust)_ `api/Concertable.Contract` | `thomasseery/concertable-contracts` |

## How it works

`.github/workflows/mirror.yml` runs on every push to `master`. For each service it runs
`git subtree split --prefix=<folder>`, which produces a commit history containing only the
commits that touched that folder (real dates, authors, and messages preserved), then
force-pushes that history to the mirror repo's `master` branch.

`git subtree split` recomputes the history from scratch each run. That is fine at the
current repo size. If it ever gets slow, swap the split step for
[`splitsh/lite`](https://github.com/splitsh/lite), which caches incrementally — same output,
much faster. The rest of the workflow stays the same.

## One-time setup

1. **Create the empty mirror repos** on GitHub (no README/license — the push provides
   everything): `concertable-b2b`, `concertable-customer`.
2. **Create a Personal Access Token** that can push to those repos:
   - Fine-grained PAT scoped to the mirror repos, **Contents: Read and write**, or a classic
     PAT with `repo` scope.
3. **Add it as a secret** on the monorepo: Settings → Secrets and variables → Actions →
   New repository secret, named `MIRROR_PAT`.
4. **Set each mirror's default branch to `master`** (the workflow pushes `master`), or change
   the push target in `mirror.yml` to `main`.
5. Trigger the first run: Actions → "Mirror services to standalone repos" → Run workflow
   (or just push to `master`).

## Adding a new mirror

Add an entry to the `matrix.include` list in `mirror.yml`:

```yaml
- prefix: api/Concertable.Contract
  repo: thomasseery/concertable-contracts
```

…then create the empty target repo and ensure `MIRROR_PAT` can push to it.

---

# Making the mirrors clone-and-build → see `plans/POLYREPO_COMPLETION.md`

This file documents the **live, browsable** read-only mirror system above. The larger effort —
making each mirror **clone-and-`dotnet build` on its own**, then (optionally) cutting over to a
**true polyrepo** where each service is independently developed — is staged in
[`POLYREPO_COMPLETION.md`](./POLYREPO_COMPLETION.md).

Most of the original "deferred clone-and-build" work (central package management, packable
projects, rewiring cross-folder `ProjectReference`s → feed `PackageReference`s, slim per-service
AppHosts, carve CI gates) was **already delivered by the Service Build Separation effort** — see
`api/ARCHITECTURE.md` "Cross-service contract distribution". `POLYREPO_COMPLETION.md` covers only
what genuinely remains: frictionless cross-repo feed restore, standalone AppHosts for the last
services, a shared-platform mirror, and the deferred per-service cut to true polyrepo.
