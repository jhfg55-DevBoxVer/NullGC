﻿using BenchmarkDotNet.Attributes;
using HonkPerf.NET.RefLinq;
using NullGC.DragRace.Models;
using NullGC.Linq;

namespace NullGC.DragRace.Benchmarks;

public class BigStructGeneric_WhereSelectSelectSumBenchmark : LinqBenchmarkBase
{
    [Benchmark]
    public void SystemLinq()
    {
        _dummyFloat = _intArr.Where(x => x > 100)
            .Select(x => new BigStructGeneric<int, float>(x, x * 1.5f))
            .Select(x => x.Value)
            .Sum();
    }

    [Benchmark]
    public void RefLinq()
    {
        _dummyFloat = _intArr.ToRefLinq().Where(x => x > 100)
            .Select(x => new BigStructGeneric<int, float>(x, x * 1.5f))
            .Select(x => x.Value)
            .Sum();
    }

    [Benchmark]
    public void NullGCLinqValue()
    {
        _dummyFloat = _valIntArr.LinqValue().Where(x => x > 100)
            .Select(x => new BigStructGeneric<int, float>(x, x * 1.5f))
            .Select(x => x.Value)
            .Sum();
    }

    [Benchmark]
    public void NullGCLinqValueSelectIn()
    {
        _dummyFloat = _valIntArr.LinqValue().Where(x => x > 100)
            .Select(x => new BigStructGeneric<int, float>(x, x * 1.5f))
            .Select((in BigStructGeneric<int, float> x) => x.Value)
            .Sum();
    }

    [Benchmark]
    public void NullGCLinqRef()
    {
        _dummyFloat = _valIntArr.LinqRef().Where(x => x > 100)
            .Select((in int x) => new BigStructGeneric<int, float>(x, x * 1.5f))
            .Select((in BigStructGeneric<int, float> x) => x.Value)
            .Sum();
    }
}