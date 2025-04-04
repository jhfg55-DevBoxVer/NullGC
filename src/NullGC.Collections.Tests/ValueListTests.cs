using NullGC.TestCommons;
using Xunit.Abstractions;

namespace NullGC.Collections.Tests;

public class ValueListTests : AssertMemoryAllFreedBase
{
    public ValueListTests(ITestOutputHelper logger) : base(logger, false, true)
    {
    }

    [Fact]
    public void DefaultListIsEmptyList()
    {
        UList<int> defaultList = default;
        Assert.False(defaultList.IsAllocated);
        Assert.True(EqualityComparer<UList<int>>.Default.Equals(UList<int>.Empty, defaultList));
        Assert.Empty(defaultList);
        foreach (ref var _ in defaultList) Assert.Fail("Should be empty.");
        defaultList.Dispose();
    }

    [Fact]
    public void SpecificCapacityListCanBeConstructed()
    {
        var arr = new UList<int>(0);
        arr.Dispose();
        arr = new UList<int>(1);
        arr.Dispose();
        arr = new UList<int>(2);
        arr.Dispose();
        arr = new UList<int>(200);
        arr.Dispose();
        arr = new UList<int>(20000);
        arr.Dispose();
        arr = new UList<int>(500000);
        arr.Dispose();
        arr = new UList<int>(50000000);
        arr.Dispose();
    }

    [Fact]
    public void ValueSetShouldBePreservedOnGrowingList()
    {
        var count = 10_000_000;
        var arr = new UList<int>();
        for (var i = 0; i < count; i++) arr.Add(i);

        for (var i = 0; i < count; i++) Assert.Equal(arr[i], i);

        arr.Dispose();
    }

    [Fact]
    public void IndexOfAndContainsFacts()
    {
        var list = new UList<int>(100);
        for (var i = 0; i < list.Capacity; i++) list.Add(i);
        for (var i = 0; i < list.Count; i++) Assert.Equal(i, list.IndexOf(i));

        Assert.False(list.IndexOf(1, 1, 10000) == -1);
        list.RemoveAt(list.Count - 1);
        Assert.Contains(list, x => x == list.Count - 1);
        Assert.DoesNotContain(list, x => x == list.Capacity - 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(99999, list.Count));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(99999, list.Count, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(99999, list.Capacity - 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(99999, list.Capacity - 1, 10000));

        for (var i = 0; i < list.Count; i++) Assert.Equal(i, list.LastIndexOf(i));

        var rand = new Random();
        for (var i = 0; i < list.Count; i++) list[i] = rand.Next(0, 50);

        list[47] = 11111;
        Assert.Equal(47, list.IndexOf(11111));
        Assert.Equal(47, list.LastIndexOf(11111));
#pragma warning disable xUnit2017
        Assert.True(list.Contains(11111));
#pragma warning restore xUnit2017
        list.Dispose();
    }
}