# Data-access roadmap

DataAccess repository/base and context-enforcement work for the shared `Concertable.DataAccess`
package and its service consumers.

## Items

- [x] Cosmos-aligned repository facets + context-enforced no-tracking `data-access/repository-redesign`
- [x] EF context/repository permission hierarchy `data-access/repository-context-permission-hierarchy`
  — the capability seam is stable; any package extraction remains separate while Kernel entity and
  messaging couplings are measured.
- [ ] DateTimeOffset audit contract and Payment adoption `data-access/audit-datetimeoffset`
- [ ] Shared concurrency-save recovery and Payment adoption `data-access/try-save-changes`
