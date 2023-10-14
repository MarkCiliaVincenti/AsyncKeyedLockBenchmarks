using AsyncKeyedLock;
using AsyncUtilities;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using Firebend.AutoCrud.Core.Threading;
using KeyedSemaphores;
using ListShuffle;

namespace AsyncKeyedLockBenchmarks
{
    //[Config(typeof(Config))]
    [Config(typeof(MemoryConfig))]
    [MemoryDiagnoser]
    [JsonExporterAttribute.Full]
    [JsonExporterAttribute.FullCompressed]
    public class Benchmarks
    {
        private class MemoryConfig : ManualConfig
        {
            public MemoryConfig()
            {
                AddDiagnoser(MemoryDiagnoser.Default);
            }
        }

        //private class Config : ManualConfig
        //{
        //    public Config()
        //    {
        //        var baseJob = Job.Default;

        //        AddJob(baseJob.WithNuGet("AsyncKeyedLock", "6.1.0").WithBaseline(true));
        //        AddJob(baseJob.WithNuGet("AsyncKeyedLock", "6.1.1-rc"));
        //    }
        //}

        [Params(200, 10_000)] public int NumberOfLocks { get; set; }

        [Params(100, 10_000)] public int Contention { get; set; }

        [Params(0, 1, 5)] public int GuidReversals { get; set; }

        private readonly Dictionary<int, List<int>> _shuffledIntegers = new();

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

