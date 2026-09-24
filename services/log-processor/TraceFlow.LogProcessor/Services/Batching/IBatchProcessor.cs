using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.Batching;

public interface IBatchProcessor
{
    Task<BatchProcessResult> AddAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken);

    Task<BatchProcessResult> FlushAsync(
        CancellationToken cancellationToken);
}
