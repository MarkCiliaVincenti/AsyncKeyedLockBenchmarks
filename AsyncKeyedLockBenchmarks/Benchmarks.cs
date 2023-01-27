using AsyncKeyedLock;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Firebend.AutoCrud.Core.Threading;
using ListShuffle;
using SixLabors.ImageSharp.Web.Synchronization;

namespace AsyncKeyedLockBenchmarks
{
    //[Config(typeof(Config))]
    [MemoryDiagnoser]
    [JsonExporterAttribute.Full]
    [JsonExporterAttribute.FullCompressed]
    public class Benchmarks
    {
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
                    o.PoolInitialFill = Environment.ProcessorCount * 4;
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
        //[Benchmark]
        public async Task AsyncKeyedLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyedLockerTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyedLock

        #region AsyncKeyLock
        public AsyncKeyLock<int>? AsyncKeyLocker { get; set; }
        public ParallelQuery<Task>? AsyncKeyLockerTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyLockFromImageSharpWeb))]
        public void SetupAsyncKeyLock()
        {
            if (NumberOfLocks != Contention)
            {
                AsyncKeyLocker = new AsyncKeyLock<int>(NumberOfLocks);
                AsyncKeyLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = i % NumberOfLocks;

                        using (var myLock = await AsyncKeyLocker.LockAsync(key).ConfigureAwait(false))
                        {
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(AsyncKeyLockFromImageSharpWeb))]
        public void CleanupAsyncKeyLock()
        {
            AsyncKeyLocker = null;
            AsyncKeyLockerTasks = null;
        }

        [Benchmark]
        public async Task AsyncKeyLockFromImageSharpWeb()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(AsyncKeyLockerTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion AsyncKeyLock

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
    }
}
