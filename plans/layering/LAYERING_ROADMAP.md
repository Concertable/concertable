# Layering and mapper hygiene roadmap

Two related defects surfaced together: `*.Domain` projects reference `*.Contracts` projects (so a domain
entity can legally take a wire DTO), and several "mapper" families are keyed-DI strategies whose arms carry no
dependencies and no behaviour. The first permits the second to look reasonable.

The epic ships when no B2B domain project can see a DTO and every surviving mapper is static, total and
dependency-free.

## Items

- [ ] Deal vocabulary split and DI-mapper collapse — `layering/deal-vocabulary-and-mapper-collapse`.
  Introduces the B2B-local vocabulary project, cuts `Deal.Domain`'s reference to `Deal.Contracts`, and
  collapses the `IDealMapper`, `IDealUpdater`, `IPaymentAmountMapper`, `ITransactionMapper` and `IUserMapper`
  families. Deal is the proving ground for the template the remaining modules follow.

- [ ] Remaining `*.Domain` → `*.Contracts` references — `layering/remaining-domain-contracts-references`.
  The other 11 modules (Admin, Artist, Concert, Conversations, Tenant, User, Venue in B2B; Messaging; Payment;
  Customer.Preference) follow the same template. Blocked on the item above only for the template, not for
  design. Concert.Domain is the awkward one: it reaches across modules for `Deal.Contracts` as well as its own.

- [ ] Vocabulary to the shared published contract — `layering/vocabulary-to-shared-contracts`.
  `Concertable.Contracts` already holds `Genre` and is the conceptually right long-term home for shared
  vocabulary. It is a **published package**, so this is a breaking published-contract change: it needs its own
  expand/contract design and cannot land in one PR. Deliberately deferred behind the two items above so a
  service-local project unblocks them first.

- [ ] Rename the two genuine-DI application mappers — `layering/application-mapper-renames`.
  `Concert.Api/ApplicationMapper` injects `IConcertWorkflowCapabilityRegistry` to gate HATEOAS links — it is a
  presenter. `Concert.Application/ApplicationMapper` injects `IContractRepository` and batches to avoid an
  N+1 — it is an assembler. Both are correctly dependency-injected and simply misnamed; neither is a mapper.
  Independent of the other items.

## Cross-item ordering

The vocabulary project introduced by the first item is the consumption contract for the second. The third
supersedes the location chosen by the first, and must not start until the second is terminal — otherwise the
remaining modules migrate twice.
