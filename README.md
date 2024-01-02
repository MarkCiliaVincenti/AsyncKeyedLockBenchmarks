Options tested:

1. AsyncKeyedLocker with pooling
2. AsyncKeyedLocker without pooling
3. StripedAsyncKeyedLocker
4. AsyncKeyLock from [ImageSharp.Web](https://github.com/SixLabors/ImageSharp.Web)
5. [AsyncKeyLock](https://github.com/usercode/AsyncKeyLock)
6. [KeyedSemaphoresCollection](https://github.com/amoerie/keyed-semaphores), similar to AsyncKeyedLocker without pooling
7. [KeyedSemaphoresDictionary](https://github.com/amoerie/keyed-semaphores), similar to StripedAsyncKeyedLocker
8. [AsyncDuplicateLock](https://stackoverflow.com/a/31194647) from Stephen Cleary's StackOverflow solution
9. [AsyncDuplicateLock](https://stackoverflow.com/a/65256155) from Theodor Zoulias' improvement over #8
10. [StripedAsyncLock](https://github.com/i3arnon/AsyncUtilities) from AsyncUtilities

When looking at the benchmarks, please ignore all values for 10k locks with 10k contention: they are dummy tests and don't actually run.

Some comparisons over time can also be found at https://markciliavincenti.github.io/AsyncKeyedLockBenchmarks/dev/bench/
