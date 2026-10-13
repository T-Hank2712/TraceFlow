using System.Threading.Channels;
using TraceFlow.LogProcessor.Contracts;
using Microsoft.Extensions.Options;
using TraceFlow.LogProcessor.Configurations;

namespace TraceFlow.LogProcessor.Services.Queue;

public sealed class PendingLogEventQueue : IPendingLogEventQueue
{
    private readonly Channel<PendingLogEvent> _channel;

    public PendingLogEventQueue(
        IOptions<ProcessorOptions> processorOptions)
    {
        var options = new BoundedChannelOptions(
            processorOptions.Value.QueueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        };

        _channel = Channel.CreateBounded<PendingLogEvent>(options);
    }

    public ValueTask WriteAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(
            pendingEvent,
            cancellationToken);
    }

    public IAsyncEnumerable<PendingLogEvent> ReadAllAsync(
        CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(
            cancellationToken);
    }
}
