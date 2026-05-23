using System.Linq.Expressions;
using Concertable.Kernel.Specifications;

namespace Concertable.Kernel.UnitTests.Specifications;

public class SpecificationTests
{
    [Fact]
    public void PredicateSpecification_Apply_FiltersByPredicate()
    {
        var sut = new MinAgeSpec(18);
        var query = new[] { new Person(10), new Person(20), new Person(30) }.AsQueryable();

        var result = sut.Apply(query).ToArray();

        Assert.Equal([20, 30], result.Select(p => p.Age));
    }

    [Fact]
    public void PredicateSpecification_WithParams_Apply_FiltersByBuiltPredicate()
    {
        var sut = new AgeAtLeastSpec();
        var query = new[] { new Person(10), new Person(20), new Person(30) }.AsQueryable();

        var result = sut.Apply(query, 25).ToArray();

        Assert.Equal([30], result.Select(p => p.Age));
    }

    [Fact]
    public void PredicateExpressionSpecification_ApplyVia_RebindsPredicateThroughNavigation()
    {
        var sut = new MinAgeExpressionSpec(18);
        var employees = new[]
        {
            new Employee(new Person(10)),
            new Employee(new Person(20)),
            new Employee(new Person(30))
        }.AsQueryable();

        var result = sut.ApplyVia(employees, e => e.Person).ToArray();

        Assert.Equal([20, 30], result.Select(e => e.Person.Age));
    }

    [Fact]
    public void PredicateExpressionSpecification_WithParams_ApplyVia_RebindsBuiltPredicate()
    {
        var sut = new AgeAtLeastExpressionSpec();
        var employees = new[]
        {
            new Employee(new Person(10)),
            new Employee(new Person(20)),
            new Employee(new Person(30))
        }.AsQueryable();

        var result = sut.ApplyVia(employees, e => e.Person, 25).ToArray();

        Assert.Equal([30], result.Select(e => e.Person.Age));
    }

    private record Person(int Age);
    private record Employee(Person Person);

    private class MinAgeSpec : PredicateSpecification<Person>
    {
        private readonly int min;
        public MinAgeSpec(int min) { this.min = min; }
        protected override Expression<Func<Person, bool>> Predicate => p => p.Age >= min;
    }

    private class AgeAtLeastSpec : PredicateSpecification<Person, int>
    {
        protected override Expression<Func<Person, bool>> BuildPredicate(int min) => p => p.Age >= min;
    }

    private class MinAgeExpressionSpec : PredicateExpressionSpecification<Person>
    {
        private readonly int min;
        public MinAgeExpressionSpec(int min) { this.min = min; }
        protected override Expression<Func<Person, bool>> Predicate => p => p.Age >= min;
    }

    private class AgeAtLeastExpressionSpec : PredicateExpressionSpecification<Person, int>
    {
        protected override Expression<Func<Person, bool>> BuildPredicate(int min) => p => p.Age >= min;
    }
}
