using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.NativeAot;
using NullGC.Allocators;
using NullGC.Collections;
using NullGC.TestCommons;

namespace NullGC.DragRace.Benchmarks
{
    // ����һ������ NativeAOT ������
    public class AotUncachedValueListBenchmarks : ManualConfig
    {
        public AotUncachedValueListBenchmarks()
        {
            AddJob(Job.Default
                .WithToolchain(NativeAotToolchain.Net90)
                .WithWarmupCount(1)
                .WithIterationCount(5));
        }
    }

    [Config(typeof(AotUncachedValueListBenchmarks))]
    public class ValueList_AOT_NonArena_Benchmarks
    {
        private ValueList<int> _valList;
        private int _count = 1_000_000;

        [GlobalSetup]
        public void GlobalSetup()
        {
            // ʹ�÷� Arena ģʽ��uncached/unscoped�����ڴ��������
            AllocatorContextInitializer.SetupDefaultUncachedUnscopedAllocationContext(out IMemoryAllocationTrackable? allocTracker,
                out IMemoryAllocationTrackable? nativeTracker);
            // ����һ����ʼ����Ϊ0�� ValueList<int>
            _valList = new ValueList<int>(0);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _valList.Clear();
        }

        [Benchmark]
        public void AddElements()
        {
            for (int i = 0; i < _count; i++)
            {
                _valList.Add(i);
            }
        }

        [Benchmark]
        public void ClearList()
        {
            _valList.Clear();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _valList.Dispose();
            AllocatorContext.ClearProvidersAndAllocations();
        }
    }
}
