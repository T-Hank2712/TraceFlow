namespace TraceFlow.Ingestion.Api.IntegrationTests.Infrastructure;

[CollectionDefinition("Ingestion API Integration")]
public sealed class IngestionApiTestCollection
    : ICollectionFixture<KafkaFixture>,
      ICollectionFixture<RedisFixture>
{
}