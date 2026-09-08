# Code review — Refactor/RepoSplit-M4-Closure-Repair

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `5f1c672c490da68cb41751f3b16f1ca2803b0da8`  `(2026-09-08)`
**Security-reviewed up to commit:** `5f1c672c490da68cb41751f3b16f1ca2803b0da8`  `(2026-09-08)`
**Judgment:** `approved`

## Review pass — 2026-09-08 — full

**Candidate base:** `c108226e9` (Platform Contract PR #945 head)
**Candidate head:** `4030eed8d31151f2ef49a17a4f214323f034745d`
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

`skip-e2e-ui`, decided on the final head rather than inherited from this branch's obsolete history.

A positive trigger is present: the Payment gRPC transport change is a cross-service wire concern, because
B2B and Customer reach Payment over gRPC. So end-to-end coverage is **not** skipped — API E2E is retained,
and it is the suite that actually exercises that path, driving B2B checkout through to Payment over gRPC
against live main. `PaymentTransportTests` covers the mechanism in the integration tier over dynamically
allocated ports, and `carve-auth` now proves the closure seam builds package-clean.

The browser suites are excluded because nothing in the candidate changes a user-facing flow: they would
re-drive the same B2B-to-Payment path with a UI layer on top of it, adding ~30 minutes and no coverage of
what changed. A transport regression fails API E2E first and faster.

### Validation

- `Concertable.Payment.Web`, `Concertable.Payment.IntegrationTests` and `Concertable.Auth.Contracts` all
  build clean, the last one rebuilt after the pin alignment.
- Exact-head PR CI and the merge queue, including API E2E against live main, are the authoritative gates.
  Delivered as PR #959; the rebuilt branch was force-pushed by Tommy, since the rebuild rewrote its history.

### Extended to the landed M1 stack

After the initial pass the candidate was brought onto the merged M1 stack: `0c7116ecc` merges
`origin/main` at Platform Contract's merge `2b5e8aad0` plus the 1335 pin sync, cleanly with no conflicts, and
`4030eed8d` re-aligned the `Concertable.Auth.Contracts` pin to 1335, and after sync PR #958 landed 1338 the candidate merged main once more and settled that pin at 1338 because sync PR #957 could not reach a file
that exists only on this branch. Nothing else changed and `Concertable.Auth.Contracts` rebuilds clean.

The pin was deliberately not chased while publishes were still in flight, because each M1 merge produced a
new version. With the whole M1 stack landed and sync PR #958 merged, main is settled at 1338 and all nine pin
files now agree. From here `bump-platform-version.sh` reaches this folder by grep like the rest.

### CI gate extension is correct for the seam

`carve-auth` now archives `api/Concertable.Auth.Contracts` into the carved tree and adds it to `CarveAuth.slnx`,
so the package-clean gate proves `Auth.Contracts` restores and builds from the feed alongside the Auth
runtime. That is the right CI change to accompany the closure seam: `Auth.Contracts` has just joined Auth's
deployable closure, and without this the carve job would not cover it. The `split-inventory` comment is
widened to say the gate covers runtime as well as test-tier cross-repository edges.

### Security pass

`.github/workflows/` is a security-sensitive path, so this candidate requires a security marker. Its workflow
diff carries no security-relevant change: no `permissions`, `secrets.*`, `GITHUB_TOKEN`, `pull_request_target`
or registry-login edits — only a comment and the two `carve-auth` lines above. The one credential-adjacent
change in the candidate moves in the fail-closed direction: an insecure `http://` gRPC address now throws
unless `PaymentClient__AllowInsecureHttp` is set explicitly, where previously it was accepted.