        private void Operation()
        {
            for (int i = 0; i < GuidReversals; i++)
            {
                Guid guid = Guid.NewGuid();
                var guidString = guid.ToString();
                guidString = guidString.Reverse().ToString();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (guidString.Length != 53)
                {
                    throw new Exception($"Not 53 but {guidString?.Length}");
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }

        private async Task RunTests(ParallelQuery<Task> tasks)
        {
            if (NumberOfLocks != Contention)
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        #region AsyncKeyedLock
        public AsyncKeyedLocker<int>? AsyncKeyedLocker { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLock))]
        public void SetupAsyncKeyedLock()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyedLocker = new AsyncKeyedLocker<int>(o =>
                {
                    o.PoolSize = NumberOfLocks;
                    o.PoolInitialFill = Environment.ProcessorCount * 2;
                }, Environment.ProcessorCount, NumberOfLocks);
                AsyncKeyedLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncKeyedLocker.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
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

        [Benchmark(Baseline = true)]
        public async Task AsyncKeyedLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyedLockerTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyedLock

        #region AsyncKeyedLockNoPooling
        public AsyncKeyedLocker<int>? AsyncKeyedLockerNoPooling { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerNoPoolingTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLockNoPooling))]
        public void SetupAsyncKeyedLockNoPooling()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyedLockerNoPooling = new AsyncKeyedLocker<int>(o =>
                { }, Environment.ProcessorCount, NumberOfLocks);
                AsyncKeyedLockerNoPoolingTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncKeyedLockerNoPooling.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(AsyncKeyedLockNoPooling))]
        public void CleanupAsyncKeyedLockNoPooling()
        {
            AsyncKeyedLockerNoPooling = null;
            AsyncKeyedLockerNoPoolingTasks = null;
        }

        public async Task AsyncKeyedLockNoPooling()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyedLockerNoPoolingTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyedLockNoPooling

        #region StripedAsyncKeyedLocker
        public StripedAsyncKeyedLocker<int>? StripedAsyncKeyedLockerCollection { get; set; }
        public ParallelQuery<Task>? StripedAsyncKeyedLockerTasks { get; set; }

        [IterationSetup(Target = nameof(StripedAsyncKeyedLock))]
        public void SetupStripedAsyncKeyedLock()
        {
            if (NumberOfLocks != Contention)
            {
                StripedAsyncKeyedLockerCollection = new StripedAsyncKeyedLocker<int>(NumberOfLocks, 1);
                StripedAsyncKeyedLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await StripedAsyncKeyedLockerCollection.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(StripedAsyncKeyedLock))]
        public void CleanupStripedAsyncKeyedLocker()
        {
            StripedAsyncKeyedLockerCollection = null;
            StripedAsyncKeyedLockerTasks = null;
        }

        [Benchmark]
        public async Task StripedAsyncKeyedLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(StripedAsyncKeyedLockerTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion StripedAsyncKeyedLocker

        #region AsyncKeyLockFromImageSharpWeb
        public SixLabors.ImageSharp.Web.Synchronization.AsyncKeyLock<int>? AsyncKeyLockerFromImageSharpWeb { get; set; }
        public ParallelQuery<Task>? AsyncKeyLockerFromImageSharpWebTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyLockFromImageSharpWeb))]
        public void SetupAsyncKeyLockFromImageSharpWeb()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyLockerFromImageSharpWeb = new SixLabors.ImageSharp.Web.Synchronization.AsyncKeyLock<int>(NumberOfLocks);
                AsyncKeyLockerFromImageSharpWebTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncKeyLockerFromImageSharpWeb.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(AsyncKeyLockFromImageSharpWeb))]
        public void CleanupAsyncKeyLockFromImageSharpWeb()
        {
            AsyncKeyLockerFromImageSharpWeb = null;
            AsyncKeyLockerFromImageSharpWebTasks = null;
        }

        [Benchmark]
        public async Task AsyncKeyLockFromImageSharpWeb()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyLockerFromImageSharpWebTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyLockFromImageSharpWeb

        #region AsyncKeyLock
        public AsyncKeyLock.AsyncLock<int>? AsyncKeyLocker { get; set; }
        public ParallelQuery<Task>? AsyncKeyLockerTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyLock))]
        public void SetupAsyncKeyLock()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyLocker = new AsyncKeyLock.AsyncLock<int>(NumberOfLocks);
                AsyncKeyLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncKeyLocker.WriterLockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(AsyncKeyLock))]
        public void CleanupAsyncKeyLock()
        {
            AsyncKeyLocker = null;
            AsyncKeyLockerTasks = null;
        }

        [Benchmark]
        public async Task AsyncKeyLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyLockerTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyLock

        #region KeyedSemaphores
        public KeyedSemaphoresCollection<int>? KeyedSemaphoresCollection { get; set; }
        public ParallelQuery<Task>? KeyedSemaphoresTasks { get; set; }

        [IterationSetup(Target = nameof(KeyedSemaphores))]
        public void SetupKeyedSemaphores()
        {
            if (NumberOfLocks != Contention)
            {
                KeyedSemaphoresCollection = new KeyedSemaphoresCollection<int>(NumberOfLocks);
                KeyedSemaphoresTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await KeyedSemaphoresCollection.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(KeyedSemaphores))]
        public void CleanupKeyedSemaphores()
        {
            KeyedSemaphoresCollection = null;
            KeyedSemaphoresTasks = null;
        }

        [Benchmark]
        public async Task KeyedSemaphores()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(KeyedSemaphoresTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion KeyedSemaphores

        #region KeyedSemaphoresDictionary
        public KeyedSemaphoresDictionary<int>? KeyedSemaphoresDictionaryDictionary { get; set; }
        public ParallelQuery<Task>? KeyedSemaphoresDictionaryTasks { get; set; }

        [IterationSetup(Target = nameof(KeyedSemaphoresDictionary))]
        public void SetupKeyedSemaphoresDictionary()
        {
            if (NumberOfLocks != Contention)
            {
                KeyedSemaphoresDictionaryDictionary = new KeyedSemaphoresDictionary<int>(Environment.ProcessorCount, NumberOfLocks, EqualityComparer<int>.Default, TimeSpan.FromMilliseconds(10));
                KeyedSemaphoresDictionaryTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await KeyedSemaphoresDictionaryDictionary.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(KeyedSemaphoresDictionary))]
        public void CleanupKeyedSemaphoresDictionary()
        {
            KeyedSemaphoresDictionaryDictionary = null;
            KeyedSemaphoresDictionaryTasks = null;
        }

        [Benchmark]
        public async Task KeyedSemaphoresDictionary()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(KeyedSemaphoresDictionaryTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion KeyedSemaphores

        #region AsyncDuplicateLock
        public ParallelQuery<Task>? AsyncDuplicateLockTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncDuplicateLockFromAutoCrud))]
        public void SetupAsyncDuplicateLock()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncDuplicateLockTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncDuplicateLock.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(AsyncDuplicateLockFromAutoCrud))]
        public void CleanupAsyncDuplicateLock()
        {
            AsyncDuplicateLockTasks = null;
        }

        [Benchmark]
        public async Task AsyncDuplicateLockFromAutoCrud()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncDuplicateLockTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyLock

        #region StripedAsyncLock
        public StripedAsyncLock<int>? StripedAsyncLocker { get; set; }
        public ParallelQuery<Task>? StripedAsyncLockTasks { get; set; }

        [IterationSetup(Target = nameof(StripedAsyncLock))]
        public void SetupStripedAsyncLock()
        {
            if (NumberOfLocks != Contention)
            {
                StripedAsyncLocker = new StripedAsyncLock<int>(NumberOfLocks);
                StripedAsyncLockTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await StripedAsyncLocker.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(StripedAsyncLock))]
        public void CleanupStripedAsyncLock()
        {
            StripedAsyncLocker = null;
            StripedAsyncLockTasks = null;
        }

        [Benchmark]
        public async Task StripedAsyncLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(StripedAsyncLockTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion StripedAsyncLock

    }
}
