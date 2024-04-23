using AsyncKeyedLock;
using AsyncUtilities;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Dao.IndividualLock;
using EasyCaching.Core.DistributedLock;
using KeyedSemaphores;
using ListShuffle;
using NeoSmart.Synchronization;

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

        #region MemoryLock
        public ParallelQuery<Task>? MemoryLockTasks { get; set; }

        [IterationSetup(Target = nameof(MemoryLocker))]
        public void SetupMemoryLock()
        {
            if (NumberOfLocks != Contention)
            {
                MemoryLockTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

                        using (var memoryLock = new MemoryLock(key))
                        {
                            await memoryLock.LockAsync(-1, CancellationToken.None).ConfigureAwait(false);
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(MemoryLocker))]
        public void CleanupMemoryLock()
        {
            MemoryLockTasks = null;
        }

        [Benchmark(Baseline = false, Description = "MemoryLock with AsyncKeyedLock")]
        public async Task MemoryLocker()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(MemoryLockTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion MemoryLock

        #region MemoryLockOld
        public ParallelQuery<Task>? MemoryLockOldTasks { get; set; }

        [IterationSetup(Target = nameof(MemoryLockerOld))]
        public void SetupMemoryLockOld()
        {
            if (NumberOfLocks != Contention)
            {
                MemoryLockOldTasks = ShuffledIntegers
                    .Select(async i =>
                    {
                        var key = (i % NumberOfLocks).ToString();

                        using (var memoryLock = new MemoryLockOld(key))
                        {
                            await memoryLock.LockAsync(-1, CancellationToken.None).ConfigureAwait(false);
                            Operation();
                        }

                        await Task.Yield();
                    }).AsParallel();
            }
        }

        [IterationCleanup(Target = nameof(MemoryLockerOld))]
        public void CleanupMemoryLockOld()
        {
            MemoryLockOldTasks = null;
        }

        [Benchmark(Baseline = true, Description = "Current MemoryLock")]
        public async Task MemoryLockerOld()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            await RunTests(MemoryLockOldTasks).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        #endregion MemoryLockOld
    }
}
