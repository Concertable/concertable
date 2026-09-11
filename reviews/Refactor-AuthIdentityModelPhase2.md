# Code review — Refactor/AuthIdentityModelPhase2

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `999950ae4baae794a94f990894382469d986e8d3`  `(2026-09-11)`
**Security-reviewed up to commit:** `999950ae4baae794a94f990894382469d986e8d3`  `(2026-09-11)`
**Judgment:** `approved`

## Review pass — 2026-09-11 — full

**Candidate base:** `10ff986516bef1cb70e9f4fd0413dd9402e592bd`
**Candidate head:** `f9a8556135a930a44e2ed2e999e7d7985960c690`
**Candidate branch:** `Refactor/AuthIdentityModelPhase2`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:288c8c86538a7859168993ae46ff708964ddabac03e3cd2dbf3e1be2ce2f3b33` `(54 paths)`
**Candidate bundle:** `C:/Users/tommy/AppData/Local/Temp/claude/C--Users-tommy-source-repos-Concertable/561bbdf2-44cd-4a05-a4cb-5c430bbf2b8b/scratchpad/review-bundle-AuthIdentityModelPhase2`
**Candidate bundle identity:** `sha256:c5ffa8e104cad2c58cb28a1183da956963253fd199009a226d2673df5a2633c7`
**Work-order path:** `reviews/Refactor-AuthIdentityModelPhase2.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Native general layer, a module-boundaries/domain-events lens, and a security-focused pass (required —
this diff touches `Concertable.Auth`, `Concertable.Payment`, and `.Contracts` paths per
`.agents/merge-gate.json`) all ran clean. Parent independently verified the highest-risk substitutions
directly: `AuthScopes.cs`/`AuthResources.cs`/`InteractiveClients.cs` wire values against the deleted
`ClientIds`/`ApiScopeIds`/old `Config.cs` literals (exact match, no swapped enum member), and
`ManagerClients.cs`'s `InteractiveClient → TenantType?` table against the prior classification semantics
(Venue/Artist browser+mobile correct, Admin manager-with-no-tenant-type correct, unlisted clients default
safely to `false`/`null`).

### Findings

No findings.

## Incremental passes — `f9a8556..999950ae4`

The recorded pass above was stamped at `f9a8556` and marked approved. Payment's architecture suite was red
at that point and had not been run; the ledger's "all suites re-verified green" claim did not cover it.
Three commits follow, each driven by a review finding rather than by further design.

### Pass 2 — `fc21947ca`, the published-package boundary

**Pass judgment:** `changes-requested` — one defect confirmed, fixed in this same commit.

The typed-scope migration added `Concertable.Auth.Contracts` to `Concertable.Payment.Client`, a published
package, with `PrivateAssets="all"` and a comment stating that keeps the dependency out of the package.
It does not. `PrivateAssets` suppresses the nuspec dependency and leaves the assembly reference in the
compiled output, so a consumer of `Concertable.Payment.Client` that does not independently carry
Auth.Contracts faults at runtime when the call-credentials delegate is first JITed. An undeclared hard
dependency is worse than a declared one.

The repository already proved this empirically before the fix: `PaymentContractReferenceTests`'
`PublishedAssemblies_ReferenceOnlySharedPaymentDependencies` reads the real IL `AssemblyRef` table via
`Assembly.GetReferencedAssemblies()`, which `PrivateAssets` cannot hide, and it was one of the two failing
tests. The failing suite was the evidence, not an obstacle to route around.

Fixed by dropping the reference and taking the scope from `PaymentScopes.Write` in the Payment-owned
`Concertable.Payment.Contracts`, with `PaymentScopeParityTests` holding it equal to
`AuthScope.PaymentWrite.Id()`. `Concertable.Payment.Web` keeps the reference: a non-packable resource
server legitimately names the audience it validates, as `B2B.Hosting` and `Customer.Hosting` already do.

### Pass 3 — `953581d34`, findings against pass 2's own fix

**Pass judgment:** `changes-requested` — both findings addressed or logged.

- **Fixed.** The host allowance keyed on the filename suffix `.Web` alone, which would have granted a
  future *packable* `Concertable.*.Web` library exactly the permission this check exists to deny —
  reproducing the defect pass 2 fixed. It now requires the suffix and the absence of `IsPackable=true`,
  read from the csproj the check already parses. Verified by mutation: declaring `Payment.Web` packable
  fails the check.
- **Logged, not fixed.** `PaymentScopeParityTests` guards only one direction. `test.yml`'s
  `SERVICE_DIRS='^api/Concertable\.(Auth|B2B|Customer|Payment|Search)([./]|$)'` admits `.` in its trailing
  class, so `api/Concertable.Auth.Contracts/` classifies as the `Auth` service's own folder and a PR
  touching only it runs Auth's suites and none of its four consumers'. Confirmed by piping the path
  through the shipped regex and `sed`, not by reading them. An Auth-side rename of the wire string
  therefore merges green. This is a repo-wide gate hole, logged HIGH in the root `TECH_DEBT.md` whose
  stated remit is `.github/workflows/**` gating logic. Not fixed here: editing `test.yml` forces every
  service into scope and discards a green 89-check run on a PR whose subject is the identity model.

Payment's `TECH_DEBT.md` carries the duplicated literal with its one-directional guard, and under it the
design finding that the scope/audience registry sits in one service's contracts while four services need
it. Each entry has a condition that deletes it; the second removes the first.

### Pass 4 — `999950ae4`, plan close-out, docs only

**Pass judgment:** `approved`.

The roadmap keeps the epic open on `auth-identity-model/extension-block-syntax`, so the roadmap survives
and only the plan and ledger are deleted, against the plan's own stale instruction to delete the
directory. The surviving item absorbed the reasoning it used to cite in the deleted ledger — why the
producer and consumer halves cannot land in one PR — and was corrected to name `Concertable/auth` as its
home, since the monorepo no longer publishes those ids. `plan_graph.py`: 0 errors, 0 warnings.

### Security layer — `f9a8556..999950ae4`

**No findings.** An independent security lens verified, and the parent re-verified directly:

- All three copies of the scope literal resolve to `payment:write` — `PaymentScopes.cs:5`,
  `AuthScopes.cs:13`, and the `ServiceToken` policy at `HostExtensions.cs:109`.
- `HostExtensions.cs` is unchanged across the whole delta (zero diff lines), so `ValidAudiences`,
  `ValidateIssuer` and `ClockSkew` are untouched.
- The Auth-side registry (`AuthScope`, `AuthScopes`, `AuthResources`) is untouched by these three commits.
- The call-site change is a literal-for-literal swap behind an unchanged `ITokenService` signature, with
  no swallowed exception and no default-scope path — `PaymentScopes.Write` is a `const`.
- A scope identifier is not a secret, and the same literal shipped inside `Payment.Client` before this
  delta as an inline string.
- The CI-scoping hole documented in `TECH_DEBT.md` is **fail-closed** — its symptom is valid
  service-to-service calls being rejected, not an authorization bypass — so disclosing the exact regex
  and its fix is appropriate for a repository about to be made public.

Security marker stamped at `999950ae4` on that basis.
