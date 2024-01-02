# Benchmarks for the AsyncKeyedLock library
This is a project to help benchmark the [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock) library against other competing solutions. We are testing with 3 separate parameters:

1. Number of locks: 200 and 10000
2. Contention: 100 and 10000
3. GUID reversals (to simulate some load): 0, 1 and 5

When looking at the benchmarks, please ignore all values for 10k locks with 10k contention: they are dummy tests and don't actually run.

Solutions tested:

1. AsyncKeyedLocker with pooling (from [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock))
2. AsyncKeyedLocker without pooling (from [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock))
3. StripedAsyncKeyedLocker (from [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock))
4. AsyncKeyLock from [ImageSharp.Web](https://github.com/SixLabors/ImageSharp.Web)
5. [AsyncKeyLock](https://github.com/usercode/AsyncKeyLock)
6. KeyedSemaphoresCollection from [Keyed Semaphores](https://github.com/amoerie/keyed-semaphores), similar to AsyncKeyedLocker without pooling
7. KeyedSemaphoresDictionary from [Keyed Semaphores](https://github.com/amoerie/keyed-semaphores), similar to StripedAsyncKeyedLocker
8. AsyncDuplicateLock from [Stephen Cleary's StackOverflow solution](https://stackoverflow.com/a/31194647)
9. AsyncDuplicateLock from [Theodor Zoulias' improvement over #8](https://stackoverflow.com/a/65256155)
10. StripedAsyncLock from [AsyncUtilities](https://github.com/i3arnon/AsyncUtilities), similar to StripedAsyncKeyedLocker

Some comparisons over time can also be found at https://markciliavincenti.github.io/AsyncKeyedLockBenchmarks/dev/bench/
