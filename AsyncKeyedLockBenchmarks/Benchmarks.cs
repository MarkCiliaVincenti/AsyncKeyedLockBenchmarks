using AsyncKeyedLock;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using DeterministicGuids;
using ListShuffle;

namespace AsyncKeyedLockBenchmarks
{
    [Config(typeof(Config))]
    [MemoryDiagnoser]
    [JsonExporterAttribute.Full]
    [JsonExporterAttribute.FullCompressed]
    public class Benchmarks
    {
        private class Config : ManualConfig
        {
            public Config()
            {
                var baseJob = Job.Default;

                AddJob(baseJob.WithNuGet("AsyncKeyedLock", "7.1.8-beta2").WithBaseline(true));
                AddJob(baseJob.WithNuGet("AsyncKeyedLock", "7.1.8-beta3"));
            }
        }


        [ParamsSource(nameof(Configurations))]
        public (int NumberOfLocks, int Contention) Setting { get; set; }
        public (int NumberOfLocks, int Contention)[] Configurations { get; } =
        {
            (200, 100),
            (200, 10_000),
            (10_000, 100)
        };

        [Params(0, 1, 5)] public int GuidReversals { get; set; }

        private readonly Dictionary<int, List<int>> _shuffledIntegers = new();

        public List<int> ShuffledIntegers
        {
            get
            {
                if (!_shuffledIntegers.TryGetValue(Setting.Contention * Setting.NumberOfLocks, out var shuffledIntegers))
                {
                    shuffledIntegers = Enumerable.Range(0, Setting.Contention * Setting.NumberOfLocks).ToList();
                    shuffledIntegers.Shuffle();
                    _shuffledIntegers[Setting.Contention * Setting.NumberOfLocks] = shuffledIntegers;
                }
                return shuffledIntegers;
            }
        }

        private void Operation()
        {
            for (int i = 0; i < GuidReversals; i++)
            {
                Guid guid = DeterministicGuid.Create(DeterministicGuid.Namespaces.Events, i.ToString());
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
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        #region AsyncKeyedLock
        public AsyncKeyedLocker<string>? AsyncKeyedLocker { get; set; }
        public ParallelQuery<Task>? AsyncKeyedLockerTasks { get; set; }

        [IterationSetup(Target = nameof(AsyncKeyedLock))]
        public void SetupAsyncKeyedLock()
        {
            if (Setting.NumberOfLocks != Setting.Contention)
            {
                AsyncKeyedLocker = new AsyncKeyedLocker<string>(o =>
                {
                    o.PoolSize = Setting.NumberOfLocks;
                    o.PoolInitialFill = Environment.ProcessorCount * 2;
                }, Environment.ProcessorCount, Setting.NumberOfLocks);
                AsyncKeyedLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % Setting.NumberOfLocks).ToString();

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

        //[Benchmark(Baseline = true, Description = "AsyncKeyedLocker with pooling")]
        [Benchmark(Description = "AsyncKeyedLocker with pooling")]
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
            if (Setting.NumberOfLocks != Setting.Contention)
            {
                AsyncKeyedLockerNoPooling = new AsyncKeyedLocker<string>(o =>
                { }, Environment.ProcessorCount, Setting.NumberOfLocks);
                AsyncKeyedLockerNoPoolingTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % Setting.NumberOfLocks).ToString();

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

        //[Benchmark(Description = "AsyncKeyedLocker without pooling")]
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
            if (Setting.NumberOfLocks != Setting.Contention)
            {
                StripedAsyncKeyedLockerCollection = new StripedAsyncKeyedLocker<string>(Setting.NumberOfLocks, 1);
                StripedAsyncKeyedLockerTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % Setting.NumberOfLocks).ToString();

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

        //[Benchmark(Description = "StripedAsyncKeyedLocker")]
        public async Task StripedAsyncKeyedLock()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(StripedAsyncKeyedLockerTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion StripedAsyncKeyedLocker
    }
}
