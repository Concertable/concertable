# Code review — Perf/MergeQueueLatency

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `b52df292909c0a52f3b2bc0d2ea4900fb00b02a0`  `(2026-09-09)`
**Security-reviewed up to commit:** `b52df292909c0a52f3b2bc0d2ea4900fb00b02a0`  `(2026-09-09)`
**Judgment:** `approved`

## Review pass — 2026-09-09 — full

**Candidate base:** `e4989767f6b083604e66d5649af8009e1e230406`
**Candidate head:** `cc3662a2cf39e86ff3f1197ccc2f94ed5cbf9d19`
**Candidate branch:** `Perf/MergeQueueLatency`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:6431361832c92dfb9a252732ec3aea40e477c03746c49fd315307aca7d05a9b7` `(6 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\review-bundle-973`
**Candidate bundle identity:** `sha256:ef823e2bf4bca4379025df7a442d4e06ced8b129c41427b9301fc0a55243c79f`
**Work-order path:** `reviews/Perf-MergeQueueLatency.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Layers: native/general and every lens ran as the strong parent over the frozen descriptor — this host's
subordinate dispatch is disabled by session policy, which `review` Stage 3 covers as the parent fallback.
Routed rules: `plans` (`skill_router.py` over the frozen path set), plus `docs-and-debt` for the guidance
files. The host `security-review` skill resolved the main checkout's branch
(`Docs/launch_seal-and-postgres-plans`) rather than this candidate, so its output was discarded and the
security layer also ran as the parent over the frozen range — required here because
`^\.github/workflows/` is a generic `security_paths` pattern in `merge_review_gate.py`.

### Findings

- [x] **MQL-1 — MEDIUM — correctness** — `.github/workflows/test.yml:186`
  The new `expand-merge` escape hatch was inert: no such label existed on the repository, so
  `gh pr edit --add-label expand-merge` fails and the workaround `PIPELINE_DEBT.md` now prescribes
  cannot be applied at the moment it is needed — a cut-over merge would be unmergeable with no
  indication why. Fixed by creating the label (`gh label create expand-merge`, colour `5319E7`),
  alongside the existing `skip-e2e`, `skip-e2e-ui` and `full-e2e`.

- [x] **MQL-2 — LOW — test-coverage** — `.github/workflows/tests/test_service_scope.py:96`
  `TIER_CASES` pinned `expand-merge` against `skip-e2e` but not against `full-e2e`, which is the
  combination that actually occurs: `merge` Step 4 applies `full-e2e` for any positive trigger, and a
  breaking published-shape change is one — so the expand merge that cannot pass UI E2E arrives carrying
  both labels. The branch order makes `expand-merge` win, which is the behaviour that keeps that merge
  landable, but nothing held it. Fixed by adding the `expand-merge outranks full-e2e` case.

- [x] **MQL-3 — LOW — docs-and-debt** — `docs/REMOTE_VALIDATION.md:11`
  The merge-queue gate row restated the exception's mechanics ("drops the UI lane only") rather than
  linking to its owner, giving the rule two homes to drift between — the failure mode `docs-and-debt`
  names. Fixed by reducing the row to a pointer at `PIPELINE_DEBT.md`, which owns the situation.

Considered and dropped: `PIPELINE_REDESIGN_PLAN.md` has no `_PROGRESS.md` companion, but that is
pre-existing legacy state on unchanged lines, `plan_graph.py` exits clean, and the plan carries its own
`## STATUS (live)` section which this change extends in the established shape. The `skip-tests` label is
dead in the classifier — also pre-existing, and outside this candidate.

Security: no findings. The only new interpolations into a `run:` block are `${{ matrix.service }}` and
`${{ matrix.project }}`, both literals defined in the workflow itself and not reachable by untrusted
input. `api_changed` derives from `git diff --name-only`, whose content a contributor controls, but it
is only ever expanded inside a quoted `printf` piped to `grep` — no eval, no unquoted expansion — and
git escapes newline-bearing paths. The label read is `grep -qixF` (fixed-string, whole-line) over values
only a write-access actor can set. Job `permissions` are unchanged and least-privilege, the secret `env`
blocks are unchanged, and both E2E lanes remain `merge_group`-gated, so no secret reaches a context it
did not already. The step has no `shell:` key, so it runs `bash -e` without `pipefail` and the new
`|| true` on the `api_changed` pipeline matches the existing `api_files` idiom.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `cc3662a2cf39e86ff3f1197ccc2f94ed5cbf9d19`
**Candidate head:** `2dca4706e835a5597cd2fb75106a7d99fa4597a2`
**Candidate branch:** `Perf/MergeQueueLatency`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:5fe1bc47c52fb87ce1f89d2726282ba0df3aa0b5721ff7109e9c0a26c07b2859` `(3 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\review-bundle-973-inc`
**Candidate bundle identity:** `sha256:37d281450262e69d7948e146c74f7c4caad47cc5d582c68f79f2c384f7f2cbaf`
**Work-order path:** `reviews/Perf-MergeQueueLatency.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

No findings. The delta is MQL-1..3's remediation: one added `TIER_CASES` entry, one shortened table cell,
and this work order. `test_service_scope.py` 41/41 and `e2e-ghcr-login.test.mjs` 11/11 green at this head,
and PR #973's own CI was green at the prior head (96 pass, 3 `skipping` — the merge-queue-only lanes).
No workflow file changed in this delta, so the full pass's security conclusion still holds over `test.yml`'s
unchanged content; both watermarks advance to this head.

## Review pass — 2026-09-09 — incremental

**Candidate base:** `2dca4706e835a5597cd2fb75106a7d99fa4597a2`
**Candidate head:** `b52df292909c0a52f3b2bc0d2ea4900fb00b02a0`
**Candidate branch:** `Perf/MergeQueueLatency`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:21389bba5b238048d67bd0fc66a12902f1ba20c44887ba905537fc7a7ba086b8` `(9 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\review-bundle-973-inc2`
**Candidate bundle identity:** `sha256:768b45c3c9004c78a0967baa1cbe4766fcebd39bb1717a382e06b8a614a5d9d7`
**Work-order path:** `reviews/Perf-MergeQueueLatency.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

No findings. `merge` Step 2 requires a branch behind base to merge base in, and a platform-sync bump (#972)
had landed, so this delta is that merge plus the previous pass's work-order update. It resolved no conflicts
and the whole code change is one line repeated across eight `Directory.Packages.props` files:
`ConcertableDotNetPlatformVersion` 0.1.0-alpha.0.1351 to 0.1.0-alpha.0.1353, already reviewed on `main`.
No workflow, script or test file changed, so the full pass's security conclusion is untouched and both
watermarks advance to this head. Exact-head PR CI is the build proof this pin change owes.
