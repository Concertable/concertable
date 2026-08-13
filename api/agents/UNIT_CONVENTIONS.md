# Unit Test Conventions

Conventions for `*.UnitTests` projects — pure in-memory tests of domain/service logic with **no**
DB, `WebApplicationFactory`, Testcontainers, fixtures, or HTTP. If a test needs any of those it's an
integration test — see [`INTEGRATION_CONVENTIONS.md`](./INTEGRATION_CONVENTIONS.md).

General C# style (field naming, `this.`-qualification, no primary-ctor capture) is governed by
[`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md) and applies here too — this doc only adds the
unit-test-specific rules.

## Framework & shape

- **xUnit.** `[Fact]` for a single case; `[Theory]` + `[InlineData(...)]` for tabular cases (keep the
  expected value as the last `InlineData` argument).
- Test class is `public sealed class XTests`, namespace = the project's root namespace
  (`Concertable.<Service>.<Module>.UnitTests`).
- **Arrange / Act / Assert**, separated by blank lines, no `// Arrange` comments — the blank lines
  carry it.

```csharp
public sealed class VatPolicyTests
{
    private readonly IVatPolicy policy;

    public VatPolicyTests()
    {
        this.policy = new VatPolicy(new UkVatCalculator());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Apply_UnregisteredSupplier_ReturnsNone(string? supplierVatNumber)
    {
        var result = policy.Apply(120m, supplierVatNumber);

        Assert.Equal(120m, result.Net);
    }
}
```

## Architecture-guard allowlists

A repo-wide guard (e.g. an arch test that scans every source file) sometimes needs a temporary
allowlist for files mid-migration. Express the allowlist as a **`public static TheoryData<>`** feeding
a self-verifying `[Theory]` that asserts each allowlisted item **still** violates the rule — so a stale
entry fails the theory and forces its removal. The guard `[Fact]` excludes that same `TheoryData` (one
source of truth). Never a silently-suppressing exclusion list that can rot.

## Naming

`Method_Scenario_ExpectedBehaviour` — `Apply_RegisteredSupplier_DecomposesInclusiveGross`,
`Create_ArtistDoorPercentOutsideRange_ThrowsDomainException`. The scenario segment names the input state; the last
segment names the observable outcome. (Some older tests use the terser `Method_ShouldXxx` — prefer the
three-part form for new tests.)

## SUT construction

- A SUT with dependencies is built **in the test-class constructor** and held as a `this.`-qualified
  `private readonly` field (same rule as production — see `CODE_CONVENTIONS.md`).
- **Prefer real collaborators over mocks** when they're cheap and deterministic
  (`new VatPolicy(new UkVatCalculator())`, not a mocked `IVatCalculator`). Reach for a test double only
  at a genuine boundary (I/O, time, randomness, an expensive/nondeterministic dependency).

## Assertions

Unit tests currently use xUnit's built-in `Assert.*` (`Assert.Equal`, `Assert.True`, …). *(Integration
tests use Shouldly `ShouldBe` — see `INTEGRATION_CONVENTIONS.md`. Whether unit tests should adopt
Shouldly too is an open call — codify it here once decided.)*

## Grouping a large test class

When one test class covers several methods of a SUT, divide it into `#region`s named for the **method
under test** (`#region Apply`, `#region Create`). A cluster that isn't a single method
names the behaviour instead (`#region Late capture compensation`). This is the established pattern
across the suite. Regioning is for navigation, not a licence to let a class sprawl — when it outgrows
comfortable regioning, that's the cue to split it (by SUT method, into separate files), not to pile on
more regions.

<!-- STARTER DOC — extend with the unit-test rules you want enforced. -->
