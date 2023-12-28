using System.Runtime.CompilerServices;

namespace AsyncKeyedLockBenchmarks;

public sealed class ConditionalWeakTableTest
{
    private static readonly ConditionalWeakTable<object, SemaphoreSlim> SemaphoreSlims
                          = new();

    private SemaphoreSlim GetOrCreate(object key)
    {
        return SemaphoreSlims.GetValue(key, _ => new SemaphoreSlim(1));
    }

    public IDisposable Lock(object key)
    {
        var semaphoreSlim = GetOrCreate(key);
        semaphoreSlim.Wait();
        return new Releaser { SemaphoreSlim = semaphoreSlim };
    }

    public async Task<IDisposable> LockAsync(object key)
    {
        var semaphoreSlim = GetOrCreate(key);
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        return new Releaser { SemaphoreSlim = semaphoreSlim };
    }

    private sealed class Releaser : IDisposable
    {
        public SemaphoreSlim SemaphoreSlim { get; set; }

        public void Dispose()
        {
            SemaphoreSlim.Release();
        }
    }
}
