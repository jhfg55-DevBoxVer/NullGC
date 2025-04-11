using Cathei.LinqGen;
using NullGC.Collections;
using NullGC.Linq.Enumerators;
using NullGC.TestCommons;

namespace NullGC.Linq.Tests;
// 添加占位符类解决 TestContextPlaceholder 未定义的问题
internal static class TestContextPlaceholder
{
    public static TestContext Instance => null;
}
[TestClass]
public class LinqTests : AssertMemoryAllFreedBase
{
    private const int _count = 2000;
    private readonly int _bigStructMin;
    private readonly ValueArray<int> _emptyArray;
    private readonly int _smallStructMin;
    private BigStruct<int, float>[] _bigStructArr;
    private ValueArray<BigStruct<int, float>> _bigStructValArr;
    private int[] _intArr;
    private SmallStruct<int, float>[] _smallStructArr;
    private ValueArray<SmallStruct<int, float>> _smallStructValArr;
    private ValueArray<int> _valIntArr;
    private ValueList<int> _valList1;

    public TestContext TestContext { get; set; }

    public LinqTests()
    : base(new MSTestOutputHelper(TestContextPlaceholder.Instance), false, false) // Use placeholder; MSTest will automatically set TestContext.
    {
        // Backup: In MSTest you can also initialize MSTestOutputHelper in the TestInitialize method by passing in TestContext.
        _emptyArray = ValueArray<int>.Empty;
        _valList1 = new ValueList<int>(0) { 7, 0, 4, 5, 6, 1, 2, 3, 8, 9 };
        _intArr = new int[_count];
        _valIntArr = new ValueArray<int>(_count);
        _bigStructArr = new BigStruct<int, float>[_count];
        _smallStructArr = new SmallStruct<int, float>[_count];
        _bigStructValArr = new ValueArray<BigStruct<int, float>>(_count);
        _smallStructValArr = new ValueArray<SmallStruct<int, float>>(_count);

        RandomFiller.FillArrayRandom<int, int[]>(_intArr);
        RandomFiller.FillArrayRandom<int, ValueArray<int>>(_valIntArr);
        RandomFiller.FillArrayRandom(_bigStructArr, (ref BigStruct<int, float> t1) => ref t1.Key);
        RandomFiller.FillArrayRandom(_smallStructArr, (ref SmallStruct<int, float> t1) => ref t1.Key);
        RandomFiller.FillArrayRandom(_bigStructValArr, (ref BigStruct<int, float> t1) => ref t1.Key);
        RandomFiller.FillArrayRandom(_smallStructValArr, (ref SmallStruct<int, float> t1) => ref t1.Key);
        if (!_intArr.SequenceEqual(_valIntArr))
            throw new InvalidOperationException();
        if (!_bigStructArr.SequenceEqual(_bigStructValArr))
            throw new InvalidOperationException();
        if (!_smallStructArr.SequenceEqual(_smallStructValArr))
            throw new InvalidOperationException();

        _bigStructMin = BigStructArrSystemLinqWhereOrderByTakeAverage();
        _smallStructMin = SmallStructArrSystemLinqWhereOrderByTakeAverage();
    }


    [TestCleanup]
    public override void Dispose()
    {
        _bigStructValArr.Dispose();
        _valList1.Dispose();
        _valIntArr.Dispose();
        _smallStructValArr.Dispose();
        base.Dispose();
    }

    [TestMethod]
    public void WhereOnEmptyArray()
    {
        var result = _emptyArray.LinqRef().Where((in int x) => true);
        Assert.IsFalse(result.Any());
    }

