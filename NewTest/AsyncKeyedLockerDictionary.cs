using System.Collections.Concurrent;
using System.Threading;

namespace NewTest
{
    /// <summary>
    /// AsyncKeyedLockerDictionary class
    /// </summary>
    /// <typeparam name="TKey">The type for the dictionary key</typeparam>
    public sealed class AsyncKeyedLockerDictionary<TKey> : ConcurrentDictionary<TKey, AsyncKeyedLockReferenceCounter<TKey>>
    {
        private readonly int _maxCount = 1;

        /// <summary>
        /// Constructor for AsyncKeyedLockerDictionary
        /// </summary>
        public AsyncKeyedLockerDictionary() : base()
        {
        }

        /// <summary>
        /// Constructor for AsyncKeyedLockerDictionary
        /// </summary>
        /// <param name="maxCount">The number of semaphore counts to allow.</param>
        public AsyncKeyedLockerDictionary(int maxCount) : base()
        {
            _maxCount = maxCount;
        }

        /// <summary>
        /// Constructor for AsyncKeyedLockerDictionary
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AsyncKeyedLockerDictionary{TKey}"/> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="AsyncKeyedLockerDictionary{TKey}"/> can contain.</param>
        public AsyncKeyedLockerDictionary(int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
        }

        /// <summary>
        /// Constructor for AsyncKeyedLockerDictionary
        /// </summary>
        /// <param name="maxCount">The number of semaphore counts to allow.</param>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AsyncKeyedLockerDictionary{TKey}"/> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="AsyncKeyedLockerDictionary{TKey}"/> can contain.</param>
        public AsyncKeyedLockerDictionary(int maxCount, int concurrencyLevel, int capacity) : base(concurrencyLevel, capacity)
        {
            _maxCount = maxCount;
        }

        /// <summary>
        /// Provider for AsyncKeyedLockReferenceCounter
        /// </summary>
        /// <param name="key">The key for which a reference counter should be obtained.</param>
        /// <returns>A created or retrieved reference counter</returns>
        public AsyncKeyedLockReferenceCounter<TKey> GetOrAdd(TKey key)
        {
            /*
            return AddOrUpdate(key, x => new AsyncKeyedLockReferenceCounter<TKey>(key, new SemaphoreSlim(_maxCount), this),
                (k, v) =>
                {
                    Monitor.Enter(v);
                    if (v.ReferenceCount > 0)
                    {
                        v.ReferenceCount++;
                        Monitor.Exit(v);
                        return v;
                    }
                    return new AsyncKeyedLockReferenceCounter<TKey>(key, new SemaphoreSlim(_maxCount), this);
                });
            */


            if (TryGetValue(key, out var firstReferenceCounter))
            {
                Monitor.Enter(firstReferenceCounter);
                if (firstReferenceCounter.ReferenceCount > 0)
                {
                    firstReferenceCounter.ReferenceCount++;
                    Monitor.Exit(firstReferenceCounter);
                    return firstReferenceCounter;
                }
                Monitor.Exit(firstReferenceCounter);
            }

            firstReferenceCounter = new AsyncKeyedLockReferenceCounter<TKey>(key, new SemaphoreSlim(_maxCount), this);
            if (TryAdd(key, firstReferenceCounter))
            {
                return firstReferenceCounter;
            }

            while (true)
            {
                if (TryGetValue(key, out var referenceCounter))
                {
                    Monitor.Enter(referenceCounter);
                    if (referenceCounter.ReferenceCount > 0)
                    {
                        referenceCounter.ReferenceCount++;
                        Monitor.Exit(referenceCounter);
                        return referenceCounter;
                    }
                    Monitor.Exit(referenceCounter);
                }

                if (TryAdd(key, firstReferenceCounter))
                {
                    return firstReferenceCounter;
                }
            }
        }

        /// <summary>
        /// Dispose and release.
        /// </summary>
        /// <param name="referenceCounter">The reference counter instance.</param>
        public void Release(AsyncKeyedLockReferenceCounter<TKey> referenceCounter)
        {
            Monitor.Enter(referenceCounter);

            var remainingConsumers = --referenceCounter.ReferenceCount;

            if (remainingConsumers == 0)
            {
                TryRemove(referenceCounter.Key, out _);
            }

            Monitor.Exit(referenceCounter);

            referenceCounter.SemaphoreSlim.Release();
        }
    }
}
