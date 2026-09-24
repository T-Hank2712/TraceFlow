namespace TraceFlow.LogProcessor.Services.Offsets;

public interface IOffsetCommitSignal
{
    void Signal();

    bool IsSignaled();
}