# Architecture-tests rename — roadmap

Collapse the home-grown "Composition" test tier into the idiomatic "Architecture" tier: one name for one
concept (architecture fitness functions), across projects, the tier gate, CI, and the published helper
package.

## Items

- [x] `architecture-tests-rename/tier-collapse` — rename the six composition-test projects to
  `.ArchitectureTests` (B2B merged into its existing ArchitectureTests), collapse the `Composition` tier in
  `TestConventions.targets`, build an `architecture` CI leg + `test.ps1` suite, update skill-routes and docs.
  Phase 1 was non-breaking; Phase 2 completed the published helper-package rename and platform sync.
