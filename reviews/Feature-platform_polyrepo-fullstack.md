# Code review — Feature/platform_polyrepo-fullstack

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `22959ea5c7f233ac0471c7e828f9bd1f9ce06875`  _(2026-08-05)_

> Range reviewed: `6f825b3ee..22959ea5c` (9 commits; Phase 3a code = `d6ac4b123` rename + `c4775ebf1` carve-prep).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

Notes (not findings):
- This diff is 100% frontend (`app/**` + one docs-only `mirror.yml` comment removal), so the backend lenses (microservice isolation, module boundaries, seeding, C# conventions) do not apply.
- `app/scripts/carve-fe.mjs` (new): reviewed for correctness — Windows npm-cli shim, tar drive-letter handling, `*`→`alpha` feed-spec rewrite, self-contained `.npmrc`, per-surface VITE placeholders. Sound. Intentionally not yet CI-wired (deferred publish-first per the plan), so no coverage gap.
- The ~191-file specifier rename is behavior-preserving; grep confirms **0** stale `@concertable/<tier>/shared|web/shared` specifiers remain, and the full green gate (`build:packages` + 4 web builds + 2 mobile `tsc --noEmit`) proves every import resolves against the renamed exports maps.
- Surface `package.json` closure declarations + `verify-fe-package.mjs` specifier updates are consistent with the rename. Whether a few CLI-ish deps (`shadcn`) belong in `dependencies` vs `devDependencies` is exactly what the deferred `carve-fe-web` CI (Phase 3b) will shake out — not a runtime correctness issue here.
