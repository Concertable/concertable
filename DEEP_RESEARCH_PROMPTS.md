# Concertable — specific deep-research prompts

> Generic how-to + template: [`DEEP_RESEARCH_PROMPT_GUIDE.md`](DEEP_RESEARCH_PROMPT_GUIDE.md).
> This file holds the *filled-in* prompts for the actual open questions. Paste a block after `/deep-research`.
> Working doc — edit freely; delete a prompt once its run has landed.

**Settled, NOT up for research:** the USP is *"GigXchange + contract options, not just flat-fee"* —
typed revenue-share contracts (door split / versus / venue hire) that auto-settle via Stripe Connect.
Competitors do flat-fee contracts only. That's decided. The open problem is **ticket distribution**.

---

> **Prompt 1 (ticket distribution)** — *ran 2026-06-22, landed in `plans/b2b/LAUNCH_PLAN.md` §9 + decision log.*
> Outcome: Ticket Tailor is the one external ticketer with create-API + sales-webhooks + organiser-keeps-money,
> but funds route to the *organiser's* Stripe — so option (A) only gives fund control if Concertable is the
> connected account (≡ own marketplace). Launch = B own marketplace + C manual fallback; A is post-launch
> data-ingestion only. Prompt deleted per the working-doc rule.

> **Prompt 2 (production deployment of the Aspire app)** — *ran 2026-07-17, landed in
> `plans/CONFIG_AND_DEPLOYMENT.md` "Phase 0 outcome".* Outcome: deployment target = Azure Container Apps
> via `azd` (auto-detects AppHost, no `azure.yaml`/Dockerfiles); emulator→managed swap is a publish-time
> no-op via `RunAsEmulator()`/`RunAsContainer()`; EF migrations = bundles/idempotent scripts as a separate
> per-DB deploy job (never runtime `Migrate()`); SPAs = Azure Static Web Apps; CD = `azd pipeline config`.
> Four gaps left open (cost/effort, SWA-vs-container + per-env config, Key-Vault-vs-ACA-secrets wiring,
> multi-DB migration ordering) are recorded in the plan and may warrant a targeted follow-up run. Prompt
> deleted per the working-doc rule.

---

