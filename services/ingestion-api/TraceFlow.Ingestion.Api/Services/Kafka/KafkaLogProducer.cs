using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TraceFlow.Ingestion.Api.Configuration;
using TraceFlow.Ingestion.Api.Services.Ingestion;
using TraceFlow.Ingestion.Api.Contracts.Log;
using TraceFlow.Ingestion.Api.Contracts.BatchLog;

namespace TraceFlow.Ingestion.Api.Kafka;

public sealed class KafkaLogProducer : ILogEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaLogProducer> _logger;
    public KafkaLogProducer(IOptions<KafkaOptions> options, ILogger<KafkaLogProducer> logger)
    {
        _options = options.Value;
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            MessageSendMaxRetries = _options.MessageSendMaxRetries,
            RetryBackoffMs = _options.RetryBackoffMs,
            MessageTimeoutMs = _options.MessageTimeoutMs,
            QueueBufferingMaxMessages = _options.QueueBufferingMaxMessages,
            QueueBufferingMaxKbytes = _options.QueueBufferingMaxKbytes,
            LingerMs = _options.LingerMs
        };
        config.Set("delivery.timeout.ms", _options.DeliveryTimeoutMs.ToString());
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public Task PublishAsync(
        EnrichedLogEvent logEvent,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        var eventId = logEvent.EventId;

        try
        {
            _producer.Produce(
                _options.Topic,
                CreateMessage(logEvent),
                deliveryReport =>
                {
                    if (deliveryReport.Error.IsError)
                    {
                        _logger.LogError(
                            "Kafka background delivery failed. EventId: {EventId}, Error: {Error}",
                            eventId,
                            deliveryReport.Error.Reason);
                    }
                });

            return Task.CompletedTask;
        }
        catch (ProduceException<string, EnrichedLogEvent> ex)
        {
            _logger.LogError(
                ex,
                "Kafka produce failed (Buffer full/Broker unavailable). EventId: {EventId}",
                eventId);

            return Task.FromException(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error publishing log event. EventId: {EventId}", eventId);
            return Task.FromException(ex);
        }
    }

    public IReadOnlyList<BatchLogItemResult> Publish(
    IReadOnlyList<(int Index, EnrichedLogEvent Event)> logEvents, CancellationToken cancellationToken = default)
    {
        if (logEvents.Count == 0)
        {
            return Array.Empty<BatchLogItemResult>();
        }

        var results = new BatchLogItemResult[logEvents.Count];

        for (var i = 0; i < logEvents.Count; i++)
        {
            var item = logEvents[i];
            var eventId = item.Event.EventId;

            try
            {
                _producer.Produce(
                    _options.Topic,
                    CreateMessage(item.Event),
                    deliveryReport =>
                    {
                        if (deliveryReport.Error.IsError)
                        {
                            _logger.LogError(
                                "Kafka delivery background status: Failed. EventId: {EventId}, Error: {Error}",
                                eventId,
                                deliveryReport.Error.Reason);
                        }
                    });

                results[i] = new BatchLogItemResult(
                    Index: item.Index,
                    Accepted: true,
                    EventId: eventId,
                    Error: null);
            }
            catch (ProduceException<string, string> ex)
            {
                results[i] = new BatchLogItemResult(
                    Index: item.Index,
                    Accepted: false,
                    EventId: eventId,
                    Error: ex.Error.Reason);
            }
            catch (Exception ex)
            {
                results[i] = new BatchLogItemResult(
                    Index: item.Index,
                    Accepted: false,
                    EventId: eventId,
                    Error: ex.Message);
            }
        }

        return results;
    }
    private static Message<string, string> CreateMessage(
        EnrichedLogEvent logEvent)
    {
        return new Message<string, string>
        {
            Key = logEvent.EventId.ToString(),
            Value = JsonSerializer.Serialize(logEvent)
        };
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
