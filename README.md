# Benchmarks for the AsyncKeyedLock library
This is a project to help benchmark the [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock) library against other competing solutions. We are testing with 3 separate parameters:

1. Number of locks: 200 and 10000
2. Contention: 100 and 10000
3. GUID reversals (to simulate some load): 0, 1 and 5

When looking at the benchmarks, please ignore all values for 10k locks with 10k contention: they are dummy tests and don't actually run.

## Solutions tested
1. AsyncKeyedLocker with pooling (from [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock))
2. AsyncKeyedLocker without pooling (from [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock))
3. StripedAsyncKeyedLocker (from [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock))
4. AsyncKeyLock from [ImageSharp.Web](https://github.com/SixLabors/ImageSharp.Web)
5. [AsyncKeyLock](https://github.com/usercode/AsyncKeyLock) by usercode
6. KeyedSemaphoresCollection from [Keyed Semaphores](https://github.com/amoerie/keyed-semaphores), similar to StripedAsyncKeyedLocker
8. KeyedSemaphoresDictionary from [Keyed Semaphores](https://github.com/amoerie/keyed-semaphores), similar to AsyncKeyedLocker without pooling
10. AsyncDuplicateLock from [Stephen Cleary's StackOverflow solution](https://stackoverflow.com/a/31194647)
11. AsyncDuplicateLock from [Theodor Zoulias' improvement over #8](https://stackoverflow.com/a/65256155)
12. StripedAsyncLock from [AsyncUtilities](https://github.com/i3arnon/AsyncUtilities), similar to StripedAsyncKeyedLocker
13. [NeoSmart.Synchronization](https://github.com/neosmart/synchronization)
14. [Dao.IndividualLock](https://github.com/OscarKoo/IndividualLock)

## Results
The [benchmark results](https://github.com/MarkCiliaVincenti/AsyncKeyedLockBenchmarks/actions/workflows/dotnet.yml) can be found in our actions as they run in Github Actions, in a fully transparent fashion.

There are also some [graphical comparisons over time](https://markciliavincenti.github.io/AsyncKeyedLockBenchmarks/dev/bench/).
