# Pipeline / CI debt

Known problems in the build/test/merge **pipeline itself** (not product code) that we've worked
around and need to fix properly. Each entry: the problem, the interim workaround in use, and the
proper fix. Delete an entry when its proper fix lands.

---

## API Markdown changes trigger package and image publication

**Found:** 2026-09-10 while delivering the configurable-Deal architecture/plan update.

**Problem.** `.github/workflows/publish-packages.yml` and `publish-images.yml` both trigger on every
`api/**` path, including `ARCHITECTURE.md`. The former packs/publishes packages and triggers generated
consumer synchronization; the latter publishes the deployable-image matrix. The test workflow treats
Markdown as inert, so a docs-only PR can pass lightweight checks but still start unrelated package and
image releases on merge.

**Interim boundary.** Keep affected docs PRs unmerged unless their delivery explicitly owns the
package/image publication and sync chain. Do not change CI selection or use a skip marker as part of a
documentation task.

**Resolution condition.** The publication owner excludes purely inert documentation changes from both
release workflows while
retaining publication for executable/package/policy changes and mixed diffs. Focused classifier/trigger
coverage proves API Markdown alone publishes neither packages nor images, code plus Markdown still
selects the appropriate releases, and publication-policy repairs still execute. A docs-only merge must
produce no package version, container image/tag or generated version-sync PR.

---

## Breaking published-package (wire-format) change can't pass the merge-queue full UI E2E

**First hit:** PR #595 (camel-case JSON enums, backend + `@concertable/shared`/`@concertable/b2b`
expand), sync half PR #600.

**Problem.** A breaking change to a type in a published FE package follows the expand→publish→sync
cut-over (`package-cutover` skill): the *expand* merge flips the backend + shared npm packages, and
the b2b consumer surfaces (venue/artist/business) are *deferred* to the sync merge. But two gates
have **opposite requirements** for those surfaces during the cut-over window:

- `carve-fe (web/b2b/*)` builds each surface **from the published feed** (test.yml) → the surface
  must stay on the OLD wire shape (PascalCase) or carve-fe fails.
- The merge-queue **full UI E2E** runs those same surfaces against the NEW backend → the deferred
  (old-shape) surfaces can't function (27/32 UI scenarios failed on #595: app shell never renders).

So the expand merge **structurally cannot pass full UI E2E**, yet the `merge` skill's Step 4
mandates full E2E for a breaking published-shape change. The two rules conflict for this PR class.

**Interim workaround.** Land the expand merge with the **`expand-merge`** label (keep full **API** E2E,
which does pass and validates the backend flip); validate UI E2E on the *sync* merge, where the surfaces
are migrated and consistent. The generic `skip-e2e-ui` no longer works here: the classifier ignores both
Skip-E2E opt-outs for any `api/` runtime diff, so this class needs the named label that says which
documented situation is being claimed. Cost: a short window where `main`'s local FE surfaces are inconsistent with
the backend until the sync merge lands. Deployed surfaces are unaffected (they build from the
published package, which only changes at the sync cut-over).

**Proper fix (options, undecided).**
- Teach the `merge` skill / merge-queue E2E tier to recognise a published-package **expand** merge
  with deferred consumers and auto-select `skip-e2e-ui` for it (documented, not ad-hoc).
- Or a pre-publish mechanism so the consumer surfaces can flip in the same PR against a
  pre-published package (mirrors the .NET "prepare consumer against an exact local producer
  package" step in `package-cutover`), letting both carve-fe and UI E2E pass together.
