This is a console application benchmarking three similar libraries, [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock), [KeyedSemaphores](https://github.com/amoerie/keyed-semaphores) and `StripedAsyncLock` from [AsyncUtilities](https://www.nuget.org/packages/AsyncUtilities).

The purpose of these libraries are to lock based on a key. This has many applications, for example a bank might want to process transactions concurrently but only one at a time for the same bank account.

The results show [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock) to be more performant than the other two libraries, especially in higher loads, whilst also consuming less memory under load.

Typical results:

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2130/21H2/November2021Update)
Intel Core i7-3632QM CPU 2.20GHz (Ivy Bridge), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.100-rc.2.22477.23
  [Host]     : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX
  Job-TGCGMH : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX
InvocationCount=1  UnrollFactor=1 


```
|             Method|NumberOfLocks|Contention|Mean|Error|StdDev|Median|Ratio|RatioSD|Gen0|Gen1|Gen2|Allocated|Alloc Ratio|
|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|
|AsyncKeyedLockNonGenerics|100|1|14.05 ms|0.273 ms|0.345 ms|14.17 ms|1.00|0.03|-|-|-|120.14 KB|1.04|
|AsyncKeyedLock|100|1|14.02 ms|0.280 ms|0.344 ms|14.14 ms|1.00|0.00|-|-|-|115.34 KB|1.00|
|KeyedSemaphores|100|1|13.87 ms|0.276 ms|0.377 ms|13.99 ms|0.99|0.04|-|-|-|114.94 KB|1.00|
|**StripedAsyncLock** _(Low Mem, Fast)_|100|1|**13.81 ms**|0.272 ms|0.439 ms|13.85 ms|0.98|0.03|-|-|-|**84.95 KB**|0.74|
|||||||||||||||
|**AsyncKeyedLockNonGenerics** _(Fast)_|100|10|**154.71 ms**|0.166 ms|0.155 ms|154.73 ms|1.00|0.00|-|-|-|650.08 KB|1.05|
|**AsyncKeyedLock** _(Low Mem)_|100|10|154.74 ms|0.210 ms|0.196 ms|154.77 ms|1.00|0.00|-|-|-|**616.68 KB**|1.00|
|KeyedSemaphores|100|10|154.75 ms|0.146 ms|0.129 ms|154.78 ms|1.00|0.00|-|-|-|623.68 KB|1.01|
|StripedAsyncLock|100|10|154.73 ms|0.195 ms|0.182 ms|154.77 ms|1.00|0.00|-|-|-|790.61 KB|1.28|
|||||||||||||||
|AsyncKeyedLockNonGenerics|100|100|1,560.70 ms|0.393 ms|0.367 ms|1,560.71 ms|1.00|0.00|1000.0000|-|-|6050.3 KB|1.05|
|**AsyncKeyedLock** _(Low Mem)_|100|100|1,560.52 ms|0.455 ms|0.425 ms|1,560.37 ms|1.00|0.00|1000.0000|-|-|**5736.49 KB**|1.00|
|**KeyedSemaphores** _(Fast)_|100|100|**1,560.43 ms**|0.145 ms|0.113 ms|1,560.45 ms|1.00|0.00|1000.0000|-|-|5813.31 KB|1.01|
|StripedAsyncLock|100|100|1,563.51 ms|7.522 ms|6.668 ms|1,560.00 ms|1.00|0.00|1000.0000|-|-|7946.99 KB|1.39|
|||||||||||||||
|AsyncKeyedLockNonGenerics|1000|1|13.70 ms|0.274 ms|0.527 ms|13.84 ms|1.01|0.06|-|-|-|764.95 KB|1.07|
|AsyncKeyedLock|1000|1|13.71 ms|0.273 ms|0.587 ms|13.89 ms|1.00|0.00|-|-|-|717.57 KB|1.00|
|KeyedSemaphores|1000|1|13.63 ms|0.271 ms|0.529 ms|13.77 ms|1.00|0.04|-|-|-|717.27 KB|1.00|
|**StripedAsyncLock** _(Low Mem, Fast)_|1000|1|**13.57 ms**|0.269 ms|0.426 ms|13.61 ms|1.00|0.06|-|-|-|**415.69 KB**|0.58|
|||||||||||||||
|AsyncKeyedLockNonGenerics|1000|10|154.66 ms|0.146 ms|0.122 ms|154.66 ms|1.00|0.00|1000.0000|-|-|6142.95 KB|1.06|
|**AsyncKeyedLock** _(Low Mem)_|1000|10|153.96 ms|0.330 ms|0.275 ms|153.93 ms|1.00|0.00|-|-|-|**5814.87 KB**|1.00|
|KeyedSemaphores|1000|10|154.25 ms|0.407 ms|0.361 ms|154.20 ms|1.00|0.00|1000.0000|-|-|5885.02 KB|1.01|
|**StripedAsyncLock** _(Fast)_|1000|10|**153.46 ms**|0.144 ms|0.127 ms|153.48 ms|1.00|0.00|1000.0000|-|-|7551.91 KB|1.30|
|||||||||||||||
|AsyncKeyedLockNonGenerics|1000|100|1,653.03 ms|9.916 ms|9.276 ms|1,649.07 ms|1.01|0.01|11000.0000|6000.0000|2000.0000|59363.66 KB|1.06|
|**AsyncKeyedLock** _(Low Mem)_|1000|100|1,637.78 ms|10.166 ms|9.509 ms|1,633.10 ms|1.00|0.00|10000.0000|4000.0000|1000.0000|**56178.54 KB**|1.00|
|**KeyedSemaphores** _(Fast)_|1000|100|**1,636.42 ms**|17.703 ms|14.783 ms|1,633.14 ms|1.00|0.01|10000.0000|4000.0000|1000.0000|56952.53 KB|1.01|
|StripedAsyncLock|1000|100|1,692.85 ms|20.619 ms|19.287 ms|1,705.26 ms|1.03|0.01|14000.0000|7000.0000|2000.0000|78330.75 KB|1.39|
|||||||||||||||
|AsyncKeyedLockNonGenerics|5000|1|13.72 ms|0.157 ms|0.302 ms|13.67 ms|1.02|0.03|-|-|-|3870.72 KB|1.07|
|**AsyncKeyedLock** _(Fast)_|5000|1|**13.56 ms**|0.268 ms|0.329 ms|13.62 ms|1.00|0.00|-|-|-|3611.55 KB|1.00|
|KeyedSemaphores|5000|1|13.66 ms|0.241 ms|0.201 ms|13.62 ms|1.00|0.03|1000.0000|-|-|3725.66 KB|1.03|
|**StripedAsyncLock** _(Low Mem)_|5000|1|27.35 ms|1.135 ms|3.329 ms|28.17 ms|2.03|0.25|-|-|-|1935.41 KB|**0.54**|
|||||||||||||||
|**AsyncKeyedLockNonGenerics** _(Fast)_|5000|10|**185.99 ms**|5.000 ms|14.742 ms|182.43 ms|1.00|0.10|5000.0000|2000.0000|1000.0000|30488.7 KB|1.06|
|**AsyncKeyedLock** _(Low Mem)_|5000|10|186.95 ms|5.048 ms|14.805 ms|183.86 ms|1.00|0.00|4000.0000|2000.0000|1000.0000|**28786.31 KB**|1.00|
|KeyedSemaphores|5000|10|191.80 ms|4.045 ms|11.798 ms|185.24 ms|1.03|0.10|5000.0000|2000.0000|1000.0000|29137.1 KB|1.01|
|StripedAsyncLock|5000|10|212.00 ms|5.478 ms|16.153 ms|212.50 ms|1.14|0.13|6000.0000|3000.0000|1000.0000|37343.16 KB|1.30|
|||||||||||||||
|AsyncKeyedLockNonGenerics|5000|100|2,357.39 ms|37.751 ms|35.313 ms|2,346.22 ms|1.07|0.03|48000.0000|17000.0000|3000.0000|294517.68 KB|1.06|
|**AsyncKeyedLock** _(Low Mem, Fast)_|5000|100|**2,132.66 ms**|42.620 ms|76.852 ms|2,122.48 ms|1.00|0.00|45000.0000|15000.0000|2000.0000|**278790.46 KB**|1.00|
|KeyedSemaphores|5000|100|2,307.62 ms|44.926 ms|53.481 ms|2,317.53 ms|1.06|0.05|45000.0000|14000.0000|1000.0000|282589.94 KB|1.01|
|StripedAsyncLock|5000|100|2,878.86 ms|55.265 ms|69.892 ms|2,873.87 ms|1.32|0.05|64000.0000|24000.0000|2000.0000|389262.54 KB|1.40|

The ideal way to get AsyncKeyedLock is from [NuGet](https://www.nuget.org/packages/AsyncKeyedLock) but the source code is available on [GitHub](https://github.com/MarkCiliaVincenti/AsyncKeyedLock).