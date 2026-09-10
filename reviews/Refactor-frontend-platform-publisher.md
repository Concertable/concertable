# Review — Refactor/frontend-platform-publisher

**Review status:** `complete`

**Judgment:** `approved`

**Reviewed up to commit:** `3381592ac731af68ee1380b679a5d64ae24d5837`

- Candidate: `081e149a2e0a4544e1c781adc5e322a6ee0e4f85..9b18e1c9dc8b181d72b5409f36b20a8cef5aa7bb`
- Branch: `Refactor/frontend-platform-publisher` (PR #978)
- Scope: all
- Changed paths: 7
- Path digest (SHA-256, NUL-joined): `9654f9be3036d6ca583c59e5155359535c29e506ead4952a7433a0e4cb7e50d2`
- Mode: new
- Routed skills: `plans` (the only route `skill_router.py` returns for these paths)
- Layers: native/general (`code-reviewer`), plans-convention lens (`workflow-review-lens`)
- Security layer: not run — no changed path classifies as security-sensitive
- Watermark note: stamped at the base merge above the remediation commit. Everything between the
  reviewed range and it is either this branch's own remediation of the findings below or
  `origin/main` content reviewed by its own pull requests; the only delta above the marker is
  `reviews/`.

## Findings

All four are fixed on this branch. None is open.

### F1 — the ledger asserted three incompatible states for 8A — major, fixed

The added "Checkpoint 8A, delivered" paragraph claimed the four IDs already publish from
`platform-frontend`, while the pre-existing Next Steps blocker still said the repository "does not
exist and publishes nothing" and another added entry called it empty. Verified at
`plans/platform/POLYREPO_FULLSTACK_PROGRESS.md` lines 38-42, 70-71 and 304-306 of the frozen head.
The publish claim was the false one: `changeset publish` is blocked on a package-access grant.

Fixed: all three state one thing — the repository holds the four tiers with green CI and the publish
is blocked — and the blocker carries its cause, why no API can resolve it, the exact grant and a
command as its resume condition.

### F2 — `emit_paths.py` silently ignored the map's `replicated` section — medium, fixed

`directives()` read only `include` and `rename`, so every emitted paths file dropped
`.editorconfig`, `.gitattributes`, `.gitignore` and `.dockerignore` with no signal, and a caller
could not tell an unrepresentable section from an overlooked one.

Fixed: the generator reports the `replicated` set it does not emit and says each target authors its
own — what the map's comment means, and what `platform-frontend` did.

### F3 — the new `.d.mts` returned `Buffer` from a package declaring no dependencies — medium, fixed

`Buffer` is an ambient Node global and the package's first public type to need one. A consumer
without `@types/node` resolved the shape to nothing, and the check added in this diff asserted only
`typeof aspNetDevelopmentHttps === "function"`, which cannot see a wrong return type because the
generated consumer runs `skipLibCheck: true`.

Fixed: `@types/node` is a `dependencies` entry at the version every app workspace pins, the node
profile asserts `const certificate: Buffer = aspNetDevelopmentHttps(".").cert`, and the harness
installs it. Verified both ways against a packed tarball — the correct declaration passes, one changed
to `{ cert: number; key: number }` fails.

### F4 — an `include` matching no tracked path was emitted silently — low, fixed

filter-repo keeps such a filter emptily and `validate_map.py` only reports *unclaimed* paths, so a
typo in one of the five new file-level entries would have dropped that file from the carve with every
gate green.

Fixed: includes are checked against `git ls-files` and a miss exits 1 naming the offenders. Verified
a planted `app/typo-does-not-exist` fails with exit 1 and emits no body, the `system` target still
fails cleanly on its unrepresentable `exclude`, and the regenerated paths file is byte-identical to
the one that drove the extraction — the strictness did not change the applied contract.

## Lens claims the parent verified independently

The plans lens had read-only tools and could not run commands. The parent had already run them:
`validate_map.py` reports `unclaimed 81 / >1 target 0` on this head and `unclaimed 85` on base
`081e149a2`; no bare `0.1.0` exists for any of the four IDs on the feed; and
`app/scripts/carve-fe.mjs` contains no `build-config`, `file:` dependency or `npm pack`.

The lens's suggested direction — restate 8A as designed-but-not-executed — was not taken. The
extraction, authoring, push, repository settings and CI are done and green, so the honest
reconciliation is a landed checkpoint with a blocked publish, not an unexecuted one.
