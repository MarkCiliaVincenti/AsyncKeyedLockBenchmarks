This is a console application benchmarking two similar libraries, [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock) and [KeyedSemaphores](https://github.com/amoerie/keyed-semaphores).

The purpose of these libraries are to lock based on a key. This has many applications, for example a bank might want to process transactions concurrently but only one at a time for the same bank account.

In these tests, we first build up data, shuffle it using [ListShuffle](https://github.com/MarkCiliaVincenti/ListShuffle), initialize both libraries and then run the tests.

The results consistently show [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock) to be considerably more performant.

Typical results:

````
Initialised AsyncKeyedLock. Starting benchmark...
AsyncKeyedLock took 15355ms
Initialised KeyedSemaphores. Starting benchmark...
KeyedSemaphores took 40444ms
````

The ideal way to get AsyncKeyedLock is from [NuGet](https://www.nuget.org/packages/AsyncKeyedLock) but the source code is available on [GitHub](https://github.com/MarkCiliaVincenti/AsyncKeyedLock).