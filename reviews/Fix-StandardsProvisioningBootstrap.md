# Code review — Fix/StandardsProvisioningBootstrap

**Review status:** complete
**Judgment:** approve

**Reviewed up to commit:** `b5217a1672ce51631afe7ae369b179e0585c6568`  _(2026-09-01)_
**Security-reviewed up to commit:** `b5217a1672ce51631afe7ae369b179e0585c6568`  _(2026-09-01)_

Frozen range `de552cf05893dcda189463bfb2a53b92c54fd00d..b5217a1672ce51631afe7ae369b179e0585c6568`
— 29 files, +525/-314. Path digest `f057ca207bd102c77056b77dae069fdcd2690a11da5cf0d3d80a96567819c79c`.
Native/general layer and every lens run by the parent over the frozen range; no subagent dispatch was
authorized in this session, which is the procedure's documented parent fallback.

Routed skills re-opened and checked against the changed files: `csharp-style`, `csharp-naming`,
`e2e-scenarios`, `dotnet:integration-testing`, `dotnet-standards:unit-testing`, `dotnet:http-api`,
`dotnet-standards:result-terminals`, `docs-and-debt`, `plans`.

Security layer required: `.github/workflows/test.yml` matches the hook's generic `^\.github/workflows/`
pattern and `api/Concertable.Payment/...` matches this repo's `(^|/)Concertable\.Payment`.

## Findings

- [x] **Genre query values were rejected case-sensitively (HIGH, correctness).**
  `CommaDelimitedGenreArrayModelBinder` used `Enum.Parse<Genre>(s)`; the SPA sends the camelCase wire
  value the JSON seam emits (`rock,indie`), so `/api/header` returned 400 and the Customer search
  scenario timed out. Fixed by parsing with `ignoreCase: true`. This is the only custom model binder in
  `api/`, so it was the only place that had lost ASP.NET's case-insensitive default enum binding.

- [x] **Undefined numeric genres were cast blindly (MEDIUM, correctness).**
  The old `int.TryParse(s, out var i) ? (Genre)i : ...` turned `genres=9999` into an undefined `Genre`
  that flowed into the query. Now rejected via `Enum.IsDefined`. Exception-driven control flow and the
  `catch (Exception)` swallow were replaced with an explicit loop.

- [x] **The wait predicate hid the response status (MEDIUM, diagnosability).**
  `resp.Url.Contains("/header?") && resp.Ok` made any 4xx/5xx indistinguishable from a request that
  never fired. Moved to `RunAndWaitForOkResponseAsync`, a focused helper in the shared UI helper library
  beside the existing `PageNavigationExtensions` rather than a private copy in one page object, per
  `e2e-ui-debug`'s "diagnostics must preserve scenario semantics". Single call site; success and
  failure outcomes unchanged for that caller.

- [x] **Stale URL assertion (LOW).** The feature still asserted `genres=Rock,Indie`, left behind by the
  camelCase cutover. Now `genres=rock,indie`.

- [x] **`@quarantine` masked a deterministic regression (MEDIUM, process).**
  The gating `e2e-ui-tests` job runs `--filter "Category!=quarantine"`, so run `33184255084`
  (2026-08-28, `success`) never executed this scenario. The tag requires *proven* non-determinism;
  this failure was deterministic. Tag removed, scenario returned to the blocking lane, verified 7/7
  locally. It was the repo's only quarantined scenario, so the non-gating lane was made to tolerate a
  filter matching zero tests without swallowing genuine failures.

- [x] **Test tier could not observe the defect (MEDIUM, test-coverage).**
  The existing `HeaderApiTests` genre tests interpolate `Genre.ToString()` — the one casing that
  worked. Added a theory over every `Genre` member asserting the declared name and the
  `JsonNamingPolicy.CamelCase` wire value return identical results, a `headerType` casing theory, and
  undefined-value rejection. Placed in the existing `Search` region per `dotnet:integration-testing`.
  Verified non-vacuous: reverting `ignoreCase` fails all 8 genre cases.

- [wontfix] **`SignUpSteps.RegisterAsUser(string user)` discards its parameter (LOW).**
  `_ = user;` — the step regex `they register as (.*)` captures a value nothing consumes. Pre-existing
  (`_ = persona;` before the rename); the rename touched the line, not the defect. Out of scope for
  this branch; either assert on the captured value or drop it from the step text.

- [wontfix] **`PlaywrightHooks` lambda variable `p` (LOW).** A leftover abbreviation of "persona" in
  the commit that retired the word. Cosmetic; no rule in the loaded standards states it.

- [wontfix] **`e2e-ui-regress` and `e2e-ui-debug` now reference removed mechanisms (MEDIUM, docs).**
  This branch deletes `E2E_BASELINE.md` and the `ui regress` lane from `scripts/e2e.ps1`, but the
  `e2e-ui-regress` skill and three passages in `e2e-ui-debug` still instruct agents to use the baseline
  and that command. That is agent-loaded guidance pointing at deleted artifacts, so it will actively
  mislead. It lives in `Concertable/agent-standards`, not this repo, so it cannot be fixed in this PR —
  tracked as a follow-up there. `plans/platform/*.md` also reference the baseline, on unchanged lines.

## Security layer

No security findings that block. Reviewed every path matching the gate's inventory:

- `.claude/settings.json` — the new SessionStart command quotes `$CLAUDE_PROJECT_DIR` and the
  `cygpath` result, and `exec bash` receives a quoted script path. No unquoted expansion.
- `standards_provisioning.py` — no network and no subprocess; every failure path exits 0 so a broken
  check cannot wedge a session. Noted, not blocking: it prints plugin and marketplace *names* read from
  a cloned `marketplace.json` into hook stdout, which becomes agent context. That is a narrow
  prompt-injection surface bounded by marketplaces the user chose to add, and the names print only on
  the unresolved path.
- `.github/workflows/test.yml` — the added step interpolates no untrusted `${{ }}` value, exposes no
  secret, and does not widen the trigger surface. The `grep` guard admits exactly one VSTest message.
- `LoginCaptureHooks.cs` — reads only `expires_at` from the stored session; tokens are never logged.
  Storage state is held in a process-local static for the test run only.
- `CommaDelimitedGenreArrayModelBinder.cs` — tightens input validation on an `[AllowAnonymous]`,
  rate-limited endpoint and leaks no internal detail in its validation message. Net security
  improvement.
- `PageResponseExtensions.cs` — includes a response body in a failure message, matching what
  `Browser.cs` already logs for error responses. Test-only diagnostics.
