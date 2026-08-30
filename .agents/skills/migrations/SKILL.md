---
name: migrations
description: Concertable never adds an additive migration — a model change means running `./initial-migrations.ps1` from `api/` to nuke and re-scaffold every module's `InitialCreate`, and because no environment holds live data the re-scaffold is free and lossless, so a migration is never a cost to weigh, to defer against, or even to mention as an argument against a change. Use when changing an entity or its configuration, when tempted to write an incremental migration, or when about to raise migration effort as a reason not to do something.
---

# Migrations — never additive, and never a cost to weigh

When the model changes, run `./initial-migrations.ps1` from `api/` to nuke and re-scaffold every module's
`InitialCreate`. Do not add an additive migration.

**A migration is NEVER a cost to weigh.** There is no live or production data — every environment is seeded
from scratch — so re-scaffolding is free and lossless. Never defer, avoid, water down, or even *mention*
"but this needs a migration" as a reason against a change. If the model is right, change it, run
`./initial-migrations.ps1`, move on.

The re-scaffold is a mechanical step, not a design consideration and not a blocker. Surfacing it as one is
the failure this rule exists to stop.
