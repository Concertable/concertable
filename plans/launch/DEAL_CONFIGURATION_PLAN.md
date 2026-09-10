# Configurable Deal templates and revisions

> **Next steps live in @plans/launch/DEAL_CONFIGURATION_PROGRESS.md → `## Next Steps`.**

## Decision and scope

Adopt one versioned Deal template/configuration model, with relational metadata and capability
selections plus a PostgreSQL `jsonb` economic rule graph. Keep the finite rule and capability vocabulary
strongly typed in C#. The four existing arrangements become platform-supplied presets of that language,
not permanent persistence subclasses beside a fifth `Composite` case.

The current four-type model is appropriate for the current four products. The reason to replace it is
the intended configurable product, not a defect in the existing four-case implementation or a claim
that JSON alone increases database throughput. Configuration scales the supported economic combinations
without multiplying entity subclasses, forms, registrations, and migrations.

This is an architecture workstream, **not an additional MVP launch gate**. The MVP still offers FlatFee,
VenueHire, DoorSplit, and Versus through ordinary forms. Deliver the common representation when this
refactor is scheduled; it need not expose any new commercial arrangement. Tenant-authored templates,
tenant-specific feature entitlements, arbitrary combinations in the UI, and a visual builder are future
product slices. No Finbuckle adoption, subscription system, general workflow platform, or new payment
primitive is a dependency of this plan.

