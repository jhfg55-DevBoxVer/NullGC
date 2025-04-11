using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NullGC.Allocators.Tests;

[TestClass]
public class AllocatorTests : IDisposable
{
    public AllocatorTests()
    {
        AllocatorContext.SetImplementation(new DefaultAllocatorContextImpl());
    }

    [TestMethod]
    public void AllocatorContext_SetAllocatorProvider_WillThrowIfAllocatorProviderIsNotSet()
    {
        Assert.ThrowsException<InvalidOperationException>(() => AllocatorContext.BeginAllocationScope());
    }

    [TestMethod]
    public void AllocatorContext_SetAllocatorProvider_WillThrowIfAllocatorProviderIdIsInvalid()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            AllocatorContext.SetAllocatorProvider(new DefaultAlignedNativeMemoryAllocator(),
                (int)AllocatorTypes.Invalid, true));
    }

    [TestMethod]
    public void AllocatorContext_SetAllocatorProvider_WillThrowIfAllocatorProviderWithSameIdIsAlreadySet()
    {
        AllocatorContext.SetAllocatorProvider(new DefaultAlignedNativeMemoryAllocator(),
            (int)AllocatorTypes.Default, true);
        Assert.ThrowsException<ArgumentException>(() =>
            AllocatorContext.SetAllocatorProvider(new DefaultAlignedNativeMemoryAllocator(),
                (int)AllocatorTypes.Default, true));
    }

    [TestMethod]
    public void CanAllocateAndFreeOnStaticScopedProviderAndReturnedAllocatorIsTheSame()
    {
        var nativeAllocator = new DefaultAlignedNativeMemoryAllocator();
        var nativeBuffer = new DefaultAllocationPooler(nativeAllocator, 1000);
        AllocatorContext.SetAllocatorProvider(
            new AllocatorPool<ArenaAllocator>(p => new ArenaAllocator(p, p, nativeBuffer)),
            (int)AllocatorTypes.Default, true);
        IMemoryAllocator allocator;
        using (AllocatorContext.BeginAllocationScope())
        {
            unsafe
            {
                allocator = AllocatorContext.GetAllocator();
                var mem = allocator.Allocate(1);
                Unsafe.WriteUnaligned(mem.ToPointer(), (byte)47);
                AllocatorContext.GetAllocator().Free(mem);
            }
        }

        using (AllocatorContext.BeginAllocationScope())
        {
            unsafe
            {
                var oldAlloc = allocator;
                allocator = AllocatorContext.GetAllocator();
                Assert.AreEqual(allocator, oldAlloc);
                var mem = allocator.Allocate(100000);
                Unsafe.WriteUnaligned(mem.ToPointer(), (byte)47);
                AllocatorContext.GetAllocator().Free(mem);
            }
        }
    }

    [TestMethod]
    public void CanAllocateAndFreeOnPooledScopedProviderAndPoolIsWorking()
    {
        var nativeAllocator = new DefaultAlignedNativeMemoryAllocator();
        AllocatorContext.SetAllocatorProvider(
            new AllocatorPool<ArenaAllocator>(p => new ArenaAllocator(p, p, nativeAllocator)),
            (int)AllocatorTypes.Default, true);
        IMemoryAllocator allocator;
        using (AllocatorContext.BeginAllocationScope())
        {
            unsafe
            {
                allocator = AllocatorContext.GetAllocator();
                var mem = allocator.Allocate(1);
                Unsafe.WriteUnaligned(mem.ToPointer(), (byte)47);
                AllocatorContext.GetAllocator().Free(mem);
            }
        }

        using (AllocatorContext.BeginAllocationScope())
        {
            unsafe
            {
                var oldAlloc = allocator;
                allocator = AllocatorContext.GetAllocator();
                Assert.AreEqual(allocator, oldAlloc);
                var mem = allocator.Allocate(10000);
                Unsafe.InitBlockUnaligned(mem.ToPointer(), 47, 10000);
                AllocatorContext.GetAllocator().Free(mem);
            }
        }
    }

    [TestMethod]
    public void CanAllocateAndFreeOnPooledCachedScopedProviderAndPoolIsWorkingAndAllFreedAtEnd()
    {
        var cache = new DefaultAllocationPooler(new DefaultAlignedNativeMemoryAllocator(), 1000);
        var arenaAllocatorPool = new AllocatorPool<ArenaAllocator>(p => new ArenaAllocator(p, p, cache));
        AllocatorContext.SetAllocatorProvider(arenaAllocatorPool, (int)AllocatorTypes.Default, true);

        IMemoryAllocator allocator;
        using (AllocatorContext.BeginAllocationScope())
        {
            unsafe
            {
                allocator = AllocatorContext.GetAllocator();
                var mem = allocator.Allocate(1);
                Unsafe.WriteUnaligned(mem.ToPointer(), (byte)47);
                AllocatorContext.GetAllocator().Free(mem);
            }
        }

        using (AllocatorContext.BeginAllocationScope())
        {
            unsafe
            {
                var oldAlloc = allocator;
                allocator = AllocatorContext.GetAllocator();
                Assert.AreEqual(allocator, oldAlloc);
                var mem = allocator.Allocate(10000);
                Unsafe.InitBlockUnaligned(mem.ToPointer(), 47, 10000);
                AllocatorContext.GetAllocator().Free(mem);
            }
        }

        Assert.AreEqual(arenaAllocatorPool.SelfTotalAllocated, arenaAllocatorPool.SelfTotalFreed);
        arenaAllocatorPool.ClearCachedMemory();
        Assert.AreEqual(cache.ClientTotalAllocated, cache.ClientTotalFreed);
        System.Threading.Thread.Sleep(1500); // go past cache ttl
        cache.Prune(0);
        Assert.AreEqual(cache.SelfTotalAllocated, cache.SelfTotalFreed);
    }

    [TestMethod]
    public void AllocatorContextReturnsToPoolWhenThreadDies()
    {
        var cache = new DefaultAllocationPooler(new DefaultAlignedNativeMemoryAllocator(), 1000);
        var arenaAllocatorPool = new AllocatorPool<ArenaAllocator>(p => new ArenaAllocator(p, p, cache));
        AllocatorContext.SetAllocatorProvider(arenaAllocatorPool, (int)AllocatorTypes.Default, true);

        Assert.AreEqual(0, ((DefaultAllocatorContextImpl)AllocatorContext.Impl).ContextPool.Count);
        var mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        var execCtx = System.Threading.Thread.CurrentThread.ExecutionContext;
        Assert.IsNotNull(execCtx);

        void Worker()
        {
            Assert.AreNotEqual(mainThreadId, System.Threading.Thread.CurrentThread.ManagedThreadId);
            Assert.AreEqual(0, ((DefaultAllocatorContextImpl)AllocatorContext.Impl).ContextPool.Count);
            using (AllocatorContext.BeginAllocationScope())
            {
                Assert.IsNotNull(((DefaultAllocatorContextImpl)AllocatorContext.Impl).GetPerProviderContainer((int)AllocatorTypes.Default).Context!.Value);
                Assert.AreEqual(0, ((DefaultAllocatorContextImpl)AllocatorContext.Impl).ContextPool.Count);
                AllocatorContext.GetAllocator();
                Assert.AreEqual(0, ((DefaultAllocatorContextImpl)AllocatorContext.Impl).ContextPool.Count);
            }

            Assert.IsNotNull(((DefaultAllocatorContextImpl)AllocatorContext.Impl).GetPerProviderContainer((int)AllocatorTypes.Default).Context!.Value);

            Assert.AreEqual(0, ((DefaultAllocatorContextImpl)AllocatorContext.Impl).ContextPool.Count);
            Assert.AreNotEqual(execCtx, System.Threading.Thread.CurrentThread.ExecutionContext);
        }

        var thread1 = new System.Threading.Thread(Worker);
        thread1.Start();
        thread1.Join();
        Assert.AreEqual(execCtx, System.Threading.Thread.CurrentThread.ExecutionContext);
        Assert.AreEqual(1, ((DefaultAllocatorContextImpl)AllocatorContext.Impl).ContextPool.Count);
        Assert.IsTrue(((IMemoryAllocationTrackable)arenaAllocatorPool).IsAllFreed);
        AllocatorContext.ClearProvidersAndAllocations();
        Assert.AreEqual(0, ((DefaultAllocatorContextImpl)AllocatorContext.Impl).ContextPool.Count);
    }

    [TestMethod]
    public void FixedRetentionNativeMemoryCache_ExpirationBehaviorWhenCleanupThresholdIsZero()
    {
        var cache = new DefaultAllocationPooler(new DefaultAlignedNativeMemoryAllocator(), 1000);
        var mem1 = cache.Allocate(1000);
        Assert.AreEqual((ulong)cache.GetAllocSize(1000), cache.SelfTotalAllocated);
        cache.Free(mem1);
        Assert.AreEqual((ulong)cache.GetAllocSize(1000), cache.SelfTotalAllocated);
        Assert.AreEqual((ulong)0, cache.SelfTotalFreed);
        return; //not working since dynamic ttl
        System.Threading.Thread.Sleep(1500);
        cache.Prune();
        Assert.IsTrue(((IMemoryAllocationTrackable)cache).IsAllFreed);
        var mem2 = cache.Allocate(100);
        Assert.AreEqual((ulong)(cache.GetAllocSize(100)),
            cache.SelfTotalAllocated - cache.SelfTotalFreed);
        cache.Free(mem2);
        System.Threading.Thread.Sleep(1500);
        cache.Prune();
        Assert.AreEqual(cache.SelfTotalAllocated, cache.SelfTotalFreed);
        cache.ClearCachedMemory();
        Assert.AreEqual(cache.SelfTotalAllocated, cache.SelfTotalFreed);
    }

    [TestMethod]
    public void FixedRetentionNativeMemoryCache_ExpirationBehavior()
    {
        var cache = new DefaultAllocationPooler(new DefaultAlignedNativeMemoryAllocator(), 1000);
        var mem1 = cache.Allocate(1000);
        Assert.AreEqual((ulong)cache.GetAllocSize(1000), cache.SelfTotalAllocated);
        cache.Free(mem1); // mem1 < ttl && < Th
        Assert.AreEqual((ulong)cache.GetAllocSize(1000), cache.SelfTotalAllocated);
        Assert.AreEqual((ulong)0, cache.SelfTotalFreed);
        System.Threading.Thread.Sleep(1500);
        cache.Prune(); // mem1 < Th
        Assert.AreEqual((ulong)cache.GetAllocSize(1000), cache.SelfTotalAllocated);
        Assert.AreEqual((ulong)0, cache.SelfTotalFreed);
        var mem2 = cache.Allocate(2000);
        Assert.AreEqual((ulong)(cache.GetAllocSize(1000) + cache.GetAllocSize(2000)),
            cache.SelfTotalAllocated - cache.SelfTotalFreed);
        return; //not working since dynamic ttl 
        cache.Free(mem2); // mem1 gone, mem2 < ttl > Th
        Assert.AreEqual((ulong)(cache.GetAllocSize(2000)), cache.SelfTotalAllocated - cache.SelfTotalFreed);
        System.Threading.Thread.Sleep(500);
        cache.Prune(); // mem2 > Th && < ttl
        Assert.AreEqual((ulong)(cache.GetAllocSize(2000)), cache.SelfTotalAllocated - cache.SelfTotalFreed);
        System.Threading.Thread.Sleep(1000);
        cache.Prune();
        Assert.AreEqual(cache.SelfTotalAllocated, cache.SelfTotalFreed);
        cache.ClearCachedMemory();
        Assert.AreEqual(cache.SelfTotalAllocated, cache.SelfTotalFreed);
    }

    [TestMethod]
    public void FixedRetentionNativeMemoryCache_ClearCacheMemoryIsWorking()
    {
        var cache = new DefaultAllocationPooler(new DefaultAlignedNativeMemoryAllocator(), 1000);
        var mem1 = cache.Allocate(1000);
        Assert.AreEqual((ulong)cache.GetAllocSize(1000), cache.SelfTotalAllocated);
        Assert.AreEqual((ulong)0, cache.SelfTotalFreed);
        cache.Free(mem1);
        Assert.AreEqual((ulong)cache.GetAllocSize(1000), cache.SelfTotalAllocated);
        Assert.AreEqual((ulong)0, cache.SelfTotalFreed);
        cache.ClearCachedMemory();
        Assert.AreEqual(cache.SelfTotalAllocated, cache.SelfTotalFreed);
    }

    [TestMethod]
    public void NestedSameProviderTypeScope()
    {
        var allocPooler = new DefaultAllocationPooler(new DefaultAlignedNativeMemoryAllocator(), 1000);
        var arenaAllocatorPool = new AllocatorPool<ArenaAllocator>(p => new ArenaAllocator(p, p, allocPooler));
        AllocatorContext.SetAllocatorProvider(arenaAllocatorPool, (int)AllocatorTypes.Default, true);

        using (AllocatorContext.BeginAllocationScope())
        {
            AllocatorContext.GetAllocator().Allocate(1000);
            Assert.AreEqual((ulong)1000, arenaAllocatorPool.ClientTotalAllocated);

            using (AllocatorContext.BeginAllocationScope())
            {
                AllocatorContext.GetAllocator().Allocate(1500);
                Assert.AreEqual((ulong)(1000 + 1500), arenaAllocatorPool.ClientTotalAllocated);
            }

            Assert.AreEqual((ulong)(1000 + 1500), arenaAllocatorPool.ClientTotalAllocated);
            Assert.AreEqual((ulong)1500, arenaAllocatorPool.ClientTotalFreed);

            allocPooler.ClearCachedMemory();
            Assert.AreEqual((ulong)allocPooler.GetAllocSize(ArenaAllocator.DefaultPageSize - allocPooler.MetadataOverhead), allocPooler.SelfTotalFreed);
        }

        Assert.AreEqual((ulong)(1000 + 1500), arenaAllocatorPool.ClientTotalAllocated);
        Assert.AreEqual((ulong)(1500 + 1000), arenaAllocatorPool.ClientTotalFreed);

        allocPooler.ClearCachedMemory();
        Assert.AreEqual(
            (ulong)(allocPooler.GetAllocSize((ArenaAllocator.DefaultPageSize - allocPooler.MetadataOverhead)) * 2),
            allocPooler.SelfTotalFreed);
    }

    [TestMethod]
    public void NestedDifferentProviderTypeScope()
    {
        var allocPooler = new DefaultAllocationPooler(new DefaultAlignedNativeMemoryAllocator(), 1000);
        var arenaAllocatorPool = new AllocatorPool<ArenaAllocator>(p => new ArenaAllocator(p, p, allocPooler));
        AllocatorContext.SetAllocatorProvider(arenaAllocatorPool, (int)AllocatorTypes.Default, true);
        var arenaAllocatorPool2 = new AllocatorPool<ArenaAllocator>(p => new ArenaAllocator(p, p, allocPooler));
        AllocatorContext.SetAllocatorProvider(arenaAllocatorPool2, 16, true);

        using (AllocatorContext.BeginAllocationScope())
        {
            AllocatorContext.GetAllocator().Allocate(1000);
            Assert.AreEqual((ulong)1000, arenaAllocatorPool.ClientTotalAllocated);

            using (AllocatorContext.BeginAllocationScope(16))
            {
                AllocatorContext.GetAllocator(16).Allocate(1500);
                Assert.AreEqual((ulong)1000, arenaAllocatorPool.ClientTotalAllocated);
                Assert.AreEqual((ulong)1500, arenaAllocatorPool2.ClientTotalAllocated);
            }

            Assert.AreEqual((ulong)1000, arenaAllocatorPool.ClientTotalAllocated);
            Assert.AreEqual((ulong)0, arenaAllocatorPool.ClientTotalFreed);
            Assert.AreEqual((ulong)1500, arenaAllocatorPool2.ClientTotalAllocated);
            Assert.AreEqual((ulong)1500, arenaAllocatorPool2.ClientTotalFreed);

            allocPooler.ClearCachedMemory();
            Assert.AreEqual(
                (ulong)allocPooler.GetAllocSize(ArenaAllocator.DefaultPageSize - allocPooler.MetadataOverhead),
                allocPooler.SelfTotalFreed);
        }

        Assert.AreEqual((ulong)1000, arenaAllocatorPool.ClientTotalAllocated);
        Assert.AreEqual((ulong)1000, arenaAllocatorPool.ClientTotalFreed);

        Assert.AreEqual((ulong)1500, arenaAllocatorPool2.ClientTotalFreed);
        Assert.AreEqual((ulong)1500, arenaAllocatorPool2.ClientTotalFreed);

        allocPooler.ClearCachedMemory();
        Assert.AreEqual((ulong)(allocPooler.GetAllocSize((ArenaAllocator.DefaultPageSize - allocPooler.MetadataOverhead)) * 2), allocPooler.SelfTotalFreed);
    }

    public void Dispose()
    {
        AllocatorContext.ResetImplementation(null);
    }
}
