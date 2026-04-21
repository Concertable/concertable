# ReadDbContext Barrier — RESOLVED

This barrier has been fixed. All Search integration tests now pass.

## What Was Done

`ReadDbContext.OnModelCreating` was added, but the real fix went further: a full
`IEntityTypeConfiguration<T>` pattern was introduced in `Concertable.Data.Infrastructure/Data/Configurations/`
so no configuration is ever duplicated between contexts.

### Configuration classes created (Concertable.Data.Infrastructure/Data/Configurations/)

| File | Covers |
|---|---|
| `UserHierarchyConfigurations.cs` | `UserEntityConfiguration`, `CustomerEntityConfiguration`, `ManagerEntityConfiguration` |
| `ArtistEntityConfiguration.cs` | `ArtistEntityConfiguration` |
| `VenueEntityConfiguration.cs` | `VenueEntityConfiguration` |
| `OpportunityConfigurations.cs` | `OpportunityEntityConfiguration`, `OpportunityApplicationEntityConfiguration` |
| `ContractConfigurations.cs` | `ContractEntityConfiguration` + 4 concrete TPT types |
| `TransactionConfigurations.cs` | `TransactionEntityConfiguration`, `TicketTransactionEntityConfiguration`, `SettlementTransactionEntityConfiguration` |
| `MiscEntityConfigurations.cs` | `StripeEventEntityConfiguration`, `MessageEntityConfiguration`, `TicketEntityConfiguration`, `PreferenceEntityConfiguration`, `ConcertEntityConfiguration`, `ConcertGenreEntityConfiguration`, `ConcertImageEntityConfiguration` |

### How each context now applies config

**`ReadDbContext.OnModelCreating`:**
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly);
```

**`ApplicationDbContext.OnModelCreating`:**
```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly);
// + Ignore token nav props (ApplicationDbContext doesn't own those tables)
// + Write-specific HasOne<ArtistManagerEntity>()/HasOne<VenueManagerEntity>() FKs
```

**`IdentityDbContext.OnModelCreating`:**
```csharp
modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
modelBuilder.ApplyConfiguration(new CustomerEntityConfiguration());
modelBuilder.ApplyConfiguration(new ManagerEntityConfiguration());
// + token FK configs
```

### Key insight: IQueryable<T> vs DbSet<T> table naming

`ReadDbContext` exposes `IQueryable<T>` properties (not `DbSet<T>`), so EF cannot infer
table names from property names. Each configuration class must include an explicit `ToTable(...)`
call — this is now the case for all configs.

## Test results after fix

- 18/18 Search integration tests ✅
- 129/129 integration tests ✅
- 95/95 unit tests ✅
- 2/12 E2E tests still fail (`ConcertDraftTests`) — **pre-existing, unrelated to this work**

Total: **234/236** tests passing. The 2 E2E failures are pre-existing from the B-pre denormalization work.

## Next

Continue `IDENTITY_MODULE_REFACTOR.md` — remaining steps:
- ⬜ **Step 10e** — `ICurrentUserResolver` interface + implementation
- ⬜ **Step 12** — Full test suite at 236 (needs the 2 E2E fixes, separate issue)