    [TestMethod]
    public void WhereWithArgs()
    {
        var expected = new int[] { 7, 6, 8, 9 };
        var result = _valList1.LinqRef().Where((in int x, int y) => x > y, 5).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Where()
    {
        var expected = new int[] { 7, 6, 8, 9 };
        var result = _valList1.LinqRef().Where((in int x) => x > 5).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void WhereSelect()
    {
        var expected = new float[] { 7 * 0.5f, 6 * 0.5f, 8 * 0.5f, 9 * 0.5f };
        var result = _valList1.LinqRef().Where((in int x) => x > 5)
            .Select((in int x, float y) => x * y, 0.5f)
            .ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderBy_LinqRef_In()
    {
        var expected = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var result = _valList1.LinqRef().OrderBy((in int x) => x).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderByDesc_LinqRef_In()
    {
        var expected = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
        var result = _valList1.LinqRef().OrderByDescending((in int x) => x).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderBy_LinqRef_In_Comparer()
    {
        var expected = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
        var result = _valList1.LinqRef().OrderBy((in int x) => x, (a, b) => b - a).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderByDesc_LinqRef_In_Comparer()
    {
        var expected = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var result = _valList1.LinqRef().OrderByDescending((in int x) => x, (a, b) => b - a).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderBy_LinqRef_Arg()
    {
        var expected = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
        var result = _valList1.LinqValue().OrderBy((x, a) => x * a, -1).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderByDesc_LinqRef_Arg()
    {
        var expected = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var result = _valList1.LinqValue().OrderByDescending((x, a) => x * a, -1).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderBy_LinqRef_In_Arg()
    {
        var expected = new int[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
        var result = _valList1.LinqRef().OrderBy((in int x, int a) => x * a, -1).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderByDesc_LinqRef_In_Arg()
    {
        var expected = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var result = _valList1.LinqRef().OrderByDescending((in int x, int a) => x * a, -1).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderByMulti_LinqRef_SingleSorter()
    {
        var expected = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var result = _valList1.LinqRef().OrderBy(OrderBy.Ascending((in int x) => x)).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderByMulti_LinqRef_MultiSorter()
    {
        var expected = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var result = _valList1.LinqRef().OrderBy(OrderBy.Ascending((in int x) => x, OrderBy.Descending((in int x) => x))).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void OrderByMultipleEnumeration()
    {
        var linq = _valList1.LinqRef().OrderBy((in int x) => x);
        CollectionAssert.AreEqual(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, linq.ToArray());
        CollectionAssert.AreEqual(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, linq.ToArray());
        CollectionAssert.AreEqual(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, linq.ToArray());
    }

    [TestMethod]
    public void Take()
    {
        var expected = new int[] { 0, 1, 2, 3 };
        var result = _valList1.LinqRef().OrderBy((in int x) => x).Take(4).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TakeZero()
    {
        Assert.IsFalse(_valList1.LinqRef().OrderBy((in int x) => x).Take(0).Any());
    }

    [TestMethod]
    public void TakeMinus()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            _valList1.LinqRef().OrderBy((in int x) => x).Take(-10));
    }

    [TestMethod]
    public void TakeTooMuch()
    {
        var expected = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var result = _valList1.LinqRef().OrderBy((in int x) => x).Take(100).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void Skip()
    {
        var expected = new int[] { 4, 5, 6, 7, 8, 9 };
        var result = _valList1.LinqRef().OrderBy((in int x) => x).Skip(4).ToArray();
        CollectionAssert.AreEqual(expected, result);
    }

    [TestMethod]
    public void BigStructValArrNullGCLinqRefWhereOrderByTakeAverageValue()
    {
        Assert.AreEqual(_smallStructMin, _bigStructValArr.LinqValue().Where(x => x.Key > 100000)
            .OrderBy(x => x.Key)
            .Take(500).Select(x => x.Key).Min());
    }

    [TestMethod]
    public void SmallStructValArrNullGCLinqRefWhereOrderByTakeAverageValue()
    {
        Assert.AreEqual(_smallStructMin, _smallStructValArr.LinqValue().Where(x => x.Key > 100000)
            .OrderBy(x => x.Key)
            .Take(500).Select(x => x.Key).Min());
    }

    [TestMethod]
    public void BigStructValArrNullGCLinqRefWhereOrderByTakeAveragePtr()
    {
        unsafe
        {
            Assert.AreEqual(_bigStructMin, _bigStructValArr.LinqPtr().Where(x => x->Key > 100000)
                .OrderBy(x => x->Key)
                .Take(500).Select(x => x->Key).Min());
        }
    }

    [TestMethod]
    public void BigStructValArrNullGCLinqRefWhereOrderByTakeAverageRef()
    {
        Assert.AreEqual(_bigStructMin, _bigStructValArr.LinqRef().Where((in BigStruct<int, float> x) => x.Key > 100000)
            .OrderBy((in BigStruct<int, float> x) => x.Key)
            .Take(500).Select((in BigStruct<int, float> x) => x.Key).Min());
    }

    [TestMethod]
    public void SmallStructValArrNullGCLinqRefWhereOrderByTakeAverageRef()
    {
        Assert.AreEqual(_smallStructMin, _smallStructValArr.LinqRef()
            .Where((in SmallStruct<int, float> x) => x.Key > 100000)
            .OrderBy((in SmallStruct<int, float> x) => x.Key)
            .Take(500).Select((in SmallStruct<int, float> x) => x.Key).Min());
    }

    public int BigStructArrSystemLinqWhereOrderByTakeAverage()
    {
        return _bigStructArr.Where(x => x.Key > 100000).OrderBy(x => x.Key).Take(500).Select(x => x.Key).Min();
    }

    public int SmallStructArrSystemLinqWhereOrderByTakeAverage()
    {
        return _smallStructArr.Where(x => x.Key > 100000).OrderBy(x => x.Key).Take(500).Select(x => x.Key).Min();
    }

    [TestMethod]
    public void BigStructArrLinqGenWhereOrderByTakeAverage()
    {
        Assert.AreEqual(_bigStructMin,
            _bigStructArr.Gen().Where(x => x.Key > 100000).OrderBy(x => x.Key).Take(500).Select(x => x.Key).Min());
    }

    [TestMethod]
    public void FirstLastNoPredicateFacts()
    {
        Assert.AreEqual(7, _valList1.LinqRef().First());
        Assert.AreEqual(7, _valList1.LinqRef().FirstOrNullRef());
        Assert.AreEqual(7, _valList1.LinqRef().FirstOrDefault());
        Assert.AreEqual(7, _valList1.LinqValue().First());
        Assert.AreEqual(7, _valList1.LinqValue().FirstOrDefault());
        Assert.AreEqual(7, _valList1.LinqValue().First());
        Assert.AreEqual(9, _valList1.LinqRef().Last());
        Assert.AreEqual(9, _valList1.LinqRef().LastOrDefault());
        Assert.AreEqual(9, _valList1.LinqValue().Last());
        Assert.AreEqual(9, _valList1.LinqValue().LastOrDefault());
    }

    [TestMethod]
    public void FirstLastWithPredicateFacts()
    {
        Assert.AreEqual(8, _valList1.LinqRef().First((in int x) => x > 7));
        Assert.AreEqual(8, _valList1.LinqRef().FirstOrNullRef((in int x) => x > 7));
        Assert.AreEqual(8, _valList1.LinqRef().FirstOrDefault((in int x) => x > 7));
        Assert.AreEqual(8, _valList1.LinqValue().First(x => x > 7));
        Assert.AreEqual(8, _valList1.LinqValue().FirstOrDefault(x => x > 7));
        Assert.AreEqual(8, _valList1.LinqValue().First(x => x > 7));
        Assert.AreEqual(3, _valList1.LinqRef().Last((in int x) => x < 4));
        Assert.AreEqual(3, _valList1.LinqRef().LastOrDefault((in int x) => x < 4));
        Assert.AreEqual(3, _valList1.LinqValue().Last(x => x < 4));
        Assert.AreEqual(3, _valList1.LinqValue().LastOrDefault(x => x < 4));
    }

    [TestMethod]
    public void ValueFixedSizeDequeEnumeratorFacts()
    {
        using var q = new ValueFixedSizeDeque<int>(7) { 1, 2, 3, 4, 5, 6, 7 };
        CollectionAssert.AreEqual(new int[] { 3, 4, 5 }, q.LinqValue().Skip(2).Take(3).ToArray());
        CollectionAssert.AreEqual(new int[] { 3 }, q.LinqValue().Take(3).Skip(2).ToArray());
        Assert.IsFalse(q.LinqValue().Skip(2).Take(0).Any());
        Assert.IsFalse(q.LinqValue().Take(0).Skip(2).Any());
    }
}
