# Code review — Chore/local-dev-config

> **This file is a work order, not a discussion.** Fix open `[ ]` findings directly; tick `[x]` as landed.

**Review status:** `complete`
**Reviewed up to commit:** `254211396`  `(2026-08-27)`
**Security-reviewed up to commit:** `3eef7d10867f806550fa83bbc5bcf906332cfd14`  `(2026-08-27)`
**Judgment:** `approved`

## Review pass — 2026-08-27 — full

**Candidate base:** `085520405dc79e98b4e8bfcf982ec1225a36249a`
**Candidate head:** `254211396`
**Candidate branch:** `Chore/local-dev-config`
**Candidate scope:** `all`
**Work-order path:** `reviews/Chore-local-dev-config.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

### Scope

Dev tooling + docs, no runtime/product/package/test-selection code (so it goes through the queue with
`skip-e2e`, not the meta-only bypass — the `.ps1` and `.example` paths are out of the meta-only list):

- `scripts/setup-local-dev.ps1` — new, `[CmdletBinding(SupportsShouldProcess)]`, idempotent (guards on
  file existence and `dotnet user-secrets list` output before writing). Dry-run (`-WhatIf`) and a real
  run both exercised: creates the three `appsettings.Development.json` files and sets nine user-secrets,
  and on a second run reports every one as "left as-is".
- `api/{Auth,B2B.Web,Customer.Web}/…/appsettings.Development.json.example` × 3 — `https://localhost:517x`
  origins only, no secrets. `.gitignore` matches `**/appsettings.Development.json` exactly, so `.example`
  is tracked (confirmed — git staged them as `A`).
- `docs/LOCAL_DEV.md` (new, reachable from `README.md` + `docs/INDEX.md` — `docs_reachability.py` 0
  errors), `README.md` + `docs/INDEX.md` pointers.

### Findings

None. The script's guards are correct, the templates carry no secrets, and the doc has one owning home
linked from the two places a reader starts. The `//`-prefixed keys in the `.example` JSON are inert under
the .NET config binder (no property matches) and are a common appsettings-comment idiom — left as-is.

### Parent finalization

Self-review by the authoring context — the diff is small, mechanical, and was executed end to end
(script run, files verified, CI green: 66 pass / 5 merge_group-skipped). No lens dispatched. Approved.

### Security layer

Ran on the `appsettings.Development.json.example` (auth-config) paths. **Zero findings.** The templates
carry only `https://localhost:517x` origins; `setup-local-dev.ps1` takes no untrusted input (all
arguments are hardcoded literals or fixed-array elements — no injection or path traversal) and the
`local-dev-shared-service-secret` value is a documented localhost placeholder that grants nothing off-box
and does not weaken production (Key Vault). Marker stamped at `3eef7d10`.
