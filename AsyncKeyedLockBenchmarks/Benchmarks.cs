using AsyncKeyedLock.Temp;
using AsyncKeyedLock;
using AsyncUtilities;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using KeyedSemaphores;
using ListShuffle;
using NeoSmart.Synchronization;

namespace AsyncKeyedLockBenchmarks
{
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.Default;

                AddJob(baseJob.WithNuGet("AsyncKeyedLock", "5.1.1").WithBaseline(true));
                AddJob(baseJob.WithNuGet("AsyncKeyedLock", "5.1.2"));
            }
        }

        [Params(200, 10_000)] public int NumberOfLocks { get; set; }

        [Params(100, 10_000)] public int Contention { get; set; }

        [Params(0, 1, 5)] public int GuidReversals { get; set; }

        private Dictionary<int, List<int>> _shuffledIntegers = new();

        public List<int> ShuffledIntegers
        {
            get
            {
                if (!_shuffledIntegers.TryGetValue(Contention * NumberOfLocks, out var shuffledIntegers))
                {
                    shuffledIntegers = Enumerable.Range(0, Contention * NumberOfLocks).ToList();
                    shuffledIntegers.Shuffle();
                    _shuffledIntegers[Contention * NumberOfLocks] = shuffledIntegers;
                }
                return shuffledIntegers;
            }
        }

        private async Task Operation()
        {
            //Interlocked.Increment(ref _count);
            for (int i = 0; i < GuidReversals; i++)
            {
                Guid guid = Guid.NewGuid();
                var guidString = guid.ToString();
                guidString = guidString.Reverse().ToString();
                if (guidString.Length != 53)
                {
                    throw new Exception($"Not 53 but {guidString.Length}");
                }
            }
        }

        private async Task RunTests(ParallelQuery<Task> tasks)
        {
            if (NumberOfLocks != Contention)
            {
                //_count = 0;
                await Task.WhenAll(tasks).ConfigureAwait(false);
                //if (_count != NumberOfLocks * Contention) throw new Exception($"Count not as expected");
            }
        }

        #region AsyncKeyedLock
        public AsyncKeyedLock.AsyncKeyedLocker<string>? AsyncKeyedLocker { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLock))]
        public void SetupAsyncKeyedLock()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyedLocker = new AsyncKeyedLock.AsyncKeyedLocker<string>(new AsyncKeyedLock.AsyncKeyedLockOptions(poolSize: NumberOfLocks), Environment.ProcessorCount, NumberOfLocks);
                AsyncKeyedLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncKeyedLocker.LockAsync(key.ToString()).ConfigureAwait(false))
                        {
                            await Operation().ConfigureAwait(false);
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(AsyncKeyedLock))]
        public void CleanupAsyncKeyedLock()
        {
            AsyncKeyedLocker = null;
            AsyncKeyedLockerTasks = null;
        }

        //[Benchmark(Baseline = true)]
        [Benchmark]
        public async Task AsyncKeyedLock()
        {
            await RunTests(AsyncKeyedLockerTasks).ConfigureAwait(false);
        }
        #endregion AsyncKeyedLock

        #region AsyncKeyedLock.Temp
        public AsyncKeyedLock.Temp.AsyncKeyedLocker<string>? AsyncKeyedLockerTemp { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerTempTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLockTemp))]
        public void SetupAsyncKeyedLockTemp()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyedLockerTemp = new AsyncKeyedLock.Temp.AsyncKeyedLocker<string>(new AsyncKeyedLock.Temp.AsyncKeyedLockOptions(poolSize: NumberOfLocks), Environment.ProcessorCount, NumberOfLocks);
                AsyncKeyedLockerTempTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncKeyedLockerTemp.LockAsync(key.ToString()).ConfigureAwait(false))
                        {
                            await Operation().ConfigureAwait(false);
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(AsyncKeyedLockTemp))]
        public void CleanupAsyncKeyedLockTemp()
        {
            AsyncKeyedLockerTemp = null;
            AsyncKeyedLockerTempTasks = null;
        }

        //[Benchmark(Baseline = true)]
        //[Benchmark]
        public async Task AsyncKeyedLockTemp()
        {
            await RunTests(AsyncKeyedLockerTempTasks).ConfigureAwait(false);
        }
        #endregion AsyncKeyedLock

        #region AsyncKeyedLock.NonBlocking
        public AsyncKeyedLock.NonBlocking.AsyncKeyedLocker<string>? AsyncKeyedLockerNonBlocking { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerNonBlockingTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLockNonBlocking))]
        public void SetupAsyncKeyedLockNonBlocking()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyedLockerNonBlocking = new AsyncKeyedLock.NonBlocking.AsyncKeyedLocker<string>(new AsyncKeyedLock.NonBlocking.AsyncKeyedLockOptions(poolSize: NumberOfLocks), Environment.ProcessorCount, NumberOfLocks);
                AsyncKeyedLockerNonBlockingTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncKeyedLockerNonBlocking.LockAsync(key.ToString()).ConfigureAwait(false))
                        {
                            await Operation().ConfigureAwait(false);
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(AsyncKeyedLockNonBlocking))]
        public void CleanupAsyncKeyedLockNonBlocking()
        {
            AsyncKeyedLockerNonBlocking = null;
            AsyncKeyedLockerNonBlockingTasks = null;
        }

        //[Benchmark]
        public async Task AsyncKeyedLockNonBlocking()
        {
            await RunTests(AsyncKeyedLockerNonBlockingTasks).ConfigureAwait(false);
        }
        #endregion AsyncKeyedLock.NonBlocking

        #region KeyedSemaphores
        public KeyedSemaphoresCollection<int>? KeyedSemaphoreCollection { get; set; }
        public ParallelQuery<Task>? KeyedSemaphoreTasks { get; set; }

        [IterationSetup(Target = nameof(KeyedSemaphores))]
        public void SetupKeySemaphores()
        {
            if (NumberOfLocks != Contention)
            {
                KeyedSemaphoreCollection = new KeyedSemaphoresCollection<int>(NumberOfLocks, Environment.ProcessorCount);
                KeyedSemaphoreTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await KeyedSemaphoreCollection.LockAsync(key).ConfigureAwait(false))
                        {
                            await Operation().ConfigureAwait(false);
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(KeyedSemaphores))]
        public void CleanupKeySemaphores()
        {
            KeyedSemaphoreCollection = null;
            KeyedSemaphoreTasks = null;
        }

        //[Benchmark]
        public async Task KeyedSemaphores()
        {
            await RunTests(KeyedSemaphoreTasks).ConfigureAwait(false);
        }
        #endregion KeyedSemaphores

        #region StripedAsyncLock
        public StripedAsyncLock<int>? StripedAsyncLocker { get; set; }
        public ParallelQuery<Task>? StripedAsyncLockerTasks { get; set; }

        [IterationSetup(Target = nameof(StripedAsyncLock))]
        public void SetupStripedAsyncLock()
        {
            if (NumberOfLocks != Contention)
            {
                StripedAsyncLocker = new StripedAsyncLock<int>(NumberOfLocks);
                StripedAsyncLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await StripedAsyncLocker.LockAsync(key).ConfigureAwait(false))
                        {
                            await Operation().ConfigureAwait(false);
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(StripedAsyncLock))]
        public void CleanupStripedAsyncLock()
        {
            StripedAsyncLocker = null;
            StripedAsyncLockerTasks = null;
        }

        //[Benchmark]
        public async Task StripedAsyncLock()
        {
            await RunTests(StripedAsyncLockerTasks).ConfigureAwait(false);
        }
        #endregion StripedAsyncLock

        #region ScopedMutex
        public ParallelQuery<Task>? ScopedMutexTasks { get; set; }

        [IterationSetup(Target = nameof(ScopedMutexTest))]
        public void SetupScopedMutex()
        {
            if (NumberOfLocks != Contention)
            {
                ScopedMutexTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await ScopedMutex.CreateAsync(key.ToString()).ConfigureAwait(false))
                        {
                            await Operation().ConfigureAwait(false);
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(ScopedMutexTest))]
        public void CleanupScopedMutex()
        {
            ScopedMutexTasks = null;
        }

        //[Benchmark]
        public async Task ScopedMutexTest()
        {
            await RunTests(ScopedMutexTasks).ConfigureAwait(false);
        }
        #endregion ScopedMutex
    }
}
