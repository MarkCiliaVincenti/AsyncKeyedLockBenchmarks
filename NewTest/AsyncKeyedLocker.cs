﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace NewTest
{
    /// <summary>
    /// AsyncKeyedLock class, inspired by <see href="https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647">Stephen Cleary's solution</see>.
    /// </summary>
    public sealed class AsyncKeyedLocker : AsyncKeyedLocker<object>
    {
    }

    /// <summary>
    /// AsyncKeyedLock class, adapted and improved from <see href="https://stackoverflow.com/questions/31138179/asynchronous-locking-based-on-a-key/31194647#31194647">Stephen Cleary's solution</see>.
    /// </summary>
    public class AsyncKeyedLocker<TKey>
    {
        private readonly AsyncKeyedLockerDictionary<TKey> _dictionary;

        /// <summary>
        /// The dictionary of SemaphoreSlim objects.
        /// </summary>
        public AsyncKeyedLockerDictionary<TKey> SemaphoreSlims => _dictionary;

        /// <summary>
        /// The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.
        /// </summary>
        public int MaxCount { get; internal set; } = 1;

        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        public AsyncKeyedLocker()
        {
            _dictionary = new AsyncKeyedLockerDictionary<TKey>();
        }

        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.</param>
        public AsyncKeyedLocker(int maxCount)
        {
            MaxCount = maxCount;
            _dictionary = new AsyncKeyedLockerDictionary<TKey>(maxCount);
        }

        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AsyncKeyedLockerDictionary{TKey}"/> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="AsyncKeyedLockerDictionary{TKey}"/> can contain.</param>
        public AsyncKeyedLocker(int concurrencyLevel, int capacity)
        {
            _dictionary = new AsyncKeyedLockerDictionary<TKey>(concurrencyLevel, capacity);
        }

        /// <summary>
        /// Constructor for AsyncKeyedLock.
        /// </summary>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently. Defaults to 1.</param>
        /// <param name="concurrencyLevel">The estimated number of threads that will update the <see cref="AsyncKeyedLockerDictionary{TKey}"/> concurrently.</param>
        /// <param name="capacity">The initial number of elements that the <see cref="AsyncKeyedLockerDictionary{TKey}"/> can contain.</param>
        public AsyncKeyedLocker(int maxCount, int concurrencyLevel, int capacity)
        {
            MaxCount = maxCount;
            _dictionary = new AsyncKeyedLockerDictionary<TKey>(concurrencyLevel, capacity);
        }

        #region Synchronous
        /// <summary>
        /// Synchronously lock based on a key.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            referenceCounter.SemaphoreSlim.Wait();
            return referenceCounter;
        }

        /// <summary>
        /// Synchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                referenceCounter.SemaphoreSlim.Wait(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(referenceCounter);
                throw;
            }
            return referenceCounter;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, int millisecondsTimeout)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            referenceCounter.SemaphoreSlim.Wait(millisecondsTimeout);
            return referenceCounter;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="success">False if timed out, true if it successfully entered.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, int millisecondsTimeout, out bool success)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            success = referenceCounter.SemaphoreSlim.Wait(millisecondsTimeout);
            return referenceCounter;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, TimeSpan timeout)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            referenceCounter.SemaphoreSlim.Wait(timeout);
            return referenceCounter;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="success">False if timed out, true if it successfully entered.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, TimeSpan timeout, out bool success)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            success = referenceCounter.SemaphoreSlim.Wait(timeout);
            return referenceCounter;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                referenceCounter.SemaphoreSlim.Wait(millisecondsTimeout, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(referenceCounter);
                throw;
            }
            return referenceCounter;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="success">False if timed out, true if it successfully entered.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, int millisecondsTimeout, CancellationToken cancellationToken, out bool success)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                success = referenceCounter.SemaphoreSlim.Wait(millisecondsTimeout, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(referenceCounter);
                throw;
            }
            return referenceCounter;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                referenceCounter.SemaphoreSlim.Wait(timeout, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(referenceCounter);
                throw;
            }
            return referenceCounter;
        }

        /// <summary>
        /// Synchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <param name="success">False if timed out, true if it successfully entered.</param>
        /// <returns>A disposable value.</returns>
        public IDisposable Lock(TKey key, TimeSpan timeout, CancellationToken cancellationToken, out bool success)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                success = referenceCounter.SemaphoreSlim.Wait(timeout, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(referenceCounter);
                throw;
            }
            return referenceCounter;
        }
        #endregion Synchronous

        #region AsynchronousTry
        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async Task<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout)
        {
            var refCounter = SemaphoreSlims.GetOrAdd(key);
            if (!await refCounter.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(false))
            {
                SemaphoreSlims.Release(refCounter);
                return false;
            }

            try
            {
                action();
            }
            catch
            {
                throw;
            }
            finally
            {
                SemaphoreSlims.Release(refCounter);
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async Task<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout)
        {
            var refCounter = SemaphoreSlims.GetOrAdd(key);
            if (!await refCounter.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(false))
            {
                SemaphoreSlims.Release(refCounter);
                return false;
            }

            try
            {
                await task().ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
            finally
            {
                SemaphoreSlims.Release(refCounter);
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async Task<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout)
        {
            var refCounter = SemaphoreSlims.GetOrAdd(key);
            if (!await refCounter.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(false))
            {
                SemaphoreSlims.Release(refCounter);
                return false;
            }

            try
            {
                action();
            }
            catch
            {
                throw;
            }
            finally
            {
                SemaphoreSlims.Release(refCounter);
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async Task<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout)
        {
            var refCounter = SemaphoreSlims.GetOrAdd(key);
            if (!await refCounter.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(false))
            {
                SemaphoreSlims.Release(refCounter);
                return false;
            }

            try
            {
                await task().ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
            finally
            {
                SemaphoreSlims.Release(refCounter);
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async Task<bool> TryLockAsync(TKey key, Action action, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var refCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                if (!await refCounter.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(false))
                {
                    SemaphoreSlims.Release(refCounter);
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(refCounter);
                throw;
            }

            try
            {
                action();
            }
            catch
            {
                throw;
            }
            finally
            {
                SemaphoreSlims.Release(refCounter);
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async Task<bool> TryLockAsync(TKey key, Func<Task> task, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var refCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                if (!await refCounter.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(false))
                {
                    SemaphoreSlims.Release(refCounter);
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(refCounter);
                throw;
            }

            try
            {
                await task().ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
            finally
            {
                SemaphoreSlims.Release(refCounter);
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="action">The synchronous action.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async Task<bool> TryLockAsync(TKey key, Action action, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var refCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                if (!await refCounter.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(false))
                {
                    SemaphoreSlims.Release(refCounter);
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(refCounter);
                throw;
            }

            try
            {
                action();
            }
            catch
            {
                throw;
            }
            finally
            {
                SemaphoreSlims.Release(refCounter);
            }
            return true;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, and if not timed out, scynchronously execute an action and release, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="task">The asynchronous task.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>False if timed out, true if it successfully entered.</returns>
        public async Task<bool> TryLockAsync(TKey key, Func<Task> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var refCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                if (!await refCounter.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(false))
                {
                    SemaphoreSlims.Release(refCounter);
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(refCounter);
                throw;
            }

            try
            {
                await task().ConfigureAwait(false);
            }
            catch
            {
                throw;
            }
            finally
            {
                SemaphoreSlims.Release(refCounter);
            }
            return true;
        }
        #endregion AsynchronousTry

        #region Asynchronous
        /// <summary>
        /// Asynchronously lock based on a key.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync().ConfigureAwait(false);
            return referenceCounter;
        }
        
        /// <summary>
        /// Asynchronously lock based on a key, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                await referenceCounter.SemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(referenceCounter);
                throw;
            }
            return referenceCounter;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, int millisecondsTimeout)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync(millisecondsTimeout).ConfigureAwait(false);
            return referenceCounter;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="TimeSpan"/> to wait.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, TimeSpan timeout)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            await referenceCounter.SemaphoreSlim.WaitAsync(timeout).ConfigureAwait(false);
            return referenceCounter;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the number of milliseconds to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, <see cref="Timeout.Infinite"/> (-1) to wait indefinitely, or zero to test the state of the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                await referenceCounter.SemaphoreSlim.WaitAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(referenceCounter);
                throw;
            }
            return referenceCounter;
        }

        /// <summary>
        /// Asynchronously lock based on a key, setting a limit for the <see cref="System.TimeSpan"/> to wait, while observing a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="key">The key to lock on.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the number of milliseconds to wait, a <see cref="TimeSpan"/> that represents -1 milliseconds to wait indefinitely, or a <see cref="TimeSpan"/> that represents 0 milliseconds to test the wait handle and return immediately.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A disposable value.</returns>
        public async Task<IDisposable> LockAsync(TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var referenceCounter = SemaphoreSlims.GetOrAdd(key);
            try
            {
                await referenceCounter.SemaphoreSlim.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                SemaphoreSlims.Release(referenceCounter);
                throw;
            }
            return referenceCounter;
        }
        #endregion

        /// <summary>
        /// Checks whether or not there is a thread making use of a keyed lock.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is in use; otherwise, false.</returns>
        public bool IsInUse(TKey key)
        {
            return SemaphoreSlims.ContainsKey(key);
        }

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests.</returns>
        [Obsolete("This method should not longer be used as it is confusing with Semaphore terminology. Use <see cref=\"GetCurrentCount\"/> or <see cref=\"GetRemaningCount\"/> instead depending what you want to do.")]
        public int GetCount(TKey key)
        {
            return GetRemainingCount(key);
        }

        /// <summary>
        /// Get the number of requests concurrently locked for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of requests concurrently locked for a given key.</returns>
        public int GetRemainingCount(TKey key)
        {
            if (SemaphoreSlims.TryGetValue(key, out var referenceCounter))
            {
                return referenceCounter.ReferenceCount;
            }
            return 0;
        }

        /// <summary>
        /// Get the number of remaining threads that can enter the lock for a given key.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns>The number of remaining threads that can enter the lock for a given key.</returns>
        public int GetCurrentCount(TKey key)
        {
            return MaxCount - GetRemainingCount(key);
        }

        /// <summary>
        /// Forces requests to be released from the semaphore.
        /// </summary>
        /// <param name="key">The key requests are locked on.</param>
        /// <returns><see langword="true"/> if the key is successfully found and removed; otherwise, false.</returns>
        public bool ForceRelease(TKey key)
        {
            if (SemaphoreSlims.TryGetValue(key, out var referenceCounter))
            {
                referenceCounter.SemaphoreSlim.Release(referenceCounter.ReferenceCount);
                return SemaphoreSlims.TryRemove(key, out _);
            }
            return false;
        }
    }
}
