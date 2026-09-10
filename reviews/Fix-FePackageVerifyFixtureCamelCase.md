# Code review — Fix/FePackageVerifyFixtureCamelCase

> Work order, not a discussion. Fix open `[ ]` findings directly.

**Reviewed up to commit:** `7d389983c43bb06c40e2075a1024d7b03569b78a`  _(2026-08-20)_

> Range: full PR (one commit). Producer fix unblocking the camel-case cut-over: the
> `publish-fe-packages` verify step's inline consumer fixture still used Genre `"Rock"`.

## Findings

No issues found. Checked:

- **Correct wire/label split.** `Genre` wire values are camel-case (`"rock"`) and
  `genreLabel("rock") === "Rock"` (display label) per `app/shared/src/types/common.ts`. The fixture's
  wire-value uses flip to `"rock"`; line 50's assertion changes from `!== genre` (relied on the old
  wire==label coincidence) to `!== "Rock"` (the actual label). Node type-check, node runtime, and metro
  checks all consistent.
- **Scope.** Only `app/scripts/verify-fe-package.mjs`; only the three genre literals. No product code,
  no security paths — no security marker.
- **Why PR CI didn't catch the original.** `verify-fe-package.mjs` runs only in the `publish-fe-packages`
  workflow (post-merge on `app/*/shared/**` pushes), not in PR checks — landing this re-triggers it.
