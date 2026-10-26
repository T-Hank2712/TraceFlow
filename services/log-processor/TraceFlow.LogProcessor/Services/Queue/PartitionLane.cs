namespace TraceFlow.LogProcessor.Services.Queue;

public sealed class PartitionLane : IAsyncDisposable
{
    private readonly Channel<PendingLogEvent> _channel;
    private readonly IBatchProcessor _batchProcessor;
    private readonly IOffsetCoordinator _offsetCoordinator;
    private readonly IOffsetCommitSignal _offsetCommitSignal;
    private readonly ILogger<PartitionLane> _logger;
    private readonly TimeSpan _flushInterval;
    private readonly CancellationTokenSource _runCts = new();

    private Task? _readTask;
    private Task? _flushTask;
    private readonly IHostApplicationLifetime _applicationLifetime;

    private int _failureSignaled;

    public PartitionLane(
        TopicPartition topicPartition,
        int queueCapacity,
        TimeSpan flushInterval,
        IBatchProcessor batchProcessor,
        IOffsetCoordinator offsetCoordinator,
        IOffsetCommitSignal offsetCommitSignal,
        ILogger<PartitionLane> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        TopicPartition = topicPartition;
        _batchProcessor = batchProcessor;
        _offsetCoordinator = offsetCoordinator;
        _offsetCommitSignal = offsetCommitSignal;
        _logger = logger;
        _flushInterval = flushInterval;
        _applicationLifetime = applicationLifetime;

        var options = new BoundedChannelOptions(
            queueCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true
        };

        _channel = Channel.CreateBounded<
            PendingLogEvent>(options);
    }

    public TopicPartition TopicPartition { get; }

    public void Start()
    {
        _readTask = Task.Run(
            ReadLoopAsync);

        _flushTask = Task.Run(
            FlushLoopAsync);
    }

    public ValueTask EnqueueAsync(
        PendingLogEvent pendingEvent,
        CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(
            pendingEvent,
            cancellationToken);
    }

    private async Task ReadLoopAsync()
    {
        try
        {
            await foreach (var pendingEvent in
                _channel.Reader.ReadAllAsync(
                    _runCts.Token))
            {
                var result = await _batchProcessor.AddAsync(
                    pendingEvent,
                    _runCts.Token);

                MarkProcessed(result);
            }
        }
        catch (OperationCanceledException)
            when (_runCts.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Partition lane stopped. Topic: {Topic}, Partition: {Partition}",
                TopicPartition.Topic,
                TopicPartition.Partition);
        }
        catch (Exception exception)
        {
            HandleUnexpectedLoopFailure(exception, "read");
            throw;
        }
    }

    private async Task FlushLoopAsync()
    {
        using var timer = new PeriodicTimer(
            _flushInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(
                _runCts.Token))
            {
                var result = await _batchProcessor.FlushAsync(
                    _runCts.Token);

                MarkProcessed(result);
            }
        }
        catch (OperationCanceledException)
            when (_runCts.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            HandleUnexpectedLoopFailure(exception, "flush");
            throw;
        }
    }
    private void HandleUnexpectedLoopFailure(
        Exception exception,
        string loopName)
    {
        if (Interlocked.Exchange(ref _failureSignaled, 1) != 0)
        {
            return;
        }

        _logger.LogCritical(
            exception,
            "Partition lane {TopicPartition} failed in the {LoopName} loop. " +
            "Stopping Log Processor so uncommitted Kafka messages can be retried.",
            TopicPartition,
            loopName);

        _applicationLifetime.StopApplication();
    }
    private void MarkProcessed(
        BatchProcessResult result)
    {
        if (result.ProcessedOffsets.Count == 0)
        {
            return;
        }

        _offsetCoordinator.MarkProcessed(
            result.ProcessedOffsets);

        _offsetCommitSignal.Signal();
    }

    public async Task StopAsync(
        CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();

        if (_readTask is not null)
        {
            await _readTask.WaitAsync(
                cancellationToken);
        }

        _runCts.Cancel();

        if (_flushTask is not null)
        {
            await _flushTask.WaitAsync(
                cancellationToken);
        }

        var result = await _batchProcessor.FlushAsync(
            cancellationToken);

        MarkProcessed(result);
    }

    public async ValueTask DisposeAsync()
    {
        _runCts.Cancel();
        _runCts.Dispose();

        if (_batchProcessor is IDisposable disposable)
        {
            disposable.Dispose();
        }

        await ValueTask.CompletedTask;
    }
}
