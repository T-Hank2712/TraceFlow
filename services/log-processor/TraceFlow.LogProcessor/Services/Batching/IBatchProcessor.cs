using TraceFlow.LogProcessor.Contracts;

namespace TraceFlow.LogProcessor.Services.Batching;

public interface IBatchProcessor
{
    Task AddAsync(
        LogEvent logEvent,
        CancellationToken cancellationToken);

    Task FlushAsync(
        CancellationToken cancellationToken);
}