using System.Threading.Channels;
using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.Queue;

public sealed class PendingLogEventQueue : IPendingLogEventQueue
{
    private readonly Channel<PendingLogEvent> _channel;

    public PendingLogEventQueue()
    {
        var options = new BoundedChannelOptions(10_000)
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