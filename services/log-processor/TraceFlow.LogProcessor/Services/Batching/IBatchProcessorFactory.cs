namespace TraceFlow.LogProcessor.Services.Batching;

public interface IBatchProcessorFactory
{
    IBatchProcessor Create();
}