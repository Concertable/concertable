# Concertable — cross-cutting technical debt

Debt spanning multiple services or living in shared code (`Shared/`, `Concertable.Messaging`, host `Program.cs` files). Service-specific debt belongs in that service's own `TECH_DEBT.md`. When an item is fixed, update both this file and [`ARCHITECTURE.md`](./ARCHITECTURE.md).

---

## MED

### Kernel `ClaimsPrincipal.GetId()` fails open with `string.Empty`

`Shared/Concertable.Kernel/Identity/ClaimsPrincipalExtensions.cs` returns `user?.FindFirst("sub")?.Value ?? string.Empty` — a principal with no `sub` claim becomes an empty-string user id instead of a failure. Its sibling `CurrentUserExtensions.GetId(ICurrentUser)` gets this right (throws `UnauthorizedAccessException`). The only consumer, `NotificationHub`, assigns the result to `string?` and null-checks it — a check that can never fire because the method never returns null, so an unauthenticated principal sails through as `""`.

**Resolves when:** the extension fails closed (returns `string?` with no empty-string coercion, or throws like its `ICurrentUser` sibling), and `NotificationHub`'s guard actually rejects principals without a `sub` claim.

---

### Required config bound with `?? ""` — services boot misconfigured and fail later

Eight hosts coalesce required auth/bus settings to empty string at bind time: `Auth:Authority` / `ServiceAuth:*` `ClientId`+`ClientSecret` in `Concertable.Auth/Program.cs`, `Concertable.B2B.Web/Program.cs`, `Concertable.B2B.Workers/ServiceCollectionExtensions.cs`, `Concertable.Customer.Web/Program.cs`; the ASB `ConnectionString` additionally in `Concertable.Payment.Web`, `Concertable.Payment.Workers`, `Concertable.Search.Workers`, and `Concertable.B2B.Seed.Simulator` `Program.cs`. A missing setting silently becomes `""`, the host starts cleanly, and the failure surfaces later as a confusing auth/bus error instead of at startup. `Concertable.Messaging.AzureServiceBus/Options/AzureServiceBusOptions.cs` compounds it with `= ""` property defaults where the convention (`docs/CODE_CONVENTIONS.md`) requires `null!` for binder-populated values. All of these also use the banned `""` literal.

**Resolves when:** required settings fail fast at startup — options validation with `ValidateOnStart` (or an explicit throw on missing key) replaces every `?? ""`, and `AzureServiceBusOptions` defaults become `null!`. Genuine optional-with-empty-default settings, if any, keep an explicit `string.Empty`.

---

### Auth builds against a pinned shared-platform package while the rest of the solution builds from source

`api/Concertable.Auth/Directory.Packages.props` pins the shared platform to `ConcertablePlatformVersion` (currently `0.1.0-alpha.0.526`), so in the full `Concertable.slnx` build Auth compiles against that *published* package while B2B/Customer/Search build the same shared projects from live source. Edit shared source without re-publishing + bumping the pin and Auth silently compiles against stale code; a breaking shared-API change turns only the Auth build red with a confusing "works in source, fails as package" error. Accepted build-separation tradeoff for now (Auth.Contracts has ~0 churn and the shared platform changes infrequently), but the divergence is real the moment shared code moves without a publish.

**Resolves when:** the SERVICE_BUILD_SEPARATION hybrid inner-loop toggle lands (`ProjectReference` for local multi-service dev, `PackageReference` in CI/standalone), or the platform-version pin is automated so it can't lag a shared-source change.

---

## LOW

### CI feed restore assumes a same-repo `GITHUB_TOKEN` — fork / Dependabot PRs can't read the org feed

`.github/workflows/test.yml` authenticates the GitHub Packages feed with `secrets.GITHUB_TOKEN` in the `build`, `carve-auth`, and merge-queue E2E jobs. A PR opened from a **fork** (or a Dependabot PR) runs with a read-only token scoped to the fork, which cannot read the `Concertable` org's private packages, so those PRs would 401 at restore regardless of the change. Not a problem for the current same-repo branch + merge-queue workflow (no fork PRs), logged in case the repo is ever opened to external contributors.

**Resolves when:** the org packages are made internal-visible to the org's repos, or fork PRs are given a `read:packages` PAT (or simply aren't accepted).
