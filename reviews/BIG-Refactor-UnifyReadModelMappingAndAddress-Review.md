# Big review — Refactor/UnifyReadModelMappingAndAddress

**Plan anchored to commit:** `605b677b20c79ee8fc5da09bb0d7ba7be4e958f4`  _(2026-06-22)_
<!-- "Reviewed up to commit:" is added ONLY when every area is [x] (Step 2 completion):
     the watermark `incremental-review` greps for. Equals the plan-anchor SHA. Omit until complete. -->
Net diff reviewed: `a3c587ab..605b677b` (160 files, +1962/-994). Move-only files skipped.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

## Coverage
<!-- dependency order, foundation first. Path globs are the literal `git diff -- <paths>` args. -->
- [x] **Shared foundation (Kernel + Contracts)** — 3 files — `api/Shared/` _(reviewed 2026-06-22)_
- [ ] **B2B service (code, non-migration/test)** — 21 files — `api/Concertable.B2B/` `:(exclude,glob)api/Concertable.B2B/**/Migrations/**` `:(exclude,glob)api/Concertable.B2B/**/Tests/**`
- [ ] **Customer service (code, non-migration/test)** — 66 files — `api/Concertable.Customer/` `:(exclude,glob)api/Concertable.Customer/**/Migrations/**` `:(exclude,glob)api/Concertable.Customer/**/Tests/**`
- [ ] **Adapters + schema/migrations (Search/Payment/Auth/Messaging + all migrations)** — 46 files — `api/Concertable.Search/` `api/Concertable.Payment/` `api/Concertable.Auth/` `api/Concertable.Messaging/` `:(glob)api/Concertable.B2B/**/Migrations/**` `:(glob)api/Concertable.Customer/**/Migrations/**`
- [ ] **Tests (B2B + Customer)** — 7 files — `:(glob)api/Concertable.B2B/**/Tests/**` `:(glob)api/Concertable.Customer/**/Tests/**`
- [ ] **Frontend + docs/plans (everything else)** — 17 files — `app/` `plans/` `api/docs/` `api/CLAUDE.md` `CLAUDE.md` `.claude/` `api/Concertable.slnx` `DEEP_RESEARCH_PROMPTS.md` `DEEP_RESEARCH_PROMPT_GUIDE.md` `MARKETPLACE_DELEGATION_RESEARCH.md` `ROLE_PARITY_INVESTIGATION_PROMPT.md`

## Cross-area notes
<!-- one-liners added during a stage for things a LATER stage must verify; struck through when resolved. -->
- **[from Shared]** `ImageDto` deleted from `Concertable.Contracts` (commit 24357e7b). Verify NO consumer still imports `Concertable.Contracts.ImageDto` and banner uploads take a plain `IFormFile`. Check in: **B2B**, **Customer**, **Tests** (`ImageMappers.cs`).
- **[from Shared]** `OwnsAddress(...)` defaults `required: true` → County/Town become NOT NULL. Any owner whose address is genuinely optional MUST pass `required: false`, else inserts of address-less rows fail at runtime. Verify the `required` arg at every `OwnsAddress(...)` call site against that owner's nullability. Check in: **B2B** (Artist/Venue read models + entities), **Customer** (read models + entities), **Adapters** (Search Artist/Venue read-model configs).
- **[from Shared]** `OwnsAddress` maps County/Town to bare `County`/`Town` columns (no `Address_` prefix). Verify each consumer's re-scaffolded `InitialCreate` migration emits exactly those column names (no leftover `Address_County` etc., no duplicated manual `OwnsOne`). Check in: **Adapters + schema/migrations**.

## Findings
<!-- appended per area; finding IDs continue across areas: MS#, MB#, BUG#, SEED#, CV# -->

## Shared foundation (Kernel + Contracts) — reviewed 2026-06-22

Files: `EntityTypeBuilderExtensions.cs` (new), `Services/UriService.cs` (M), `Concertable.Contracts/ImageDto.cs` (deleted).

**No issues found in this area.** Checked correctness, microservice isolation, module boundaries, seeding, and C# conventions.

- `OwnsAddress<TOwner>` — verified correct: `Address` (`Concertable.Kernel/Address.cs`) has exactly `County` + `Town`, both given explicit column names, so `OwnsOne` leaves no property to silently default to an `Address_`-prefixed column. Returns the builder for chaining; `Navigation(...).IsRequired(required)` correctly drives owned-ref nullability. The `Address?` expression type is just to satisfy both required/optional call sites.
- **Isolation (Lens B):** placing the owned-`Address` mapping helper in shared `Kernel` is correct, not a violation — `Address` is an audience-agnostic value object (generic county/town) consumed by every service, so its EF mapping is the *intersection*, exactly what `Kernel` is for (`api/CLAUDE.md` "Shared code is the intersection").
- `UriService` — pure convention fix (`_urlSettings` → `this.urlSettings`), matches `CODE_CONVENTIONS.md` "no underscore prefix". Behaviour unchanged.
- `ImageDto` deletion — no defect here; consumer fan-out tracked in Cross-area notes.
- The XML `<summary>` on `OwnsAddress` earns its place (documents the no-prefix behaviour + the `required` subtlety) and stays within the "use sparingly" guidance.
