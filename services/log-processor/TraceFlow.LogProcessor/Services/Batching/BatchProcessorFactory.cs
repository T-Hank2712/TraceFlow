namespace TraceFlow.LogProcessor.Services.Batching;

public sealed class BatchProcessorFactory
    : IBatchProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public BatchProcessorFactory(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IBatchProcessor Create()
    {
        return _serviceProvider.GetRequiredService<IBatchProcessor>();
    }
}
