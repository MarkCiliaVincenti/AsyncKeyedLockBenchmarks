using System.Collections.Concurrent;

namespace AsyncKeyedLockBenchmarks
{
    public class TheodorZoulias
    {
        private readonly ConcurrentDictionary<object, Entry> _semaphores = new();

        private readonly record struct Entry(SemaphoreSlim Semaphore, int RefCount);

        public readonly struct Releaser : IDisposable
        {
            private readonly TheodorZoulias _parent;
            private readonly object _key;
            public Releaser(TheodorZoulias parent, object key)
            {
                _parent = parent; _key = key;
            }
            public void Dispose() => _parent.Release(_key);
        }

        public async ValueTask<Releaser> LockAsync(object key)
        {
            Entry entry = _semaphores.AddOrUpdate(key,
                static _ => new Entry(new SemaphoreSlim(1, 1), 1),
                static (_, entry) => entry with { RefCount = entry.RefCount + 1 });

            await entry.Semaphore.WaitAsync().ConfigureAwait(false);
            return new Releaser(this, key);
        }

        private void Release(object key)
        {
            Entry entry;
            while (true)
            {
                bool exists = _semaphores.TryGetValue(key, out entry);
                if (!exists)
                    throw new InvalidOperationException("Key not found.");
                if (entry.RefCount > 1)
                {
                    Entry newEntry = entry with { RefCount = entry.RefCount - 1 };
                    if (_semaphores.TryUpdate(key, newEntry, entry))
                        break;
                }
                else
                {
                    if (_semaphores.TryRemove(KeyValuePair.Create(key, entry)))
                        break;
                }
            }
            entry.Semaphore.Release();
        }
    }
}
