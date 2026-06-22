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

## Prompt 2 — Production deployment of the Aspire app

**Tweak before running:** if you're not committed to Azure, change the platform line in CONTEXT; if budget is a hard constraint, add a target monthly cost so Q5 optimises for it.

```
CONTEXT: Concertable is a .NET 10 Aspire microservices app — services B2B, Customer, Auth,
Search, Payment (each Web + Workers), backed by SQL Server, Azure Service Bus and Azure Storage,
plus 4 Vite SPAs (venue/artist/business/customer) and a mobile app. Today it ONLY runs locally
via the Aspire AppHost (SQL/Service Bus/Storage are containers/emulators). There is NO production
deployment: no Dockerfiles, no IaC, no azure.yaml, no CD pipeline (CI is build + test only).
EF Core migrations are dev-destructive — a script nukes and re-scaffolds InitialCreate each time.
I want to take this to production, likely on Azure (already on Azure Service Bus + Storage).

DECISION: the best, lowest-effort production deployment approach for this Aspire app, and the
specific gotchas to plan for.

QUESTIONS (cited, prefer official Microsoft / .NET Aspire docs; recency = 2025–2026):
1. Current best practice for deploying a .NET Aspire app to Azure Container Apps with `azd`
   (azure.yaml generation, `azd up`, container build without Dockerfiles). What does it provision,
   and what are the limits / things it won't do for you?
2. How to map Aspire's local emulated resources (SQL container, Service Bus emulator, Storage
   emulator) to real managed Azure resources in PUBLISH mode only, keeping local dev unchanged
   (run-vs-publish branching, RunAsEmulator / RunAsContainer).
3. Production EF Core migration strategy for a multi-database Aspire microservices app — migration
   bundles vs apply-on-startup vs one-off deploy jobs — to replace a dev nuke-and-rescaffold
   workflow. Safe patterns and pitfalls (zero-downtime, ordering, rollback).
4. Hosting Vite SPAs in production alongside ACA services — Azure Static Web Apps vs containerised
   — and per-environment config (API base URLs, auth redirect URIs, CORS).
5. Secrets/config (Stripe keys, JWT signing, connection strings) via Key Vault / Container Apps
   secrets with azd; setting up CD via `azd pipeline config`; and a realistic first-deploy effort
   + monthly cost ballpark for ~7 container apps + Azure SQL + Service Bus.

DELIVERABLE: a concrete, ordered path from "runs locally in Aspire" to "deployed on Azure with CD"
— the emulator→managed swap, the migration-strategy fix, the SPA hosting choice, secrets, and a
rough effort + cost estimate.
```
