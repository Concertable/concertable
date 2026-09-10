# Frontend domain model roadmap

> **Goal:** make transformations between frontend domain shapes explicit, discoverable, and owned by
> the feature that defines them, while keeping React components, HTTP transport encoding, external
> adapters, and presentation projections at their proper boundaries.
>
> **Current decision:** retain interfaces for object contracts and add same-name value companions for
> reusable pure conversions. Companion operations use source-owned `toX` names and live directly
> below the owning type declarations in the feature's `types.ts`. Closed labels use direct typed
> tables, cohesive stateful capabilities use one service/facade, stores remain private, client-owned
> absence uses `undefined`, and no mapping library is being added.

## How to continue this roadmap

The selected item is active on `Refactor/frontend_domain-companion-mapping` in the worktree recorded
by [`DOMAIN_COMPANION_MAPPING_PROGRESS.md`](DOMAIN_COMPANION_MAPPING_PROGRESS.md). Continue only from
that ledger's `## Next Steps`; it owns the current phase, verification evidence, and delivery state.

## Status

### Selected

- [ ] 🟡 **Domain companion mapping convention and migration.** `frontend/domain-companion-mapping`
      Design and operational state: [`DOMAIN_COMPANION_MAPPING_PLAN.md`](DOMAIN_COMPANION_MAPPING_PLAN.md)
      and [`DOMAIN_COMPANION_MAPPING_PROGRESS.md`](DOMAIN_COMPANION_MAPPING_PROGRESS.md). Establish the
      interface-plus-companion convention, correct the known read/write contract leaks, migrate reusable
      pure transformations, make domain-facing APIs cohesive, keep stores private, normalize owned absence,
      and keep transport, adapter, and presentation mappings boundary-local.

## Dependency map

```text
Current frontend enum and guidance PRs land
└── refresh the mapping inventory from current origin/main
    └── establish the companion convention and canonical Opportunity example
        ├── correct form-buffer and slim request boundaries
        ├── correct read types reused as write bodies
        └── verify every inventoried transformation has one explicit owner
```

## Adoption rules

- Interfaces remain the standard for object shapes and HTTP contracts.
- A companion is a same-name exported `const` containing pure synchronous operations for that source
  type. It is not a class, namespace, prototype extension, service, hook, or dependency-injected mapper.
- Companion methods use `toX`; destination-owned `from`, generic `map`, and `convert` names are not
  introduced.
- Companions live in the owning feature's `types.ts`, directly below the related declarations. This
  migration does not create `mappers/`, `utils/`, `domain/`, or global shared mapping folders.
- Zod owns validation and normalization at user-input boundaries. HTTP modules own wire encoding;
  adapters own third-party decoding; render code owns presentation-only projections.
- A companion is introduced only for a reusable or semantically meaningful named conversion. Identity
  mappings and one-use one-or-two-field bodies remain direct.
- No runtime mapper or functional-programming dependency is added for this work.

## Epic definition of done

- The convention and its exclusions are recorded in frontend guidance after the active guidance
  restructure lands.
- Every occurrence named by the plan is migrated or explicitly retained at its correct boundary.
- Frontend writes accept slim `XRequest` contracts and never accept a server read type merely because
  it contains the writable fields.
- Every edited form parses its raw buffer before constructing a request.
- Companion behaviour with semantic branching, omission, normalization, or restructuring has focused
  unit coverage; trivial identity copies are not padded with tests.
- All frontend package, boundary, and surface build gates required by the plan are green on the exact
  reviewed head.
