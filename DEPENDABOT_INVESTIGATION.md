# Investigate & remediate the open Dependabot alerts

**Goal:** clear the 26 open Dependabot alerts (1 critical, 7 high, 15 medium, 3 low). Produce a short
triage, then land the actual fix (updated lockfile / `overrides`) with all four web builds green.

**This is a throwaway working doc.** Delete it in the same commit that lands the fix.

## Scope — what's actually affected

- **All 26 alerts are npm, all in `app/package-lock.json`.** Nothing in the .NET backend
  (`Directory.Packages.props` etc.) is flagged.
- The npm workspace root is **`app/`** (run all `npm` commands from there). There is already an
  `overrides` block in `app/package.json` — extend it for transitive pins rather than inventing a new
  mechanism.
- Alerts are on the **default branch (`master`)**, so this is a fix for code already in master →
  **branch off `master`** as `Fix/DependabotNpmVulns` (capitalized prefix per repo branch rules). Do
  **not** stack it on `Feature/BookingAgreement`.

## The alerts (deduped by package → target version)

| Sev | Package | Vulnerable | Fix ≥ | Direct/transitive | Note |
|-----|---------|-----------|-------|-------------------|------|
| 🔴 critical | `shell-quote` | ≤ 1.8.3 | **1.8.4** | transitive | newline escaping in `quote()` |
| 🟠 high | `vite` | 8.0.0–8.0.15 | **8.0.16** | **direct (dev)** | `server.fs.deny` bypass on Windows |
| 🟠 high | `ws` | multiple majors | **8.21.0** (also 6.2.4 / 7.5.11 if those majors resolve) | transitive | memory-exhaustion DoS |
| 🟠 high | `form-data` | 4.0.0–4.0.5 | **4.0.6** | transitive | CRLF injection in multipart names |
| 🟠 high | `hono` | < 4.12.25 | **4.12.25** | transitive | CORS wildcard+credentials reflect (+ many medium below) |
| 🟠 high | `undici` | < 6.27.0 | **6.27.0** | transitive | WebSocket DoS (+ header-injection medium + 2 low) |
| 🟡 medium | `postcss` | < 8.5.10 | **8.5.10** | transitive (via vite/build) | XSS in stringify output |
| 🟡 medium | `uuid` | < 11.1.1 | **11.1.1** | transitive | buffer bounds check |
| 🟡 medium | `tar` | ≤ 7.5.15 | **7.5.16** | transitive | file-smuggling parser differential |
| 🟡 medium | `js-yaml` | ≤ 4.1.1 / < 3.15.0 | **4.2.0** / **3.15.0** | transitive | quadratic DoS (two majors resolve) |
| ⚪ low | `@babel/core` | ≤ 7.29.0 | **7.29.6** | transitive | file read via sourceMappingURL |

`hono` accounts for ~10 alerts on its own (JWT scheme, mount prefix, IPv6 deny bypass, cookie
injection, serve-static path traversal, Lambda adapters, body-limit bypass). A single bump to
**4.12.25** clears all of them. `undici` has 1 high + 1 medium + 2 low, all fixed by **6.27.0**.

## Method

1. **Confirm the tree.** From `app/`: `npm audit` and `npm ls <pkg>` for each package above to see
   *who* pulls it in (which is direct vs transitive, and via which parent). Note anything dev-only vs
   runtime-reachable — but since fixes exist for every alert, prefer just fixing rather than arguing
   reachability. Flag (don't silently skip) any alert with no non-breaking fix.
2. **Direct deps:** bump `vite` in `app/package.json` to `^8.0.16` (verify no breaking change from
   8.0.13 — patch bump, should be safe).
3. **Transitive deps:** run `npm audit fix` first; for anything that won't resolve (a parent pins an
   old range), add a pin to the existing `overrides` block in `app/package.json` using the target
   versions above. Then `npm install` and re-run `npm audit` to confirm 0 remaining.
4. **Prefer the real fix over pinning.** If a parent has a newer release that depends on the patched
   version, bump the parent instead of overriding — an override is the fallback for abandoned/lagging
   parents. Note any override added and why (which parent lagged).

## Verification gate — all four web builds green

From repo root (the `@/` alias means type-check per-app, never multi-project):

```
./app/node_modules/.bin/tsc -b app/web/b2b/venue --force
./app/node_modules/.bin/tsc -b app/web/b2b/artist --force
./app/node_modules/.bin/tsc -b app/web/customer --force
cd app && npm -w @concertable/web-business run build   # business has no tsc step; vite is its gate
```

All must exit 0. A `vite` bump is the one most likely to affect the build — if any app fails to
build after it, that's the signal to investigate, not to revert blindly.

## Deliverable

- `app/package.json` (bumped `vite`, any new `overrides`) + regenerated `app/package-lock.json`.
- `npm audit` from `app/` showing **0 vulnerabilities** (or a short note on any that genuinely can't
  be fixed without a breaking major bump, with a recommendation).
- Commit on `Fix/DependabotNpmVulns`, one line per non-obvious override in the commit body.
- **Delete this file in that commit.**
