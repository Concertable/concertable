# Code review — Fix/container-image-digest-pinning

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `db8fb1d0ce8b6ae22c1f8d8d575a931ebe6b5bc2`  _(2026-08-31)_

**Security-reviewed up to commit:** `db8fb1d0ce8b6ae22c1f8d8d575a931ebe6b5bc2`  _(2026-08-31)_

> Range reviewed: `eda7300e9974676c5f99585a26644ac6b2c1074e..db8fb1d0ce8b6ae22c1f8d8d575a931ebe6b5bc2` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 / SEC1 — HIGH — runtime and container supply chain** —
  `api/Concertable.AppHost.Shared/DistributedApplicationBuilderExtensions.cs:13`
  `AddContainerImage` passed an OCI digest to Aspire as an image tag. The first correction then passed
  the full `sha256:<hex>` reference to `WithImageSHA256`, whose annotation expects only the 64-character
  hexadecimal payload. The helper now removes the canonical `sha256:` prefix and supplies the payload to
  `WithImageSHA256`; the focused regression exercises the prefixed consumer input and asserts the exact
  normalized `ContainerImageAnnotation.SHA256` value.

## Final review — 2026-08-31

No open findings. Independent native correctness and security re-reviews are clean at `db8fb1d0c`.
The corrected helper preserves the repository separately and produces Aspire's immutable
`image@sha256:<digest>` model rather than a mutable or malformed tag reference.

### Verification

- `Concertable.AppHost.Shared.UnitTests`: 12 passed, 0 failed.
- `Concertable.AppHost.Shared` Release pack completed successfully.
- `python eng/repository-split/inventory.py --check` completed successfully.
- `git diff --check` completed successfully.
