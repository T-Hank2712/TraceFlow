namespace TraceFlow.E2ETests.Infrastructure;

[CollectionDefinition("TraceFlow E2E")]
public sealed class E2ETestCollection
    : ICollectionFixture<KafkaFixture>,
      ICollectionFixture<RedisFixture>,
      ICollectionFixture<OpenSearchFixture>
{
}