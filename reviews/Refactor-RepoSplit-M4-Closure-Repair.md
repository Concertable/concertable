# Code review — Refactor/RepoSplit-M4-Closure-Repair

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `6d317d348d463b1828eda95f341ed38a45cf0439`  `(2026-09-08)`
**Judgment:** `approved`

## Review pass — 2026-09-08 — full

**Candidate base:** `c108226e9` (Platform Contract PR #945 head)
**Candidate head:** `6d317d348d463b1828eda95f341ed38a45cf0439`
**Candidate branch:** `Refactor/RepoSplit-M4-Closure-Repair`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:7eb3c7d474174abbf905e3f31e017631a30f6b3ac74d95f43e3a81020213ac35` `(18 paths)`
**Work-order path:** `reviews/Refactor-RepoSplit-M4-Closure-Repair.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

This is a **new frozen watermark, not an extension**. The prior artifact on this branch approved an obsolete
base: the branch carried 19 commits, of which 15 were stale rebased copies of the P2/P3/P4 work, confirmed
patch-identical by `git range-diff`. The candidate was rebuilt as the branch's four real commits cherry-picked
onto Platform Contract's head, which produced one plan-document conflict instead of the seven code conflicts a
merge produced.

Lenses applied: the cross-repository closure seam, Payment transport correctness, fail-closed credential
handling, and package-pin consistency.

### Findings

- [x] **M4-1 — MEDIUM — package closure** — `api/Concertable.Auth.Contracts/Directory.Packages.props:5`
  The closure-seam commit introduces `api/Concertable.Auth.Contracts` as an eighth folder carrying a
  `ConcertablePlatformVersion` pin, at `0.1.0-alpha.0.1329` — three versions behind main's
  `0.1.0-alpha.0.1332`. Landing it as written would put two versions of `Concertable.Messaging.Contracts`
  in one dependency graph until the next generated sync corrected it. Fixed by `6d317d348`, which aligns the
  pin with main; `Concertable.Auth.Contracts` rebuilds clean at 1332.

### Verified clean

- **The closure seam is the actual point of this stage and it is complete.**
  `Concertable.Auth.Contracts.csproj` replaces its `ProjectReference` to
  `Concertable.Messaging.Contracts` with a `PackageReference` — the last cross-repository runtime source edge
  in the graph. After this, no deployable project reaches outside its service folder by source.
- **The new pin needs no workflow change.** I checked `bump-platform-version.sh` rather than assuming: it
  discovers pin files with `grep -rlE '<ConcertablePlatformVersion>…' "$root/api" --include=Directory.Packages.props`,
  not a fixed list, so this eighth folder is picked up automatically by every future sync. This is
  deliberately unlike `ConcertablePaymentVersion`, which the sync genuinely does not track and which
  therefore drifts by hand.
- **Payment transport now selects protocol per endpoint rather than globally.** `ConfigurePaymentTransport`
  sets `HttpProtocols.Http2` on the gRPC port and `Http1AndHttp2` elsewhere, replacing a blanket
  `Http1AndHttp2` on all endpoints. The gRPC port comes from `PaymentTransport:GrpcPort`, which
  `AppHostExtensions` emits from `PaymentConstants.GrpcPort`.
- **The duplicated `8081` default is not a maintenance hazard.** It appears in `PaymentConstants.GrpcPort`
  (the AppHost's declaration) and as `?? 8081` in `Payment.Web`, which deliberately does not reference
  `Payment.Hosting` — a runtime service depending on its own AppHost composition package would invert the
  boundary. The env var is the contract and the literal is the standalone fallback. `PaymentTransportTests`
  allocates ports dynamically and asserts the mechanism honours the config, so it pins the behaviour rather
  than a port; the `8081` in its insecure-URL test is incidental to the URL string.
- **Credential handling is fail-closed.** The insecure-HTTP path is opt-in through an explicit
  `PaymentClient__AllowInsecureHttp` variable, and `AddPaymentClient` throws `InvalidOperationException` for
  an `http://` gRPC address otherwise, with a test asserting the throw.
- **Coverage is real, not incidental.** `PaymentTransportTests` is 134 new lines exercising the transport
  end to end over dynamically allocated ports.

### E2E tier

To be set at delivery from the merged diff. The candidate changes Payment's runtime transport configuration
and a published package boundary, so it is not obviously skippable in the way Platform Contract was; decide
it against the `merge` skill's positive-trigger list on the final head rather than inheriting a label from
this branch's obsolete history.

### Validation

- `Concertable.Payment.Web`, `Concertable.Payment.IntegrationTests` and `Concertable.Auth.Contracts` all
  build clean, the last one rebuilt after the pin alignment.
- Exact-head PR CI and the merge queue are the authoritative gates. The PR does not exist yet; publishing the
  rebuilt branch requires a force-push, since the rebuild rewrote its history.
