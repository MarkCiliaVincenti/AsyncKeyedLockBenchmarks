using AsyncKeyedLock;
using AsyncUtilities;
using BenchmarkDotNet.Attributes;
using KeyedSemaphores;

namespace AsyncKeyedLockBenchmarks
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        [Params(100, 1000, 5000)] public int NumberOfLocks { get; set; }

        [Params(1, 10, 100)] public int Contention { get; set; }

        #region AsyncKeyedLockerNonGenerics
        public AsyncKeyedLocker? AsyncKeyedLockerNonGenerics { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerNonGenericsTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLockNonGenerics))]
        public void SetupAsyncKeyedLockNonGenerics()
        {
            AsyncKeyedLockerNonGenerics = new AsyncKeyedLocker();
            AsyncKeyedLockerNonGenericsTasks = Enumerable.Range(0, Contention * NumberOfLocks)
                .AsParallel()
                .Select(async i =>
                {
                    var key = i % NumberOfLocks;

                    using (var myLock = await AsyncKeyedLockerNonGenerics.LockAsync(key))
                    {
                        await Task.Delay(10);
                    }

                    await Task.Yield();
                });
        }

        [IterationCleanup(Target = nameof(AsyncKeyedLockNonGenerics))]
        public void CleanupAsyncKeyedLockNonGenerics()
        {
            AsyncKeyedLockerNonGenerics = null;
            AsyncKeyedLockerNonGenericsTasks = null;
        }

        [Benchmark]
        public async Task AsyncKeyedLockNonGenerics()
        {
            var asyncKeyedLockerNonGenerics = AsyncKeyedLockerNonGenerics;
            var tasks = Enumerable.Range(0, Contention * NumberOfLocks)
                .AsParallel()
                .Select(async i =>
                {
                    var key = i % NumberOfLocks;

                    using (var myLock = await asyncKeyedLockerNonGenerics.LockAsync(key))
                    {
                        await Task.Delay(10);
                    }

                    await Task.Yield();
                });

            await Task.WhenAll(tasks);
        }
        #endregion AsyncKeyedLockNonGenerics

        #region AsyncKeyedLock
        public AsyncKeyedLocker<int>? AsyncKeyedLocker { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLock))]
        public void SetupAsyncKeyedLock()
        {
            AsyncKeyedLocker = new AsyncKeyedLocker<int>();
            AsyncKeyedLockerTasks = Enumerable.Range(0, Contention * NumberOfLocks)
                .AsParallel()
                .Select(async i =>
                {
                    var key = i % NumberOfLocks;

                    using (var myLock = await AsyncKeyedLocker.LockAsync(key))
                    {
                        await Task.Delay(10);
                    }

                    await Task.Yield();
                });
        }

        [IterationCleanup(Target = nameof(AsyncKeyedLock))]
        public void CleanupAsyncKeyedLock()
        {
            AsyncKeyedLocker = null;
            AsyncKeyedLockerTasks = null;
        }

        [Benchmark(Baseline = true)]
        public async Task AsyncKeyedLock()
        {
            await Task.WhenAll(AsyncKeyedLockerTasks);
        }
        #endregion AsyncKeyedLock

        #region KeyedSemaphores
        public KeyedSemaphoresCollection<int>? KeyedSemaphoreCollection { get; set; }
        public ParallelQuery<Task>? KeyedSemaphoreTasks { get; set; }

        [IterationSetup(Target = nameof(KeyedSemaphores))]
        public void SetupKeySemaphores()
        {
            KeyedSemaphoreCollection = new KeyedSemaphoresCollection<int>();
            KeyedSemaphoreTasks = Enumerable.Range(0, Contention * NumberOfLocks)
                .AsParallel()
                .Select(async i =>
                {
                    var key = i % NumberOfLocks;

                    using (var myLock = await KeyedSemaphoreCollection.LockAsync(key))
                    {
                        await Task.Delay(10);
                    }

                    await Task.Yield();
                });
        }

        [IterationCleanup(Target = nameof(KeyedSemaphores))]
        public void CleanupKeySemaphores()
        {
            KeyedSemaphoreCollection = null;
            KeyedSemaphoreTasks = null;
        }

        [Benchmark]
        public async Task KeyedSemaphores()
        {
            await Task.WhenAll(KeyedSemaphoreTasks);
        }
        #endregion KeyedSemaphores

        #region StripedAsyncLock
        public StripedAsyncLock<int>? StripedAsyncLocker { get; set; }
        public ParallelQuery<Task>? StripedAsyncLockerTasks { get; set; }

        [IterationSetup(Target = nameof(StripedAsyncLock))]
        public void SetupStripedAsyncLock()
        {
            StripedAsyncLocker = new StripedAsyncLock<int>(NumberOfLocks);
            StripedAsyncLockerTasks = Enumerable.Range(0, Contention * NumberOfLocks)
                .AsParallel()
                .Select(async i =>
                {
                    var key = i % NumberOfLocks;

                    using (var myLock = await StripedAsyncLocker.LockAsync(key))
                    {
                        await Task.Delay(10);
                    }

                    await Task.Yield();
                });
        }

        [IterationCleanup(Target = nameof(StripedAsyncLock))]
        public void CleanupStripedAsyncLock()
        {
            StripedAsyncLocker = null;
            StripedAsyncLockerTasks = null;
        }

        [Benchmark]
        public async Task StripedAsyncLock()
        {
            await Task.WhenAll(StripedAsyncLockerTasks);
        }
        #endregion StripedAsyncLock
    }
}
