namespace TraceFlow.LogProcessor.IntegrationTests.Infrastructure;

[CollectionDefinition("Log Processor Integration")]
public sealed class LogProcessorTestCollection
    : ICollectionFixture<KafkaFixture>,
      ICollectionFixture<OpenSearchFixture>
{
}