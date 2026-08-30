# Code review — Plan/RepoSplit-Stage3-Hosting-rt2

**Review status:** `complete`
**Reviewed up to commit:** `working tree` `(2026-08-30)`
**Judgment:** `approved`

## Review pass — 2026-08-30 — self-review

**Candidate base:** `037a9ecc0`
**Candidate scope:** `all`
**Candidate branch:** `Plan/RepoSplit-Stage3-Hosting-rt2`
**Reviewer independence:** This is a self-review; no independent second pass was performed.

The shared `AddContainerImage` primitive forwards the repository and digest separately to Aspire's
three-argument `AddContainer` API, preserving immutable digest pinning without parsing a combined
reference. Each requested deployable composition method has one image-mode overload; the service
references, waits, environment variables, and optional secrets match its project-mode counterpart.
`AddSecrets`, Auth SPA configuration, and B2B SPA CORS configuration now retain the concrete resource
type so they apply to containers as well as projects.

Search and Frontend Hosting opt into packing with the same one-line property used by the other hosting
projects. No AppHost composition file, project reference, architecture-test carve job, or unrelated Auth
persisted-grant behavior changed.

### Verification

- Focused builds of AppHost.Shared, Auth.Hosting, B2B.Hosting, Customer.Hosting, Payment.Hosting,
  Search.Hosting, and Frontend.Hosting completed with 0 errors after restore from the live feed.
- `python eng/repository-split/inventory.py --check` completed successfully.
- `git diff --check` completed successfully.

### Findings

No findings.

## Incremental review — 2026-08-30 — CI remediation

The first CI run found generated inventory drift from the two newly packable projects and a local-platform
pack failure: hosting projects restored the released AppHost.Shared package, which lacks the new primitive.
The inventory is regenerated. `local-platform prepare` now enables the existing source-swap mechanism for
its own restore/pack cycle and maps AppHost.Shared, leaving ordinary consumers and carves on package
references. No AppHost composition changes are introduced.

No findings.
