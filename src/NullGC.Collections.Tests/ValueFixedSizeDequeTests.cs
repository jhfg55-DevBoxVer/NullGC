using NullGC.TestCommons;

namespace NullGC.Collections.Tests
{
    [TestClass]
    public class ValueFixedSizeDequeTests : AssertMemoryAllFreedBase
    {
        public static TestContext SharedTestContext { get; set; }
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SharedTestContext = context;
        }

        public ValueFixedSizeDequeTests()
            : base(new MSTestOutputHelper(SharedTestContext), false)
        {
        }

        [TestMethod]
        public void ZeroCapacityQueue()
        {
            var q = new ValueFixedSizeDeque<int>();
            Assert.AreEqual(0, q.Capacity);
            CollectionAssert.AreEqual(new int[0], q.ToArray());
            Assert.IsTrue(q.IsEmpty);
            Assert.IsTrue(q.IsFull);
        }

        [TestMethod]
        public void EmptyQueue()
        {
            var q = new ValueFixedSizeDeque<int>(5);
            CollectionAssert.AreEqual(new int[0], q.ToArray());
            Assert.IsTrue(q.IsEmpty);
            Assert.IsFalse(q.IsFull);
            Assert.ThrowsException<InvalidOperationException>(() => { var _ = q.HeadRef; });
            Assert.ThrowsException<InvalidOperationException>(() => { var _ = q.TailRef; });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var _ = q.GetNthItemRefFromHead(0); });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var _ = q.GetNthItemRefFromTail(0); });
            Assert.ThrowsException<InvalidOperationException>(() => { q.RemoveFront(out _); });
            Assert.ThrowsException<InvalidOperationException>(() => { q.RemoveBack(out _); });
            q.Dispose();
        }

        [TestMethod]
        public void ThrowOnAddWhenFull()
        {
            var q = new ValueFixedSizeDeque<int>(1);
            q.AddBack(1);
            Assert.ThrowsException<InvalidOperationException>(() => { q.AddBack(1); });
            Assert.ThrowsException<InvalidOperationException>(() => { q.AddFront(1); });
            q.Dispose();
        }

        [TestMethod]
        public void CanAddRemovePushFrontBack()
        {
            var q = new ValueFixedSizeDeque<int>(5);
            q.AddBack(1);
            q.AddBack(2);
            CollectionAssert.AreEqual(new int[] { 1, 2 }, q.ToArray());
            q.AddBack(3);
            q.AddBack(4);
            q.AddBack(5);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4, 5 }, q.ToArray());
            q.RemoveBack(out var e);
            Assert.AreEqual(5, e);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4 }, q.ToArray());
            q.AddBack(6);
            CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4, 6 }, q.ToArray());
            q.RemoveFront(out e);
            Assert.AreEqual(1, e);
            CollectionAssert.AreEqual(new int[] { 2, 3, 4, 6 }, q.ToArray());
            q.AddFront(7);
            CollectionAssert.AreEqual(new int[] { 7, 2, 3, 4, 6 }, q.ToArray());
            q.PushBack(1, out e);
            CollectionAssert.AreEqual(new int[] { 2, 3, 4, 6, 1 }, q.ToArray());
            Assert.AreEqual(7, e);
            q.PushFront(8, out e);
            CollectionAssert.AreEqual(new int[] { 8, 2, 3, 4, 6 }, q.ToArray());
            Assert.AreEqual(1, e);
            q.PushBack(1, out e);
            CollectionAssert.AreEqual(new int[] { 2, 3, 4, 6, 1 }, q.ToArray());
            q.Clear();
            CollectionAssert.AreEqual(new int[0], q.ToArray());
            q.PushBack(1, out e);
            q.PushBack(1, out e);
            CollectionAssert.AreEqual(new int[] { 1, 1 }, q.ToArray());

            q.Dispose();
        }

        [TestMethod]
        public void GetNthFromHeadOrTail()
        {
            var q = new ValueFixedSizeDeque<int>(5);
            q.AddBack(1);
            Assert.AreEqual(1, q.GetNthItemRefFromHead(0));
            Assert.AreEqual(1, q.GetNthItemRefFromTail(0));
            q.AddBack(2);
            q.AddBack(3);
            q.AddBack(4);
            q.AddBack(5);
            Assert.AreEqual(1, q.GetNthItemRefFromHead(0));
            Assert.AreEqual(5, q.GetNthItemRefFromTail(0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var _ = q.GetNthItemRefFromHead(-1); });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var _ = q.GetNthItemRefFromTail(-1); });
            q.RemoveFront(out _);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var _ = q.GetNthItemRefFromHead(4); });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { var _ = q.GetNthItemRefFromTail(4); });
            q.AddBack(6);

            CollectionAssert.AreEqual(new int[] { 2, 3, 4, 5, 6 }, q.ToArray());
            Assert.AreEqual(6, q.GetNthItemRefFromHead(4));
            Assert.AreEqual(2, q.GetNthItemRefFromTail(4));

            q.Dispose();
        }
    }
}
