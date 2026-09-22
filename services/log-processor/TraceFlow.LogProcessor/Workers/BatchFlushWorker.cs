// using TraceFlow.LogProcessor.Configurations;
// using TraceFlow.LogProcessor.Services.Batching;
// using Microsoft.Extensions.Options;

// namespace TraceFlow.LogProcessor.Workers;

// public sealed class BatchFlushWorker : BackgroundService
// {
//     private readonly IBatchProcessor _batchProcessor;
//     private readonly ILogger<BatchFlushWorker> _logger;
//     private readonly TimeSpan _flushInterval;
//     public BatchFlushWorker(IBatchProcessor batchProcessor, ILogger<BatchFlushWorker> logger, IOptions<ProcessorOptions> options)
//     {
//         _batchProcessor = batchProcessor;
//         _logger = logger;

//         _flushInterval = TimeSpan.FromSeconds(
//             options.Value.FlushIntervalMs);
//     }
//     // public override async Task ExecuteAsync(CancellationToken cancellationToken)
//     // {
        
//     // }
// }