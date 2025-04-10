using Microsoft.VisualStudio.TestTools.UnitTesting;
using NullGC.TestCommons;

namespace NullGC.Collections.Tests
{
    [TestClass]
    public class ValueArrayTests : AssertMemoryAllFreedBase
    {
        public static TestContext SharedTestContext { get; set; }
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SharedTestContext = context;
        }

        public ValueArrayTests()
            : base(new MSTestOutputHelper(SharedTestContext), false)
        {
        }

        [TestMethod]
        public void DefaultArrayFacts()
        {
            ValueArray<int> defaultArr = default;
            Assert.IsFalse(defaultArr.IsAllocated);
            Assert.IsTrue(EqualityComparer<ValueArray<int>>.Default.Equals(ValueArray<int>.Empty, defaultArr));
            CollectionAssert.AreEqual(new int[0], defaultArr.ToArray());
            foreach (ref var item in defaultArr)
            {
                Assert.Fail("Should be empty.");
            }
            defaultArr.Dispose();
        }

        [TestMethod]
        public void SpecificLengthArrayCanBeConstructed()
        {
            var arr = new ValueArray<int>(0);
            arr.Dispose();
            arr = new ValueArray<int>(1);
            arr.Dispose();
            arr = new ValueArray<int>(2);
            arr.Dispose();
            arr = new ValueArray<int>(200);
            arr.Dispose();
            arr = new ValueArray<int>(20000);
            arr.Dispose();
            arr = new ValueArray<int>(500000);
            arr.Dispose();
            arr = new ValueArray<int>(50000000);
            arr.Dispose();
        }

        [TestMethod]
        public void ValueSetShouldBePreserved()
        {
            var count = 10000000;
            var arr = new ValueArray<int>(count);
            for (var i = 0; i < count; i++)
            {
                arr[i] = i;
            }
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(i, arr[i]);
            }
            arr.Dispose();
        }
    }
}
