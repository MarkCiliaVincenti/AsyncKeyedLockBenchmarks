﻿using AsyncKeyedLock;
using AsyncUtilities;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Dao.IndividualLock;
using KeyedSemaphores;
using ListShuffle;
using NeoSmart.Synchronization;
using Stl.Locking;

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

        //        AddJob(baseJob.WithNuGet("AsyncKeyedLock", "6.2.5").WithBaseline(true));
        //        AddJob(baseJob.WithNuGet("AsyncKeyedLock", "6.2.6-alpha"));
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
        public AsyncKeyedLocker<string>? AsyncKeyedLocker { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLock))]
        public void SetupAsyncKeyedLock()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyedLocker = new AsyncKeyedLocker<string>(o =>
                {
                    o.PoolSize = NumberOfLocks;
                    o.PoolInitialFill = Environment.ProcessorCount * 2;
                }, Environment.ProcessorCount, NumberOfLocks);
                AsyncKeyedLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

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

        [Benchmark(Baseline = true, Description = "AsyncKeyedLocker with pooling")]
        public async Task AsyncKeyedLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyedLockerTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyedLock

        #region AsyncKeyedLockNoPooling
        public AsyncKeyedLocker<string>? AsyncKeyedLockerNoPooling { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerNoPoolingTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLockNoPooling))]
        public void SetupAsyncKeyedLockNoPooling()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyedLockerNoPooling = new AsyncKeyedLocker<string>(o =>
                { o.PoolSize = 0; }, Environment.ProcessorCount, NumberOfLocks);
                AsyncKeyedLockerNoPoolingTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

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

        [Benchmark(Description = "AsyncKeyedLocker without pooling")]
        public async Task AsyncKeyedLockNoPooling()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyedLockerNoPoolingTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyedLockNoPooling

        #region StripedAsyncKeyedLocker
        public StripedAsyncKeyedLocker<string>? StripedAsyncKeyedLockerCollection { get; set; }
        public ParallelQuery<Task>? StripedAsyncKeyedLockerTasks { get; set; }

        [IterationSetup(Target = nameof(StripedAsyncKeyedLock))]
        public void SetupStripedAsyncKeyedLock()
        {
            if (NumberOfLocks != Contention)
            {
                StripedAsyncKeyedLockerCollection = new StripedAsyncKeyedLocker<string>(NumberOfLocks, 1);
                StripedAsyncKeyedLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

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

        [Benchmark(Description = "StripedAsyncKeyedLocker")]
        public async Task StripedAsyncKeyedLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(StripedAsyncKeyedLockerTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion StripedAsyncKeyedLocker

        #region AsyncKeyLockFromImageSharpWeb
        public SixLabors.ImageSharp.Web.Synchronization.AsyncKeyLock<string>? AsyncKeyLockerFromImageSharpWeb { get; set; }
        public ParallelQuery<Task>? AsyncKeyLockerFromImageSharpWebTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyLockFromImageSharpWeb))]
        public void SetupAsyncKeyLockFromImageSharpWeb()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyLockerFromImageSharpWeb = new SixLabors.ImageSharp.Web.Synchronization.AsyncKeyLock<string>(NumberOfLocks);
                AsyncKeyLockerFromImageSharpWebTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

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

        [Benchmark(Description = "AsyncKeyLock from ImageSharp.Web")]
        public async Task AsyncKeyLockFromImageSharpWeb()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyLockerFromImageSharpWebTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyLockFromImageSharpWeb

        #region AsyncKeyLock
        public AsyncKeyLock.AsyncLock<string>? AsyncKeyLocker { get; set; }
        public ParallelQuery<Task>? AsyncKeyLockerTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyLock))]
        public void SetupAsyncKeyLock()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyLocker = new AsyncKeyLock.AsyncLock<string>(NumberOfLocks);
                AsyncKeyLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

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

        [Benchmark(Description = "AsyncKeyLock by usercode")]
        public async Task AsyncKeyLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyLockerTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyLock

        #region KeyedSemaphores
        public KeyedSemaphoresCollection<string>? KeyedSemaphoresCollection { get; set; }
        public ParallelQuery<Task>? KeyedSemaphoresTasks { get; set; }

        [IterationSetup(Target = nameof(KeyedSemaphores))]
        public void SetupKeyedSemaphores()
        {
            if (NumberOfLocks != Contention)
            {
                KeyedSemaphoresCollection = new KeyedSemaphoresCollection<string>(NumberOfLocks);
                KeyedSemaphoresTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

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

        [Benchmark(Description = "KeyedSemaphoresCollection from Keyed Semaphores")]
        public async Task KeyedSemaphores()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(KeyedSemaphoresTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion KeyedSemaphores

        #region KeyedSemaphoresDictionary
        public KeyedSemaphoresDictionary<string>? KeyedSemaphoresDictionaryDictionary { get; set; }
        public ParallelQuery<Task>? KeyedSemaphoresDictionaryTasks { get; set; }

        [IterationSetup(Target = nameof(KeyedSemaphoresDictionary))]
        public void SetupKeyedSemaphoresDictionary()
        {
            if (NumberOfLocks != Contention)
            {
                KeyedSemaphoresDictionaryDictionary = new KeyedSemaphoresDictionary<string>(Environment.ProcessorCount, NumberOfLocks, EqualityComparer<string>.Default, TimeSpan.FromMilliseconds(10));
                KeyedSemaphoresDictionaryTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

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

        [Benchmark(Description = "KeyedSemaphoresDictionary from Keyed Semaphores")]
        public async Task KeyedSemaphoresDictionary()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(KeyedSemaphoresDictionaryTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion KeyedSemaphores

        #region AsyncDuplicateLock
        public ParallelQuery<Task>? AsyncDuplicateLockTasks { get; set; }
        public AsyncDuplicateLock? AsyncDuplicateLockCollection { get; set; }

        [IterationSetup(Target = nameof(AsyncDuplicateLock))]
        public void SetupAsyncDuplicateLock()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncDuplicateLockCollection = new();
                AsyncDuplicateLockTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

                        using (var myLock = await AsyncDuplicateLockCollection.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(AsyncDuplicateLock))]
        public void CleanupAsyncDuplicateLock()
        {
            AsyncDuplicateLockTasks = null;
            AsyncDuplicateLockCollection = null;
        }

        [Benchmark(Description = "AsyncDuplicateLock, Stephen Cleary's SO solution")]
        public async Task AsyncDuplicateLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncDuplicateLockTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncDuplicateLock

        #region TheodorZoulias
        public ParallelQuery<Task>? TheodorZouliasTasks { get; set; }
        public TheodorZoulias? TheodorZouliasCollection { get; set; }

        [IterationSetup(Target = nameof(TheodorZoulias))]
        public void SetupTheodorZoulias()
        {
            if (NumberOfLocks != Contention)
            {
                TheodorZouliasCollection = new();
                TheodorZouliasTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

                        using (var myLock = await TheodorZouliasCollection.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(TheodorZoulias))]
        public void CleanupTheodorZoulias()
        {
            TheodorZouliasTasks = null;
            TheodorZouliasCollection = null;
        }

        [Benchmark(Description = "AsyncDuplicateLock, Theodor Zoulias' SO solution")]
        public async Task TheodorZoulias()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(TheodorZouliasTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion TheodorZoulias

        #region StripedAsyncLock
        public StripedAsyncLock<string>? StripedAsyncLocker { get; set; }
        public ParallelQuery<Task>? StripedAsyncLockTasks { get; set; }

        [IterationSetup(Target = nameof(StripedAsyncLock))]
        public void SetupStripedAsyncLock()
        {
            if (NumberOfLocks != Contention)
            {
                StripedAsyncLocker = new StripedAsyncLock<string>(NumberOfLocks);
                StripedAsyncLockTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

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

        [Benchmark(Description = "StripedAsyncLock from AsyncUtilities")]
        public async Task StripedAsyncLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(StripedAsyncLockTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion StripedAsyncLock

        #region NeoSmart.Synchronization
        public ParallelQuery<Task>? NeoSmartTasks { get; set; }

        [IterationSetup(Target = nameof(NeoSmart))]
        public void SetupNeoSmart()
        {
            if (NumberOfLocks != Contention)
            {
                NeoSmartTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

                        using (var myLock = await ScopedMutex.CreateAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(NeoSmart))]
        public void CleanupNeoSmart()
        {
            NeoSmartTasks = null;
        }

        [Benchmark(Description = "NeoSmart.Synchronization")]
        public async Task NeoSmart()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(NeoSmartTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion NeoSmart.Synchronization

        #region Dao.IndividualLock
        public IndividualLocks<string> DaoIndividualLocks { get; set; }
        public ParallelQuery<Task>? DaoIndividualLockTasks { get; set; }

        [IterationSetup(Target = nameof(DaoIndividualLock))]
        public void SetupDaoIndividualLock()
        {
            if (NumberOfLocks != Contention)
            {
                DaoIndividualLocks = new IndividualLocks<string>();
                DaoIndividualLockTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

                        using (var myLock = await DaoIndividualLocks.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(DaoIndividualLock))]
        public void CleanupDaoIndividualLock()
        {
            DaoIndividualLockTasks = null;
        }

        [Benchmark(Description = "Dao.IndividualLock")]
        public async Task DaoIndividualLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(DaoIndividualLockTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion Dao.IndividualLock

        #region Stl.Fusion.AsyncLockSet
        public AsyncLockSet<string> AsyncLockSetLocks { get; set; }
        public ParallelQuery<Task>? AsyncLockSetTasks { get; set; }

        [IterationSetup(Target = nameof(StlFusionAsyncLockSet))]
        public void SetupAsyncLockSet()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncLockSetLocks = new AsyncLockSet<string>(LockReentryMode.Unchecked, Environment.ProcessorCount, NumberOfLocks);
                AsyncLockSetTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

                        using (var myLock = await AsyncLockSetLocks.Lock(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(StlFusionAsyncLockSet))]
        public void CleanupAsyncLockSet()
        {
            AsyncLockSetTasks = null;
        }

        [Benchmark(Description = "Stl.Fusion.AsyncLockSet")]
        public async Task StlFusionAsyncLockSet()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncLockSetTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion Stl.Fusion.AsyncLockSet
    }
}
