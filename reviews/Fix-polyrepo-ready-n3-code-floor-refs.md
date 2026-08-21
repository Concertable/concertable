# Review — Fix/polyrepo-ready-n3-code-floor-refs

> **This file is a work order, not a discussion.** Fix open `[ ]` findings directly; tick `[x]` as landed.

**Reviewed up to commit:** `6d864c41a848260f28737b7add311da27d447b30`  _(2026-08-21)_

**Security-reviewed up to commit:** `6d864c41a848260f28737b7add311da27d447b30`  _(2026-08-21)_

> Range reviewed: `origin/main..6d864c41` (1 commit).

## Scope

N3 follow-up: repoints the two references to the now-deleted `api/AGENTS.md` / `api/CLAUDE.md` that live
outside `#698`'s meta-only scope (docs-review findings ACC1 HIGH, ACC3 LOW). The entire diff is **two
comment/prompt-text lines**:

- `.github/workflows/claude-review.yml:34` — remove `api/AGENTS.md` from the PR-review bot's read list.
- `StripeAccountController.cs:12` — repoint an XML-doc-comment citation from `api/CLAUDE.md` to the
  `microservice-boundaries` skill.

## Findings

- _None._ Lenses checked (correctness, security, accuracy): the diff changes no executable code — one line
  of a GitHub Actions prompt string and one XML documentation comment. No control flow, no inputs, no
  auth/authorization/credential/secret logic, no route or contract change.

## Security review

**No findings.** The workflow change removes a documentation path from an AI-review prompt's read list — no
change to permissions, triggers, secrets, or job steps. The `.cs` change is a doc-comment citation. Neither
alters any authorization, credential, or payment code path in `StripeAccountController` (the authorize→forward
logic is untouched).
