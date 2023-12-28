using System.Runtime.CompilerServices;

namespace AsyncKeyedLockBenchmarks;

public sealed class ConditionalWeakTableTest<TKey> where TKey : class
{
    private readonly ConditionalWeakTable<TKey, SemaphoreSlim> _semaphores = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask<IDisposable> LockAsync(TKey key)
    {
        var semaphoreSlim = _semaphores.GetValue(key, _ => new SemaphoreSlim(1));
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        return new Releaser { SemaphoreSlim = semaphoreSlim };
    }

    private struct Releaser : IDisposable
    {
        public SemaphoreSlim SemaphoreSlim { get; set; }

        public readonly void Dispose()
        {
            SemaphoreSlim.Release();
        }
    }
}
