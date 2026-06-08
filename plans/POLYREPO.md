# Polyrepo mirroring

The monorepo (`thomasseery/Concertable`) is the single source of truth. You only
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

# Deferred: make the mirrors clone-and-build (nuget.org)

The mirrors above are **browsable** but not independently buildable: each service's
`.csproj`/`.slnx` files reference projects that live outside its folder, so a cloned mirror
has dangling project references. Making a mirror `dotnet build` / run on its own is a separate,
larger project — do it on a branch, where a broken build doesn't matter. This section is the
plan for when you pick it up.

## The dependency picture

Both `Concertable.B2B` and `Concertable.Customer` reach outside their folders for two tiers:

**Tier 1 — true shared libraries** (no service identity → publish as packages):
`Concertable.Kernel`, `Concertable.Contracts`, `Concertable.ServiceDefaults`,
`Concertable.AppHost.Shared`, `Concertable.Messaging.*`, `Concertable.Seed.Shared`, and
`Concertable.Shared.{Api,Blob,Email,Geocoding,Imaging,Notification,Pdf}` (Application +
Infrastructure each).

**Tier 2 — sibling services** (`Concertable.Auth`, `Concertable.Payment`, `Concertable.Search`),
referenced two different ways:
- their `*.Contracts` projects — real library coupling → publish as packages (each service
  owns and publishes its own contracts package).
- their `*.Web` / `*.AppHost.Extensions` — referenced **only** so the AppHost can orchestrate
  them in local dev. A NuGet package is a library; you cannot launch an executable service from
  a `.nupkg`. This is the part that does **not** translate — see "The AppHost problem".

## Package hosting decision: nuget.org

Publish the shared + contract packages to the public **nuget.org** feed.
- Zero friction for anyone cloning a mirror — `dotnet build` just works, no token, no
  `nuget.config`.
- Public packages under your name are a genuine portfolio plus.
- Cost: one nuget.org API key stored as a CI secret. (Rejected alternatives: GitHub Packages
  requires a PAT to restore even for public repos — 401s for a cloning interviewer; a local
  `.nupkg` feed committed into each repo is bulletproof but looks odd to a reviewer.)

## The AppHost problem (the real design call)

A standalone mirror cannot reproduce the full multi-service Aspire orchestration, because that
launches sibling services as executables. The correct microservice story is instead:

- The **full orchestration AppHost stays monorepo-only** (boots every service together).
- Each mirror ships a **slim per-service AppHost** that boots only that service + its own infra
  (SQL, Azure Service Bus emulator), with siblings either not run or pointed at config URLs.
  This is the more honest "this service runs independently against its dependencies' contracts"
  demonstration anyway.

## Conversion steps (when picked up)

1. Add `Directory.Packages.props` (central package management) + a `nuget.org` API key secret.
2. Make Tier 1 + every `*.Contracts` project packable (`IsPackable`, `PackageId`, versioning).
   Honour the existing module-visibility rules — only intentionally public surface ships.
3. Rewire each service's cross-folder `ProjectReference`s → `PackageReference`s. In the monorepo
   they stay project references; the **mirror split step** rewrites them to package references
   (e.g. a transform, or service-local `.slnx` files that the mirror uses).
4. Add a slim per-service AppHost in each service folder for standalone run.
5. CI: a pack/publish job pushes shared + contract packages to nuget.org on version bump.
6. Verify by cloning a mirror into a clean checkout and running `dotnet build` with no monorepo
   present.

> Reassess whether this is worth it first: interviewers overwhelmingly *browse* a candidate's
> GitHub rather than clone-and-build it. The read-only mirrors already deliver the
> separated-repos story. Spend the days on this only if you want the clone-and-build capability
> and/or the NuGet packaging experience itself.
