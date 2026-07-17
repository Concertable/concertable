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
