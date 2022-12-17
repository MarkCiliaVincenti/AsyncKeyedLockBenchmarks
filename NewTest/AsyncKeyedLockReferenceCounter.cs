using System;
using System.Threading;

namespace NewTest
{
    /// <summary>
    /// The AsyncKeyedLock ReferenceCounter class
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public sealed class AsyncKeyedLockReferenceCounter<TKey> : IDisposable
    {
        private readonly TKey _key;
        /// <summary>
        /// The key
        /// </summary>
        public TKey Key => _key;

        /// <summary>
        /// The reference count
        /// </summary>
        public volatile int ReferenceCount = 1;

        private readonly SemaphoreSlim _semaphoreSlim;
        
        /// <summary>
        /// The SemaphoreSlim object.
        /// </summary>
        public SemaphoreSlim SemaphoreSlim => _semaphoreSlim;

        private readonly AsyncKeyedLockerDictionary<TKey> _dictionary;

        /// <summary>
        /// Constructor for AsyncKeyedLockReferenceCounter
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="semaphoreSlim">The SemaphoreSlim object</param>
        /// <param name="dictionary">The dictionary</param>
        public AsyncKeyedLockReferenceCounter(TKey key, SemaphoreSlim semaphoreSlim, AsyncKeyedLockerDictionary<TKey> dictionary)
        {
            _key = key;
            _semaphoreSlim = semaphoreSlim;
            _dictionary = dictionary;
        }

        /// <summary>
        /// Dispose and release.
        /// </summary>     
        public void Dispose()
        {
            _dictionary.Release(this);
        }
    }
}
