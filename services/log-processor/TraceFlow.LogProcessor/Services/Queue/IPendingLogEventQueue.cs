using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.Queue;

public interface IPendingLogEventQueue
{
    ValueTask WriteAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken);

    IAsyncEnumerable<PendingLogEvent> ReadAllAsync(
        CancellationToken cancellationToken);
}
