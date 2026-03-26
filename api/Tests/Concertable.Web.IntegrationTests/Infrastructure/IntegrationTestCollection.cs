using Xunit;

namespace Concertable.Web.IntegrationTests.Infrastructure;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<ApiFixture>;
