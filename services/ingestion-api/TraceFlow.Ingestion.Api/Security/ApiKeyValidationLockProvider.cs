using System.Collections.Concurrent;

namespace TraceFlow.Ingestion.Api.Security;
public sealed class ApiKeyValidationLockProvider
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    public SemaphoreSlim GetLock(string apiKeyFingerprint)
    {
        return _locks.GetOrAdd(apiKeyFingerprint, _ => new SemaphoreSlim(1,1));
    }
    public void ReleaseLockIfIdle(string apiKeyFingerprint, SemaphoreSlim semaphore)
    {
        if (semaphore.CurrentCount == 1)
        {
            _locks.TryRemove(
                new KeyValuePair<string, SemaphoreSlim>(apiKeyFingerprint, semaphore)
            );
        }
    }
}