> **Prompt 3 (idiomatic config + connection-string architecture)** — *ran 2026-07-17, landed in
> `plans/CONFIG_AND_DEPLOYMENT.md` "Recommended architecture — runtime + design-time + secrets + prod
> source (Prompt 3 outcome)".* Landing-spot correction: the prompt named `CONFIG_STRATEGY.md`, but that
> doc is now *region*-scoped and delegates app-wide config to `CONFIG_AND_DEPLOYMENT.md` (where Prompt 2
> also landed), so the architecture lives there. Outcome: the Aspire AppHost already **is** the
> transfer; Aspire injects the string DbUp couldn't). Runtime = keep `GetConnectionString(<const>)` +
> `EnrichSqlServerDbContext<T>()` (pooling forbids the `AddSqlServerDbContext` one-liner — non-options
> ctors). Design-time = per-closure base `IDesignTimeDbContextFactory` that resolves from config by the
> name constant and **throws** (no hard-coded fallback); authoring never connects, applying is Aspire
> migration bundles/jobs sharing `WithReference(db)`. Secrets = Aspire parameters (local user-secrets) →
> App Config + Key Vault (prod) via run-vs-publish. Rated BEATS the bar at runtime (no abstraction to
> own), MEETS at design-time with one flagged residual (Aspire isn't running during `dotnet ef`), MEETS
> for secrets/prod. Prompt kept below for now; delete per the working-doc rule once digested.

<details>
<summary>Prompt 3 — original text (ran, kept for reference)</summary>

## Prompt 3 — Idiomatic config + connection-string architecture (EF Core on Aspire, multi-service, multi-tenant)

**Why:** the design-time connection-string handling (a static helper that fell back to a hard-coded
`Server=localhost…Password11!…` literal) is a confirmed smell (MS EF docs: *"the primary code smell to
avoid is having the connection string hard-coded"*). Removing the literal is easy; the real question is
the *whole* config story — runtime + design-time + secrets + the future prod config source — done once,
only its *runtime* provider idea transfers — its migration cleanliness does not).

**Landing spot:** `plans/CONFIG_STRATEGY.md` (exists) — merge with/supersede as appropriate; cross-link
`plans/CONFIG_AND_DEPLOYMENT.md`.

```
CONTEXT: Concertable is a .NET 10 Aspire microservices app — services B2B, Customer, Auth, Search,
Payment (each Web + Workers). Each service is a modular monolith with MANY EF Core DbContexts (one per
module) over SQL Server, and MUST stay independently ownable (a "standalone carve" — shared code is the
intersection only, never a cross-service escape). Multi-tenancy is SHARED-DB-PER-SERVICE with a scoped
`ITenantContext` + EF global query filters — NOT connection-per-tenant. Connection strings today:
- RUNTIME: the Aspire AppHost injects `ConnectionStrings__<Db>` env vars; services read them via
  `configuration.GetConnectionString(<Name>)` in each module's `AddXModule(IConfiguration)`.
- DESIGN-TIME (`dotnet ef` via `api/initial-migrations.ps1`, which nukes + re-scaffolds InitialCreate —
  migrations are dev-destructive, there is NO prod data): each module DbContext has an
  `IDesignTimeDbContextFactory` that stubs the scoped `ITenantContext` (`DesignTimeTenantContext`),
  required because the ctor takes deps not in design-time DI (the MS-documented trigger for a factory).
  Its connection string was a static `DesignTimeConnectionString` helper with a hard-coded fallback.
- SECRETS: local secrets live in gitignored `appsettings.Development.json` (history-only in git); a
  proper secrets story is unresolved (see CONFIG_AND_DEPLOYMENT Phase 2).
- SEAM ALREADY BUILT (this PR): `ServiceDefaults` embeds `SharedDefaults/appsettings.json` and layers it
  lowest-precedence via `AddServiceDefaults` — the intended home for shared config defaults.
PROD target (already researched): Azure Container Apps via `azd`; EF migrations as separate per-DB
idempotent-script/bundle deploy jobs (never runtime Migrate()).

DECISION: the cleanest end-to-end configuration + connection-string architecture — runtime AND
design-time AND secrets AND the prod config source — that has ONE environment-driven source of truth,
zero hard-coded values/secrets in code, a call-site as clean as an injected provider, and no per-service
boundary violation.

QUESTIONS (cited; prefer official .NET Aspire / EF Core / Azure docs, recency 2025–2026):
1. Idiomatic runtime connection-string resolution in Aspire multi-service apps: is a custom
   `IConnectionStringProvider`-style abstraction worth it over `GetConnectionString`, and when? What do
   Aspire's own conventions (`AddSqlServerDbContext`, `AddSqlServerClient`, connection-name matching)
   prescribe for many-DbContexts-per-service?
2. Confirm shared-DB-per-service + query-filter multi-tenancy needs NO connection-string provider
   apply here. If wrong, say why.
3. Design-time DbContext creation given the `ITenantContext` stub requirement: is
   `IDesignTimeDbContextFactory` the right/only idiom, or is there a cleaner Aspire/EF pattern (migration
   bundles, a dedicated migrations host project, EF host-based resolution)? How to feed its connection
   string from the SAME central source so the `initial-migrations.ps1` env-var step disappears — with no
   hard-coded fallback and no magic strings (constants only)?
4. Centralized prod config: Azure App Configuration + Key Vault references, per-environment — how values
   flow to ACA services AND to the design-time/migration jobs, while keeping local dev (Aspire
   emulators) unchanged (run-vs-publish branching).
5. Local-dev secrets to replace gitignored `appsettings.Development.json`: .NET user secrets vs Aspire
   parameters (`AddParameter`/`AddConnectionString`) vs Key Vault — the idiomatic Aspire local story.
6. DRY across ~19 DbContexts × 5 services WITHOUT breaking the standalone carve: shared design-time
   helper vs inline-per-factory vs a small shared design-time-config package — what's idiomatic?

DELIVERABLE: a concrete recommended architecture (runtime + design-time + secrets + prod source) with
the exact code shape for (a) runtime registration, (b) the design-time factory's connection-string
resolution (no fallback, constants only), (c) where each connection string/secret lives per environment,
(d) a migration path from today's state. Explicitly rate the result against the "as clean as an injected
IConnectionStringProvider" bar, and flag anything that can't reach it and why.
```

</details>
