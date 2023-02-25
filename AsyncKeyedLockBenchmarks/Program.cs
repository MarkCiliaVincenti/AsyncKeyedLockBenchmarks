using BenchmarkDotNet.Running;
using System.Collections;

/*
long v1clashes = 0;
long v2clashes = 0;
long v3clashes = 0;

int[] guids;
var iterations = 100_000;
var size = 10_000;
var primeSize = HashHelpers.GetPrime(size);

for (int i = 0; i < iterations; ++i)
{
    guids = Enumerable.Range(1, size).Select(x => Guid.NewGuid().GetHashCode()).ToArray();

    v1clashes += size - guids.Select(x => (uint)x % size).Distinct().Count();
    v2clashes += size - guids.Select(x => (uint)x % primeSize).Distinct().Count();
    v3clashes += size - guids.Select(x => (x & int.MaxValue) % primeSize).Distinct().Count();
}

var differenceV1toV2 = (Convert.ToDouble(v2clashes - v1clashes) / v1clashes) * 100;
var differenceV1toV3 = (Convert.ToDouble(v3clashes - v1clashes) / v1clashes) * 100;
var differenceV2toV3 = (Convert.ToDouble(v3clashes - v2clashes) / v2clashes) * 100;

var percentageClashed = (Convert.ToDouble(v3clashes) / (iterations * size)) * 100;*/
Environment.SetEnvironmentVariable("R_HOME", "C:\\Program Files\\R\\R-4.2.1");
var summary = BenchmarkRunner.Run(typeof(Program).Assembly);