using Xunit;

namespace Concertable.Testing.Integration;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<ApiFixture>;
