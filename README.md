This is a console application benchmarking three similar libraries, [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock), [KeyedSemaphores](https://github.com/amoerie/keyed-semaphores) and `StripedAsyncLock` from [AsyncUtilities](https://www.nuget.org/packages/AsyncUtilities).

The purpose of these libraries are to lock based on a key. This has many applications, for example a bank might want to process transactions concurrently but only one at a time for the same bank account.

The results clearly show [AsyncKeyedLock](https://github.com/MarkCiliaVincenti/AsyncKeyedLock) to be more performant than the other two libraries, especially in higher loads, whilst also consuming less memory under load.

Typical results:

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.2130/21H2/November2021Update)
Intel Core i7-3632QM CPU 2.20GHz (Ivy Bridge), 1 CPU, 8 logical and 4 physical cores
.NET SDK=7.0.100-rc.2.22477.23
  [Host]     : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX
  Job-TGCGMH : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX
InvocationCount=1  UnrollFactor=1 


```
|Method|NumberOfLocks|Contention|Mean|Error|StdDev|Median|Ratio|RatioSD|Gen0|Gen1|Gen2|Allocated|Alloc Ratio|
|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|:----|
|AsyncKeyedLock (non-generics)|100|1|13.99 ms|0.276 ms|0.405 ms|14.13 ms|0.99|0.04|-|-|-|120.13 KB|1.05|
|AsyncKeyedLock|100|1|14.14 ms|0.263 ms|0.258 ms|14.18 ms|1.00|0.00|-|-|-|114.86 KB|1.00|
|KeyedSemaphores|100|1|13.86 ms|0.270 ms|0.396 ms|13.90 ms|0.98|0.03|-|-|-|115.03 KB|1.00|
|**StripedAsyncLock** _(Low Mem, Fast)_|100|1|**13.77 ms**|0.271 ms|0.502 ms|13.93 ms|0.97|0.04|-|-|-|84.92 KB|**0.74**|
|||||||||||||||
|AsyncKeyedLock (non-generics)|100|10|154.67 ms|0.174 ms|0.163 ms|154.72 ms|1.00|0.00|-|-|-|650.03 KB|1.05|
|**AsyncKeyedLock** (Low Mem, Fast)|100|10|**154.63 ms**|0.301 ms|0.267 ms|154.75 ms|1.00|0.00|-|-|-|616.63 KB|**1.00**|
|KeyedSemaphores|100|10|154.70 ms|0.274 ms|0.256 ms|154.74 ms|1.00|0.00|-|-|-|623.83 KB|1.01|
|StripedAsyncLock|100|10|154.66 ms|0.170 ms|0.159 ms|154.66 ms|1.00|0.00|-|-|-|790.61 KB|1.28|
|||||||||||||||
|AsyncKeyedLock (non-generics)|100|100|1,560.55 ms|0.451 ms|0.422 ms|1,560.48 ms|1.00|0.00|1000.0000|-|-|6049.44 KB|1.05|
|**AsyncKeyedLock** (Low Mem)|100|100|1,560.51 ms|0.504 ms|0.472 ms|1,560.34 ms|1.00|0.00|1000.0000|-|-|5735.64 KB|**1.00**|
|KeyedSemaphores|100|100|1,560.56 ms|0.395 ms|0.370 ms|1,560.41 ms|1.00|0.00|1000.0000|-|-|5813.16 KB|1.01|
|**StripedAsyncLock** (Fast)|100|100|**1,559.94 ms**|0.503 ms|0.446 ms|1,559.77 ms|1.00|0.00|1000.0000|-|-|7946.99 KB|1.39|
|||||||||||||||
|AsyncKeyedLock (non-generics)|1000|1|13.63 ms|0.272 ms|0.555 ms|13.76 ms|1.00|0.05|-|-|-|764.77 KB|1.06|
|AsyncKeyedLock|1000|1|13.68 ms|0.267 ms|0.439 ms|13.76 ms|1.00|0.00|-|-|-|718.3 KB|1.00|
|**KeyedSemaphores** (Fast)|1000|1|**13.49 ms**|0.269 ms|0.572 ms|13.62 ms|0.99|0.07|-|-|-|717.48 KB|1.00|
|**StripedAsyncLock** (Low Mem)|1000|1|13.51 ms|0.269 ms|0.486 ms|13.55 ms|0.99|0.06|-|-|-|415.71 KB|**0.58**|
|||||||||||||||
|AsyncKeyedLock (non-generics)|1000|10|154.67 ms|0.211 ms|0.176 ms|154.73 ms|1.00|0.00|1000.0000|-|-|6142.62 KB|1.06|
|**AsyncKeyedLock** (Low Mem)|1000|10|153.96 ms|0.273 ms|0.213 ms|154.00 ms|1.00|0.00|-|-|-|5814.83 KB|**1.00**|
|KeyedSemaphores|1000|10|154.14 ms|0.288 ms|0.255 ms|154.18 ms|1.00|0.00|1000.0000|-|-|5885.14 KB|1.01|
|**StripedAsyncLock** (Fast)|1000|10|**153.40 ms**|0.164 ms|0.153 ms|153.39 ms|1.00|0.00|1000.0000|-|-|7551.91 KB|1.30|
|||||||||||||||
|AsyncKeyedLock (non-generics)|1000|100|1,657.05 ms|17.517 ms|16.385 ms|1,662.99 ms|1.01|0.02|10000.0000|4000.0000|1000.0000|59320.31 KB|1.06|
|**AsyncKeyedLock** (Low Mem, Fast)|1000|100|**1,643.23 ms**|19.737 ms|18.462 ms|1,647.41 ms|1.00|0.00|10000.0000|4000.0000|1000.0000|56180.38 KB|**1.00**|
|KeyedSemaphores|1000|100|1,647.31 ms|16.207 ms|15.160 ms|1,648.28 ms|1.00|0.01|10000.0000|4000.0000|1000.0000|56952.45 KB|1.01|
|StripedAsyncLock|1000|100|1,686.04 ms|26.492 ms|24.781 ms|1,705.19 ms|1.03|0.02|14000.0000|6000.0000|2000.0000|78342.73 KB|1.39|
|||||||||||||||
|AsyncKeyedLock (non-generics)|5000|1|17.89 ms|2.264 ms|6.676 ms|13.71 ms|1.48|0.53|1000.0000|-|-|3872.83 KB|1.07|
|**AsyncKeyedLock** (Fast)|5000|1|**13.52 ms**|0.267 ms|0.297 ms|13.55 ms|1.00|0.00|-|-|-|3611.71 KB|1.00|
|KeyedSemaphores|5000|1|13.82 ms|0.271 ms|0.332 ms|13.79 ms|1.02|0.04|-|-|-|3614.29 KB|1.00|
|**StripedAsyncLock** (Low Mem)|5000|1|27.67 ms|0.746 ms|2.188 ms|28.06 ms|2.02|0.26|-|-|-|1935.41 KB|**0.54**|
|||||||||||||||
|**AsyncKeyedLock (non-generics)** (Fast)|5000|10|**186.06 ms**|5.180 ms|15.274 ms|182.09 ms|0.99|0.10|5000.0000|2000.0000|1000.0000|30448.1 KB|1.06|
|**AsyncKeyedLock** (Low Mem)|5000|10|188.14 ms|4.745 ms|13.841 ms|183.74 ms|1.00|0.00|4000.0000|1000.0000|-|28787.59 KB|**1.00**|
|KeyedSemaphores|5000|10|202.33 ms|4.425 ms|13.046 ms|198.48 ms|1.08|0.11|5000.0000|2000.0000|1000.0000|29138.61 KB|1.01|
|StripedAsyncLock|5000|10|211.78 ms|5.398 ms|15.915 ms|211.20 ms|1.13|0.12|7000.0000|3000.0000|1000.0000|37338.21 KB|1.30|
|||||||||||||||
|AsyncKeyedLock (non-generics)|5000|100|2,367.69 ms|46.713 ms|47.971 ms|2,376.10 ms|1.04|0.02|47000.0000|16000.0000|2000.0000|294447.91 KB|1.06|
|**AsyncKeyedLock** (Low Mem, Fast)|5000|100|**2,286.55 ms**|41.570 ms|38.884 ms|2,293.50 ms|1.00|0.00|46000.0000|17000.0000|2000.0000|278762.28 KB|**1.00**|
|KeyedSemaphores|5000|100|2,318.70 ms|45.770 ms|50.873 ms|2,314.48 ms|1.01|0.03|46000.0000|16000.0000|2000.0000|282649.57 KB|1.01|
|StripedAsyncLock|5000|100|2,950.73 ms|58.971 ms|60.559 ms|2,945.02 ms|1.29|0.03|63000.0000|22000.0000|1000.0000|389243.53 KB|1.40|

The ideal way to get AsyncKeyedLock is from [NuGet](https://www.nuget.org/packages/AsyncKeyedLock) but the source code is available on [GitHub](https://github.com/MarkCiliaVincenti/AsyncKeyedLock).