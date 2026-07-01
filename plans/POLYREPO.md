# Polyrepo mirroring

The monorepo (`Concertable/concertable`) is the single source of truth. You only
ever work and commit here. Each backend service is **automatically mirrored** into
its own standalone GitHub repo so the services are also browsable as separate repos.

Mirrors are **read-only outputs** — never edit, push to, or develop in them. They are
regenerated on every push to `master`. Nothing flows back from a mirror into the monorepo.

| Source folder              | Mirror repo                      |
| -------------------------- | -------------------------------- |
| `api/Concertable.B2B`      | `Concertable/concertable-b2b`      |
| `api/Concertable.Customer` | `Concertable/concertable-customer` |
| `api/Concertable.Auth`     | `Concertable/concertable-auth`     |
| `api/Concertable.Payment`  | `Concertable/concertable-payment`  |
| `api/Concertable.Search`   | `Concertable/concertable-search`   |
| `api/Shared`               | `Concertable/concertable-shared`   |

## How it works

`.github/workflows/mirror.yml` runs on every push to `master`. For each service it runs
`git subtree split --prefix=<folder>`, which produces a commit history containing only the
commits that touched that folder (real dates, authors, and messages preserved), then
force-pushes that history to the mirror repo's `master` branch.

`git subtree split` recomputes the history from scratch each run. That is fine at the
current repo size. If it ever gets slow, swap the split step for
[`splitsh/lite`](https://github.com/splitsh/lite), which caches incrementally — same output,
much faster. The rest of the workflow stays the same.

## One-time setup — **DONE** (recorded here as the runbook for adding future orgs/mirrors)

All six mirror repos exist, `MIRROR_PAT` is set, and auto-sync on `master` is green. The steps that
got it there, for reference / re-use:

1. **Create the empty mirror repos** on GitHub (no README/license — the push provides everything).
   The six live: `concertable-{b2b,customer,auth,payment,search,shared}` under the `Concertable` org.
2. **Create a Personal Access Token** that can push to those repos:
   - Fine-grained PAT scoped to the mirror repos, **Contents: Read and write**, or a classic
     PAT with `repo` scope.
3. **Add it as a secret** on the monorepo, named `MIRROR_PAT` (Settings → Secrets and variables →
   Actions). **Footgun:** the checkout step must set `persist-credentials: false`, or `actions/checkout`
   leaves the job `GITHUB_TOKEN` as an `extraheader` that overrides the `MIRROR_PAT` push creds and the
   cross-repo push 403s as `github-actions[bot]`.
4. **Set each mirror's default branch to `master`** (the workflow pushes `master`), or change
   the push target in `mirror.yml` to `main`.
5. Trigger the first run: Actions → "Mirror services to standalone repos" → Run workflow
   (or just push to `master`).

## Adding a new mirror

Add an entry to the `matrix.include` list in `mirror.yml`:

```yaml
- prefix: api/Concertable.Contract
  repo: Concertable/concertable-contracts
```

…then create the empty target repo and ensure `MIRROR_PAT` can push to it.

---

# Clone-and-build mirrors — **DONE**; true polyrepo → see `plans/POLYREPO_COMPLETION.md`

This file documents the **live, browsable** read-only mirror system above. Those mirrors now also
**clone-and-`dotnet build` on their own** — a standalone clone restores the deployable closure from
the private org feed with a `read:packages` PAT (`GITHUB_PACKAGES_TOKEN`) and builds with 0 errors.
That "buildable mirror" effort (Phases 1–4 of `POLYREPO_COMPLETION.md`) is complete.

Most of it was **already delivered by the Service Build Separation effort** — central package
management, packable projects, rewiring cross-folder `ProjectReference`s → feed `PackageReference`s,
carve CI gates (see `api/ARCHITECTURE.md` "Cross-service contract distribution"). Phases 1–4 added
the rest: cross-repo feed-restore docs/PAT, slim standalone AppHosts for Auth/Payment/Search, the
`concertable-shared` mirror, and the six real repos + green auto-sync.

What's **still deferred** in `POLYREPO_COMPLETION.md`: the optional one-way cut to a **true polyrepo**
where each service is independently developed. Not started — do it only when the monorepo demonstrably
holds you back.