The durable design belongs in [Deal architecture §5](../../api/Concertable.B2B/src/Modules/Deal/ARCHITECTURE.md#5-configurable-deals--target-design).
This plan owns its delivery and the supersession of older whole-Deal closure assumptions.

## Domain meanings and lifetimes

| Concept | Meaning | Lifetime and ownership |
|---|---|---|
| `DealTemplate` | Reusable blueprint: typed parameter definitions, defaults, supported rule structure, and permitted capability choices | Deal-owned stable identity with editable draft and immutable published revisions; platform supplied initially |
| `DealConfiguration` | Reusable specialization of one exact template revision: chosen capabilities, fixed/default values, and explicitly permitted per-offer parameters | Deal-owned stable identity and immutable published revisions; platform-owned presets initially, optional tenant-owned specializations later |
| `Deal` | A concrete economic offer created from one configuration revision, with offer-specific values bound | Deal-owned editable aggregate with optimistic concurrency and immutable offer revisions; editing creates a new revision and never retargets an existing signature |
| `DealTerms` | The immutable typed economic value/graph inside a configuration or offer revision | No repository, identity, workflow, or independently mutable lifecycle; contains named typed future inputs where settlement depends on facts not yet known |
| `Contract` | The agreement formed at acceptance, containing the complete accepted economic and operational snapshot plus signing artifacts | Booking-owned, immutable; downstream Concert execution consumes its frozen handoff, not the latest Deal or configuration |

Configuration revisions pin template revisions; Deal revisions pin configuration revisions. No implicit
"latest" lookup participates in an accepted agreement. Configuration defaults are resolved before
issuing an offer revision. Future inputs such as door revenue remain named, typed inputs with an agreed
source, period, and calculation basis; they are not permission to leave agreed fees or percentages unset.

Platform ownership is explicit: use a discriminated owner scope with a checked optional tenant ID, not a
fake tenant or an identical copy of each preset per tenant. Future tenant overrides create a new owned
configuration revision; they do not mutate a shared template. Archiving a template prevents new use
without invalidating already referenced revisions. Read visibility of a negotiated offer/Contract is
counterparty-aware and separate from a tenant's private template library.

## Persistence decision

| Option | Strengths and limits | Decision |
|---|---|---|
| Relational envelope + one `jsonb` document per revision | Natural immutable unit; simple cloning and hashing; typed deserialization and full semantic validation still required. Embedded capability references, ownership relations, and reporting need additional enforcement/projections | Viable smaller design, but leaves useful capability relationships inside the document |
| Relational metadata and selected capabilities + `jsonb` term graph | Relational identity, ownership, unique revision numbers, references and capability lookups; flexible typed nested economics. Graph/capability consistency must be validated and committed as one aggregate | **Selected** |
| Fully normalized template/rule/parameter/capability tables | Strong foreign keys and ordinary financial queries; optional node shapes, graph ordering and whole-graph validity still need semantic validation. Per-kind tables couple new vocabulary to DDL; generic parameter/EAV rows weaken types | Use relational projections for demonstrated reporting needs, not as the primary authoring representation |
| Current relational inheritance + `Composite` | Lowest initial disruption and strong typing for four cases; creates two validation, snapshot, execution and reporting paths, with arbitrary permanent distinctions between equivalent arrangements | Rejected as the durable model; existing inheritance survives only until the coherent cut-over |

### Why composition and `jsonb`, rather than whole-Deal TPH

TPH is an inheritance mapping: one table, a discriminator identifying the C# subtype, and columns
covering the mapped types. `jsonb` is a PostgreSQL column type containing parsed, queryable JSON in an
internal binary representation. These are not mutually exclusive technologies: a TPH entity can have
a `jsonb` property. The decision here is first to replace the closed whole-Deal subtype taxonomy with
composition, then to store its bounded economic graph in `jsonb`. Current Deal persistence uses TPT,
not TPH; do not attribute TPT's subtype joins to TPH, which already stores a hierarchy in one table.

A configuration may have several term nodes, references and explicit composition operators. Its
published graph is naturally read, validated and snapshotted as one unit. Existing rule kinds can be
combined within template and capability constraints without creating a new whole-Deal entity type or
adding another set of term columns. Serializing the same four closed `DealTerms` variants into JSON
would change storage without delivering that product model. Conversely, a normalized rule-node model
could also support composition; `jsonb` is the selected storage fit, not the only scalable option.

The principal scalability gain is product/schema diversity, not an automatic throughput improvement:

| Workload | Implementation requirement and trade-off |
|---|---|
| Load one offer/configuration | Select by indexed relational ID and revision, then materialize its bounded graph and selected capabilities. TPH can also load one row efficiently; no speed advantage is assumed |
| List a tenant's offers | Filter/page/project relational metadata without deserializing every graph. Batch reads and event-wide operations remain real workloads |
| Filter or aggregate individual terms | Use measured JSON queries, targeted expression indexes or typed read projections. Ordinary typed columns may be simpler and faster for repeated financial aggregation |
| Edit a configuration | Publish a new atomic revision under the aggregate edit token. A JSON-path update still locks the containing row; it does not provide independent per-term concurrency |

Keep the document bounded; do not put an event's bookings, execution history or financial ledger inside
it. Immutable parsed revisions are candidates for caching by revision and semantic version if measured
load warrants it; no cache platform is a prerequisite. Benchmark document sizes, deserialization,
capability loading, indexes and write contention before making performance claims.

### Storage boundaries

Logical storage, with physical names settled during implementation:

- Template/configuration identity rows: ID, ownership scope, optional tenant ID, creator, lifecycle
  status, current revision pointer, and aggregate edit token. Revision rows: immutable ID, unique
  `(owner aggregate ID, revision number)`, provenance, document schema version and graph payload.
- Revision-owned capability selections: operation slot, capability kind and semantic version, and
  explicit rule-node bindings where needed. Enforce supported cardinality per slot; a required slot
  cannot be missing at publication. Capability implementation catalogs are code-owned, never tenant
  registrations or cross-module entity foreign keys.
- Deal identity/head and revision rows: owning tenant, source configuration revision, fully resolved
  effective terms, selected capabilities, and provenance. The offer's current head is editable; the
  published revision is not. An acceptance command identifies the exact offered revision and signature
  bindings, not merely the Deal ID.
- Contract: full effective document including the capability selections and semantic versions, source
  IDs and hashes, parties/roles, currency and rounding, revenue definitions, rendered legal terms and
  exact signing artifacts. A template/configuration foreign key or rendered prose alone is insufficient.

The rule graph is the sole economic authoring authority. Do not duplicate its editable amounts in
relational columns. Capability selections are authoritative relational revision children; the signed
snapshot materializes them with the graph. Any indexes or reporting projections derived from graph
values are read-only and have a rebuild/version contract.

### Worked composition and binding example

The following is semantic notation, not an approved wire schema or executable user expression. Phase 1
must settle the concrete C# shapes, discriminators, node IDs, parameter/input references and versions
before publishing documents. Each operator's meaning is implemented in the finite C# language.

| Supplied preset | Economic structure |
|---|---|
| FlatFee | `FlatCharge(Venue -> Artist, fee)` |
| VenueHire | `FlatCharge(Artist -> Venue, hireFee)` |
| DoorSplit | `PercentSplit(settlementRevenue, artistPercentage)` |
| Current Versus | Explicit sum of `Guarantee(guaranteeAmount)` and `PercentSplit(settlementRevenue, artistPercentage)` |

For these supplied presets, `settlementRevenue` preserves the current basis: Concertable ticket revenue
(`TicketsSold * Price`) plus venue-declared external door revenue. The external component remains
declared, not independently verified. A future template with another revenue basis is a distinct
economic agreement; it must not silently change either preset's formula.

FlatFee and VenueHire reuse the same charge semantics with different direction and values. DoorSplit
and Versus reuse the same percentage calculation. Sharing a term means sharing its typed definition
and implementation, not mutable values or identity across offers. Terms can own pure calculation and
validation behaviour; they do not own workflow orchestration, payment execution or other I/O.

A Versus template defines the guarantee-plus-share structure and allowed parameter slots. A reusable
configuration pins that template revision, supplies defaults/restrictions and chooses compatible
capabilities. An offer binds its permitted values, for example:

```text
DealTerms: GBP; Venue -> Artist
  total: Sum(guarantee, share)
  guarantee: Guarantee(GBP 500)
  share: PercentSplit(settlementRevenue, 20%)
  settlementRevenue: Concertable ticket revenue plus declared external door revenue
```

At acceptance the GBP 500 and 20% are fixed; the final revenue input is not yet available. Once that
input is GBP 2,000 (GBP 1,200 ticket revenue plus GBP 800 declared external revenue), the gross obligation
is GBP 900, before any separately defined commission or other
deductions. A DoorSplit offer using 70% of the same input yields GBP 1,400 through the same percentage
rule. Current Versus is addition, not the greater of guarantee and share; a collection of two nodes
without an explicit composition meaning is insufficient.

An ordinary configuration/offer cannot add another term outside the template's permitted structure.
That requires a template revision with an approved structural choice or a new template. A new supported
combination is data-only once its constituent rules, composition operator and required capabilities
are deployed; new calculation/effect semantics still require code and compatibility review.

### Strong validation and finite execution

Treat request JSON as untrusted. Deserialize only allowlisted, versioned C# rule shapes with typed
parameters (`Money`, bounded percentage, party role, trigger, input reference); reject unknown kinds,
unsupported versions, duplicate object keys and incompatible fields. Do not accept CLR type names,
scripts, arbitrary expression strings, SQL, network calls, unbounded loops, or user-supplied DI keys.
Possible language concepts include FlatCharge, PercentSplit and Guarantee; Hold, Release and Refund
are effect/capability concepts, not unrestricted arithmetic nodes. Their precise typed schemas and
legal compositions must be fixed before a publisher can select them.

One write-boundary compiler produces an immutable validated configuration or typed errors. It checks
shape, ranges, dimensions, currency/rounding, party direction, bounded graph size/depth, acyclicity,
input availability by lifecycle phase, and the complete term/capability compatibility relation. A
term requiring held funds needs the matching funding/hold path; releases and refunds must reference
the permitted payment operation and cannot precede their prerequisites. Do not validate only pairs
when a three-way combination or ordering can be invalid. Template parameter bounds and configuration
restrictions may narrow the code-owned language but cannot expand it.

Publishing and all server write/import paths require that validated value. Saving an incomplete draft
must not make it executable. Revalidate offer bindings before publication and at acceptance against
the pinned language semantics. Capability catalogs fail composition for missing/ambiguous finite
implementations; configuration publication fails for unsupported slots, versions, bindings or graphs.
Compile-time C# closure protects vocabulary handling, not the validity of arbitrary incoming JSON.
Execution still checks real balances, provider state, permissions and lifecycle prerequisites.

EF Core mapping and typed deserialization do not perform this economic compatibility proof. The domain
compiler owns it; database checks are backstops. Only its validated result can reach executable
publication or acceptance, including through non-HTTP import and internal command paths.

Module-owned capability descriptors identify the permitted operation slot, typed inputs/bindings,
prerequisites and effects. Validate selections against the whole graph and its phase-dependent input
availability, not a preset-name allowlist. Bind monetary effects to explicit obligation/node identities
and prove accounting coverage without overlapping funding or settlement of the same amount. A declared
funding path proves configuration compatibility, not that funds have actually arrived at execution.

Use the worked example above for the following compatibility fixtures. These are semantic requirements,
not a claim that every illustrated capability exists in today's implementation:

| Proposed binding | Required outcome |
|---|---|
| Charge the final `total` at acceptance | Reject: final `settlementRevenue` is unavailable; never substitute zero or an estimate for the agreed final basis |
| Settle the `total` after the agreed revenue input is finalized | Structurally eligible only with a deployed capability supporting that input, direction, phase and obligation; runtime prerequisites still apply |
| Fund `guarantee` at acceptance and settle the remaining share later | Reject unless the deployed partial-funding/accounting capability explicitly supports and credits the funded component; separate charge and settlement handlers alone are insufficient |
| Release funds without a compatible preceding funding/hold path | Reject even if the individual release capability is known |
| Bind two payment paths to overlapping amounts of the same obligation | Reject double-payment coverage; deliberate partial allocations require an explicitly supported allocation/accounting model |
| Move Booking acceptance after Concert settlement | Reject: configuration cannot reorder the owning aggregate lifecycle |

UI step filtering may use the same published descriptor/validation contract for feedback, but the
server validates the submitted graph regardless of what the UI offered. Do not introduce a global
workflow executor, tenant-authored code, or new partial-payment primitives to satisfy these examples.

### PostgreSQL enforcement and queries

- Use relational primary/foreign keys, unique revision keys and ordinary B-tree indexes for identity,
  tenant, status, ownership and revision/capability selection. Foreign keys remain within the owning
  module's schema; cross-module handoffs use Contracts and copied values.
- Use `NOT NULL`, row-local `CHECK`s and explicit required-key/type tests for document shape and simple
  financial bounds. Ensure failed/missing JSON predicates cannot pass as SQL `NULL`. PostgreSQL JSON
  path error suppression must not turn malformed data into an accepted configuration.
- These checks supplement the compiler; foreign keys and `CHECK`s alone do not prove a complete
  graph, its funding semantics, or compatibility with a deployed implementation. Enforce immutable
  published revisions and their capability children at the database write boundary; editing means
  inserting a new revision. Restrict writes to the owning service and approved migration tooling.
- Add GIN indexes only for demonstrated containment/JSON-path queries, choosing `jsonb_ops` versus
  `jsonb_path_ops` for the actual operators. GIN is not a general numeric range index. Use targeted
  B-tree expression indexes or typed relational read projections for frequent fee/percentage/range
  reporting, including semantic kind, currency, units and document version.
- Report actual settlement, commission, invoice and refund amounts from the financial facts their
  owning modules already persist. Term analytics are a different read model; a future revenue-share
  rule is not an already earned settlement amount. Benchmark representative event volumes and query
  plans before claiming a throughput improvement.

### Revisions, signing, evolution and concurrency

Keep business revision, document schema version and rule/capability semantic version distinct. Preserve
old semantics/readers and calculation fixtures while agreements still depend on them; do not silently
reinterpret an old rule kind after a deployment. Where runtime facts arrive later, persist their
provenance and the calculated result/version so settlement remains reproducible.

Acceptance atomically verifies the offered revision and signing intent, creates the immutable Contract
snapshot and advances the owning lifecycle transition. Use the existing acceptance transaction and
operation-claim/idempotency boundaries; do not add a distributed transaction with Payment. A later Deal
edit cannot alter or implicitly re-sign an accepted agreement. Contract amendments require a separate
explicit agreement path, outside this plan.

Canonicalize the typed semantic payload for a versioned content hash; keep exact signed text/PDF bytes
separately. `jsonb` does not preserve original JSON formatting/key order, so it is not a byte-exact
signature archive. Template revision retention alone does not replace the full Contract snapshot.

Use an explicit `bigint` optimistic edit token for each mutable configuration/Deal aggregate. Every
head, draft or capability-child change participates in the same compare-and-swap transaction and
increments that token; return a typed conflict for stale writers. Immutable revision IDs/numbers are
not concurrency tokens. This choice does not predetermine Phase 9's token for other B2B aggregates.

Upgrade configuration document schemas through explicit, validated transforms that create new
revisions; never rewrite published or signed documents in place. Support old documents with versioned
readers until their retention obligations end. New combinations/values within the supported vocabulary
require data publication, not DDL or deployment. New rule kinds, effects or semantics require an
application deployment and compatibility review; changed relational structures or indexes may require
a migration. Verify Npgsql mapping support for the chosen polymorphic graph; do not assume every C#
union/recursive shape is supported by EF JSON mapping.

## Behaviour and module ownership

Keep Application → Booking → Concert ownership and fixed lifecycle order. Configurable steps mean
approved module-local capabilities, bindings and ordering within allowed extension points, not a
tenant-defined replacement of the aggregate state machines. Deal owns economics and validation, never
end-to-end workflow execution. Each operation owner publishes the finite capability descriptor its
consumers need through Contracts; no global runtime workflow registry owns their implementations.

Factories select a supported capability kind/version from validated configuration, not a configuration
ID or whole-Deal enum. Retain genuine same-interface strategy families and named facades where behaviour
varies. Retain operation-owned unions only for genuinely different method headers; several configurations
may select the same capability. Do not preserve artificial per-preset mappers/calculators or resurrect
`IAcceptPaid` merely because an old plan illustrated it. Inventory the landed APIs: Payment now owns
payment-method input, so current homogeneous operations may remain homogeneous.

B2B still computes deal gross once and shares that result with payout/invoicing; Payment remains
deal-type-agnostic and owns commission configuration/bindings and money movement. Preserve the four
current formulas as fixtures, including the current Versus **guarantee plus percentage share**, not
`max(guarantee, share)`. Do not introduce tenant commission rates through deal configuration.

## Future economic vocabulary, explicitly outside this implementation

Preserve the ability to extend the finite language for more complex agreements at mid-sized events.
This is a product-discovery direction, not evidence of validated market demand or an instruction to
implement more operators in this refactor. The four-preset MVP and the delivery gates below do not grow.

| Candidate | Intended use | Semantics to settle before implementation |
|---|---|---|
| `Max` | Guarantee or revenue share, whichever is greater | Compare compatible monetary results for the same obligation; this is not current Versus's additive formula |
| `Min` | Cap a calculated payment | Define exactly which obligation/component the cap limits and how it composes with other agreed components |
| Tiered percentage | Different shares at different revenue thresholds | Specify marginal bands versus a rate applied to the whole base, threshold inclusivity, rounding and the agreed revenue basis |

Names here are candidates, not reserved wire discriminators or selectable capabilities. Prefer composing
existing operators when they express the required semantics; do not introduce separate `Cap`/`Floor`
kinds merely as labels for the same `Min`/`Max` calculation. Do not add placeholder handlers, arbitrary
expression evaluation, new payment primitives or a tenant builder to prepare for these candidates.

Before promoting a candidate into a separately scoped feature, validate its use against representative
real agreements and pin its typed inputs/results, versioned meaning and compatibility requirements.
Its fixtures must cover calculation boundaries, nesting with other supported rules, unavailable future
inputs, invalid payment/lifecycle bindings and historical replay. Preserve the four supplied formulas;
a different formula gets a new template revision/configuration rather than reinterpreting a signed one.
New operator semantics require a deployment; subsequent permitted combinations use ordinary validated
configuration publication, as described above.

## Future tenant eligibility, explicitly outside this implementation

Economic compatibility, tenant eligibility, actor permissions and data visibility are different checks.
Future eligibility can grant a tenant certain template/configuration IDs or finite capabilities, using
the existing verified active tenant and membership boundary. Tenant authoring/publishing/using a preset
must then pass both eligibility and economic validation; a frontend-hidden option is not enforcement.
Capability restrictions must cover imported graphs too, not just their advertised template ID.

Use existing tenant-filtered contexts for private configurations and explicit authorized read surfaces
for shared presets and counterparty-visible offers. Do not bypass filters or infer active tenancy from
an unvalidated claim. Eligibility changes govern new usage; they must not rewrite an existing Contract
or strand its required settlement/refund operations. Define any future revocation policy explicitly.

Finbuckle provides tenant resolution, stores and per-tenant options/integration infrastructure. It is
not required for this business authorization policy, does not validate the Deal language and is not an
MVP dependency. Evaluate it separately only if replacing/standardizing tenant infrastructure becomes a
demonstrated need.

## Delivery ownership and gates

- Consume the landed Application/Booking/Concert boundaries; refresh actual APIs before implementation.
  Keep operation-claim and seal-enforcement guarantees intact. Lifecycle method examples are not new
  requirements to recreate superseded interfaces.
- Integrate the active Deal vocabulary/mapper-collapse work and platform-commission baseline through
  their owning branches. They can finish independently; this design does not gate their delivery or
  authorize rewriting their live plans. The progress ledger records exact downstream handoffs.
- [Postgres migration](POSTGRES_MIGRATION_PLAN.md) owns the provider swap. Typed-language work is locally
  implementable first; delivery of this plan's B2B `jsonb` persistence requires the B2B provider cut-over.
  Do not grow SQL Server JSON persistence or dual-provider support to evade that gate.
- [Deal dispatch](DEAL_CLOSED_SUM_MODEL_PLAN.md) and the
  [B2B .NET 11 plan](../dotnet-11/B2B_WORKFLOW_UNIONS_PLAN.md) may improve compile-time closure of finite
  vocabulary. Configurability does not wait for .NET 11 or a new public dispatch library. Whole deals
  cease to be the closed sum after this cut-over.

### Phase 1 — executable finite language and four-preset equivalence

Inventory the landed Deal, lifecycle, signing and commission consumers. Fix the typed vocabulary,
capability descriptor/compatibility contract and version policy. Implement the compiler and economic
evaluator, consumed by the existing four supported offer/calculation paths; current fields may adapt
to typed inputs during this checkpoint. Do not merge an unused language framework or expose a builder.

Gate: all four current formulas, payer/payee directions, invoice/commission inputs, legal renderings
and lifecycle capabilities remain equivalent. Negative fixtures reject unsupported kinds, invalid
bindings, unsafe ordering and incompatible whole graphs, including the worked compatibility cases
above. Prove repeated rule types have independent values, multi-term composition is explicit, template
restrictions cannot be expanded by a configuration, and every executable write path uses the compiler.
Include mixed-source revenue fixtures proving both ticket sales and declared external revenue enter
the percentage base exactly once. Record every changed module/public consumer and its precise
input/output contract before the persistence cut-over.

### Phase 2 — revision storage and coherent offer-to-Contract cut-over

After the B2B Postgres gate, persist templates/configurations, capabilities and typed graphs through the
selected hybrid model. Supply four platform presets and bind the existing forms to them. Switch offer
editing, application signature intent, acceptance, Contract issuance, downstream lifecycle handoffs,
settlement and invoice consumers to the same pinned revision/snapshot path. Reconcile retention/data
migration with the actual release state: pre-launch fixtures can be regenerated; any real agreements
require a proven preservation/backfill path, not deletion or semantic reinterpretation.

Gate: Postgres integration proves atomic revision publication and acceptance, tenant/counterparty
visibility, stale-write conflicts, immutable revisions/children, old-version reads and signing races.
Contract tests show template edits cannot change accepted economics or capability selection. No runtime
consumer silently reads the latest configuration. If a published contract changes, deliver its producer,
package and all consumers as an explicit coordinated cut-over, not a compatibility alias.

### Phase 3 — retire whole-Deal subtype identity and prove extension

Remove obsolete entity subclasses, relational subtype tables, whole-Deal discriminators and per-preset
dispatch/form dependencies once all consumers use the shared model. Preserve genuinely finite rule and
operation alternatives and ordinary launch form presentation. Delete temporary Phase 1 adapters in the
same coherent cut-over; these phases may be checkpoints of one source PR where no independent delivery
boundary exists.

Gate: a test fixture publishes an additional supported configuration without adding a C# Deal subtype,
enum member, DI registration or database migration, and unsupported combinations fail before execution.
Existing four-preset journeys remain equivalent; representative financial queries have measured indexes
or projections. Required local checks, isolated review, exact-head CI, publication/consumer closure and
merge-queue checks are terminal green before implementation is called delivered.

## Technical evidence

- [PostgreSQL JSON types and indexing](https://www.postgresql.org/docs/current/datatype-json.html),
  [constraints](https://www.postgresql.org/docs/current/ddl-constraints.html), and
  [JSON-path functions](https://www.postgresql.org/docs/current/functions-json.html).
- [EF Core inheritance mapping: TPH and TPT](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance).
- [Npgsql JSON mapping](https://www.npgsql.org/efcore/mapping/json.html) and
  [concurrency](https://www.npgsql.org/efcore/modeling/concurrency.html).
- [Finbuckle configuration and usage](https://www.finbuckle.com/MultiTenant/Docs/v10.1.3/ConfigurationAndUsage).
