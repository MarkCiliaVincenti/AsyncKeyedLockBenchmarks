﻿using AsyncKeyedLock;
using AsyncUtilities;
using KeyedSemaphores;
using ListShuffle;
using System.Diagnostics;

int count = 500;
List<string> myList = new();
for (int i = 1; i <= count; i++)
{
    for (int j = 1; j <= i; j++)
    {
        myList.Add(i.ToString());
    }
}

myList.Shuffle();

#region StripedAsyncLock from AsyncUtilities
StripedAsyncLock<string> _lock = new(stripes: myList.Count);

{
    // Discard these results
    await Task.WhenAll(Enumerable.Range(0, 10).Select(async i =>
    {
        using (await _lock.LockAsync(myList[i]))
        {
            await Task.Delay(1);
        }
    }));
    Console.WriteLine($"Initialised StripedAsyncLock. Starting benchmark...");
}

{
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    await Task.WhenAll(Enumerable.Range(0, myList.Count).Select(async i =>
    {
        using (await _lock.LockAsync(myList[i]))
        {
            await Task.Delay(1);
        }
    }));
    stopwatch.Stop();
    Console.WriteLine($"StripedAsyncLock took {stopwatch.ElapsedMilliseconds}ms");
}
#endregion

#region AsyncKeyedLock
var asyncKeyedLocker = new AsyncKeyedLocker();

{
    // Discard these results
    await Task.WhenAll(Enumerable.Range(0, 10).Select(async i =>
    {
        using (await asyncKeyedLocker.LockAsync(myList[i]))
        {
            await Task.Delay(1);
        }
    }));
    Console.WriteLine($"Initialised AsyncKeyedLock. Starting benchmark...");
}

{
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    await Task.WhenAll(Enumerable.Range(0, myList.Count).Select(async i =>
    {
        using (await asyncKeyedLocker.LockAsync(myList[i]))
        {
            await Task.Delay(1);
        }
    }));
    stopwatch.Stop();
    Console.WriteLine($"AsyncKeyedLock took {stopwatch.ElapsedMilliseconds}ms");
}
#endregion

#region KeyedSemaphores
{
    // Discard these results
    await Task.WhenAll(Enumerable.Range(0, 10).Select(async i =>
    {
        using (await KeyedSemaphore.LockAsync(myList[i]))
        {
            await Task.Delay(1);
        }
    }));
    Console.WriteLine($"Initialised KeyedSemaphores. Starting benchmark...");
}

{
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    await Task.WhenAll(Enumerable.Range(0, myList.Count).Select(async i =>
    {
        using (await KeyedSemaphore.LockAsync(myList[i]))
        {
            await Task.Delay(1);
        }
    }));
    stopwatch.Stop();
    Console.WriteLine($"KeyedSemaphores took {stopwatch.ElapsedMilliseconds}ms");
}
#endregion