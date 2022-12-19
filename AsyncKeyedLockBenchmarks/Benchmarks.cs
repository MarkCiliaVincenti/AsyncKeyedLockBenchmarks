using AsyncKeyedLock;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using KeyedSemaphores;
using ListShuffle;

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

        //        AddJob(baseJob.WithNuGet("AsyncKeyedLock", "5.1.2").WithBaseline(true));
        //        AddJob(baseJob.WithNuGet("AsyncKeyedLock", "6.0.2"));
        //    }
        //}

        //[Params(200, 10_000)] public int NumberOfLocks { get; set; }

        //[Params(100, 10_000)] public int Contention { get; set; }

        //[Params(0, 1, 5)] public int GuidReversals { get; set; }

        [Params(20, 1000)] public int NumberOfLocks { get; set; }

        [Params(10, 1000)] public int Contention { get; set; }

        [Params(0)] public int GuidReversals { get; set; }

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

        [Benchmark(Baseline = true)]
        //[Benchmark]
        public async Task AsyncKeyedLock()
        {
            await RunTests(AsyncKeyedLockerTasks).ConfigureAwait(false);
        }
        #endregion AsyncKeyedLock
    }
}
