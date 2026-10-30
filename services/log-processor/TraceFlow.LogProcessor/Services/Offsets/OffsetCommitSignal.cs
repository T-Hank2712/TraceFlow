namespace TraceFlow.LogProcessor.Services.Offsets;

public sealed class OffsetCommitSignal : IOffsetCommitSignal
{
    private int _signaled;

    public void Signal()
    {
        Interlocked.Exchange(ref _signaled, 1);
    }

    public bool IsSignaled()
    {
        return Interlocked.Exchange(
            ref _signaled,
            0) == 1;
    }
}
