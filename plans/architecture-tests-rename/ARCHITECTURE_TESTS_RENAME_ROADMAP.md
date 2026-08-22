# Architecture-tests rename — roadmap

Collapse the home-grown "Composition" test tier into the idiomatic "Architecture" tier: one name for one
concept (architecture fitness functions), across projects, the tier gate, CI, and the published helper
package.

## Items

- [ ] `architecture-tests-rename/tier-collapse` — rename the six composition-test projects to
  `.ArchitectureTests` (B2B merged into its existing ArchitectureTests), collapse the `Composition` tier in
  `TestConventions.targets`, build an `architecture` CI leg + `test.ps1` suite, update skill-routes and docs.
  Non-breaking (shared lib untouched). *(Phase 2 — the published-package rename — is folded into this item's
  plan as its second phase, since it is the same workstream continued as a publish-then-bump chain.)*
