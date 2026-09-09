# Review — Refactor/frontend-platform-publisher

- Review status: in-progress
- Judgment: pending
- Candidate: `081e149a2e0a4544e1c781adc5e322a6ee0e4f85..9b18e1c9dc8b181d72b5409f36b20a8cef5aa7bb`
- Branch: `Refactor/frontend-platform-publisher` (PR #978)
- Scope: all
- Changed paths: 7
- Path digest (SHA-256, NUL-joined): `9654f9be3036d6ca583c59e5155359535c29e506ead4952a7433a0e4cb7e50d2`
- Mode: new
- Routed skills: `plans`
- Layers run: native/general (`code-reviewer`), plans-convention lens (`workflow-review-lens`)

## Pass 1

- Pass judgment: pending

### F1 — `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md` asserts three incompatible states for 8A — **major, fixed**

The added "Checkpoint 8A, delivered" paragraph claimed "the four IDs' first non-prerelease versions
publish from there", while the pre-existing Next Steps blocker still read "checkpoint 8A has not run —
`Concertable/platform-frontend` does not exist and publishes nothing" and an added decisions entry
described the repository as "public, empty". A reader could not tell which was true, and the
publish claim was in fact false: `changeset publish` is blocked on a package-access grant.

Verified at `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md` lines 38-42, 70-71 and 304-306 of the
frozen head.

Fix applied: all three now state one thing — the repository holds the four tiers with green CI, the
publish is blocked, and the blocker carries its cause, why no API can resolve it, the exact grant, and
an observable resume condition. The stale "Unblock action: Tommy creates the repository" is gone,
because he already did.

### Lens claims the parent verified independently

The plans lens had read-only tools and could not execute commands. The parent had already verified,
by running them: `validate_map.py` reports `unclaimed 80 / >1 target 0` on the candidate and
`unclaimed 85` on base `081e149a2`; no bare `0.1.0` exists for any of the four IDs on the feed; and
`app/scripts/carve-fe.mjs` contains no `build-config`, `file:` dependency or `npm pack`.

The lens's suggested direction — restate 8A as designed-but-not-executed — was **not** taken: the
extraction, authoring, push, repository settings and CI are all done and green, so the honest
reconciliation is a landed checkpoint with a blocked publish, not an unexecuted one.
