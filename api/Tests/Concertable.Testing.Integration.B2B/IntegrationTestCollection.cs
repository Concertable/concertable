using Xunit;

namespace Concertable.Testing.Integration.B2B;

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<ApiFixture>;
