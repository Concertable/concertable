using Xunit;

namespace Concertable.IntegrationTests.Common;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<ApiFixture>;
