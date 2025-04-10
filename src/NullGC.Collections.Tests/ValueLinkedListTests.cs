using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NullGC.Linq;
using NullGC.TestCommons;

namespace NullGC.Collections.Tests
{
    [TestClass]
    public class ValueLinkedListTests : AssertMemoryAllFreedBase
    {
        public static TestContext SharedTestContext { get; set; }
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            SharedTestContext = context;
        }

        public ValueLinkedListTests()
            : base(new MSTestOutputHelper(SharedTestContext), false)
        {
        }

        [TestMethod]
        public void CanAddRemoveFirst()
        {
            var lst = new ValueLinkedList<int>();
            lst.AddFront(100);
            Assert.AreEqual(100, lst.HeadRefOrNullRef.Value);
            lst.RemoveFront();
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.HeadRefOrNullRef));
            lst.AddFront(200);
            Assert.AreEqual(200, lst.HeadRefOrNullRef.Value);
            lst.AddFront(300);
            Assert.AreEqual(300, lst.HeadRefOrNullRef.Value);
            lst.RemoveFront();
            Assert.AreEqual(200, lst.HeadRefOrNullRef.Value);
            lst.RemoveFront();
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.HeadRefOrNullRef));
            AssertEx.Throws<InvalidOperationException, ValueLinkedList<int>>(
                (ref ValueLinkedList<int> a) => a.RemoveFront(), ref lst);
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.HeadRefOrNullRef));
            lst.AddFront(400);
            Assert.AreEqual(400, lst.HeadRefOrNullRef.Value);
            lst.Dispose();
        }

        [TestMethod]
        public void CanAddRemoveLast()
        {
            var lst = new ValueLinkedList<int>();
            lst.AddBack(100);
            Assert.AreEqual(100, lst.TailRefOrNullRef.Value);
            lst.RemoveBack();
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.TailRefOrNullRef));
            lst.AddBack(200);
            Assert.AreEqual(200, lst.TailRefOrNullRef.Value);
            lst.AddBack(300);
            Assert.AreEqual(300, lst.TailRefOrNullRef.Value);
            lst.RemoveBack();
            Assert.AreEqual(200, lst.TailRefOrNullRef.Value);
            lst.RemoveBack();
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.TailRefOrNullRef));
            AssertEx.Throws<InvalidOperationException, ValueLinkedList<int>>(
                (ref ValueLinkedList<int> a) => a.RemoveBack(), ref lst);
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.TailRefOrNullRef));
            lst.AddBack(400);
            Assert.AreEqual(400, lst.TailRefOrNullRef.Value);
            lst.Dispose();
        }

        [TestMethod]
        public void CanAddRemoveFirstLastMixed()
        {
            var lst = new ValueLinkedList<int>();
            lst.AddFront(100);
            Assert.AreEqual(100, lst.TailRefOrNullRef.Value);
            lst.RemoveBack();
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.TailRefOrNullRef));
            lst.AddFront(200);
            Assert.AreEqual(200, lst.TailRefOrNullRef.Value);
            lst.AddFront(300);
            Assert.AreEqual(300, lst.HeadRefOrNullRef.Value);
            Assert.AreEqual(200, lst.TailRefOrNullRef.Value);
            lst.RemoveBack();
            Assert.AreEqual(300, lst.TailRefOrNullRef.Value);
            lst.RemoveFront();
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.HeadRefOrNullRef));
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.TailRefOrNullRef));
            lst.AddBack(1);
            lst.AddFront(2);
            lst.AddBack(3);
            CollectionAssert.AreEqual(new int[] { 2, 1, 3 }, lst.LinqRef().Select(x => x.Value).ToArray());
            lst.RemoveFront();
            CollectionAssert.AreEqual(new int[] { 1, 3 }, lst.LinqRef().Select(x => x.Value).ToArray());
            lst.RemoveFront();
            lst.RemoveFront();
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.HeadRefOrNullRef));
            Assert.IsTrue(Unsafe.IsNullRef(ref lst.TailRefOrNullRef));
            lst.Dispose();
        }

        [TestMethod]
        public void GetNthNode()
        {
            var lst = new ValueLinkedList<int>();
            lst.AddFront(1);
            lst.AddFront(2);
            lst.AddFront(3);
            lst.AddFront(4);
            lst.AddFront(5);
            Assert.AreEqual(2, lst.GetNthNodeRefFromHead(3).Value);
            Assert.AreEqual(1, lst.GetNthNodeRefFromHead(4).Value);
            AssertEx.Throws<ArgumentOutOfRangeException, ValueLinkedList<int>>((ref ValueLinkedList<int> x) =>
                x.GetNthNodeRefFromHead(5), ref lst);
            AssertEx.Throws<ArgumentOutOfRangeException, ValueLinkedList<int>>((ref ValueLinkedList<int> x) =>
                x.GetNthNodeRefFromHead(-1), ref lst);
            Assert.AreEqual(5, lst.GetNthNodeRefFromHead(0).Value);
            Assert.AreEqual(4, lst.GetNthNodeRefFromHead(1).Value);

            Assert.AreEqual(4, lst.GetNthNodeRefFromTail(3).Value);
            Assert.AreEqual(5, lst.GetNthNodeRefFromTail(4).Value);
            AssertEx.Throws<ArgumentOutOfRangeException, ValueLinkedList<int>>((ref ValueLinkedList<int> x) =>
                x.GetNthNodeRefFromTail(5), ref lst);
            AssertEx.Throws<ArgumentOutOfRangeException, ValueLinkedList<int>>((ref ValueLinkedList<int> x) =>
                x.GetNthNodeRefFromTail(-1), ref lst);
            Assert.AreEqual(1, lst.GetNthNodeRefFromTail(0).Value);
            Assert.AreEqual(2, lst.GetNthNodeRefFromTail(1).Value);
            lst.Dispose();
        }

        [TestMethod]
        public void CanRemoveAny()
        {
            var lst = new ValueLinkedList<int>();
            lst.AddFront(1);
            lst.AddFront(2);
            lst.AddFront(3);
            lst.AddFront(4);
            lst.AddFront(5);
            CollectionAssert.AreEqual(new int[] { 5, 4, 3, 2, 1 }, lst.LinqRef().Select(x => x.Value).ToArray());

            // remove 2
            lst.Remove(lst.GetNthNodeRefFromHead(3).Index);
            CollectionAssert.AreEqual(new int[] { 5, 4, 3, 1 }, lst.LinqRef().Select(x => x.Value).ToArray());
            // remove 4
            lst.Remove(lst.GetNthNodeRefFromHead(1).Index);
            CollectionAssert.AreEqual(new int[] { 5, 3, 1 }, lst.LinqRef().Select(x => x.Value).ToArray());
            lst.Remove(lst.GetNthNodeRefFromHead(0).Index);
            CollectionAssert.AreEqual(new int[] { 3, 1 }, lst.LinqRef().Select(x => x.Value).ToArray());
            lst.Remove(lst.GetNthNodeRefFromHead(1).Index);
            CollectionAssert.AreEqual(new int[] { 3 }, lst.LinqRef().Select(x => x.Value).ToArray());
            lst.Remove(lst.GetNthNodeRefFromHead(0).Index);
            CollectionAssert.AreEqual(new int[] { 2, 1, 3 }, lst.LinqRef().Select(x => x.Value).ToArray());


            lst.Dispose();
        }

        [TestMethod]
        public void CanMove()
        {
            var lst = new ValueLinkedList<int>();
            lst.AddFront(1);
            lst.AddFront(2);
            lst.AddFront(3);
            lst.AddFront(4);
            lst.AddFront(5);
            CollectionAssert.AreEqual(new int[] { 5, 4, 3, 2, 1 }, lst.LinqRef().Select(x => x.Value).ToArray());

            lst.Move(lst.TailRefOrNullRef.Index, lst.HeadRefOrNullRef.Index);
            CollectionAssert.AreEqual(new int[] { 1, 5, 4, 3, 2 }, lst.LinqRef().Select(x => x.Value).ToArray());
            lst.Move(lst.HeadRefOrNullRef.Index, ValueLinkedList.LastPosition);
            CollectionAssert.AreEqual(new int[] { 5, 4, 3, 2, 1 }, lst.LinqRef().Select(x => x.Value).ToArray());

            lst.Move(lst.HeadRefOrNullRef.Next, lst.TailRefOrNullRef.Previous);
            CollectionAssert.AreEqual(new int[] { 5, 3, 4, 2, 1 }, lst.LinqRef().Select(x => x.Value).ToArray());

            lst.Move(lst.TailRefOrNullRef.Previous, ValueLinkedList.FirstPosition);
            CollectionAssert.AreEqual(new int[] { 2, 5, 3, 4, 1 }, lst.LinqRef().Select(x => x.Value).ToArray());

            lst.Dispose();
        }

        private struct StructForUsing : IDisposable
        {
            public int SomeInt;

            public void Dispose()
            {
                if (SomeInt != 100)
                {
                    throw new InvalidOperationException("Copied.");
                }
                SomeInt = -1;
            }
        }

        private static void MutateStruct(ref StructForUsing s)
        {
            s.SomeInt = 100;
        }

        [TestMethod]
        public void UsingMadeACopy()
        {
            bool copied = false;
            var s = new StructForUsing();
            try
            {
                using (s)
                {
                    MutateStruct(ref s);
                    Assert.AreEqual(100, s.SomeInt);
                }
            }
            catch (InvalidOperationException ex) when (ex.Message == "Copied.")
            {
                copied = true;
            }
            catch (Exception)
            {
                // ignored.
            }
            Assert.AreEqual(100, s.SomeInt);
            Assert.IsTrue(copied);
        }

        private static void MutateStructIn(in StructForUsing s)
        {
            Unsafe.AsRef(in s).SomeInt = 100;
        }

        [TestMethod]
        public void UsingMadeACopyAnyway()
        {
            bool copied = false;
            var s = new StructForUsing();
            try
            {
                using (s)
                {
                    MutateStructIn(in s);
                    Assert.AreEqual(100, s.SomeInt);
                }
            }
            catch (InvalidOperationException ex) when (ex.Message == "Copied.")
            {
                copied = true;
            }
            catch (Exception)
            {
                // ignored.
            }
            Assert.AreEqual(100, s.SomeInt);
            Assert.IsTrue(copied);
        }

        [TestMethod]
        public void UsingVarNotMakeACopy()
        {
            bool copied = false;
            try
            {
                using var s = new StructForUsing();
                MutateStructIn(in s);
                Assert.AreEqual(100, s.SomeInt);
            }
            catch (InvalidOperationException ex) when (ex.Message == "Copied.")
            {
                copied = true;
            }
            catch (Exception)
            {
                // ignored.
            }
            Assert.IsFalse(copied);
        }

        [TestMethod]
        public void UsingVarNotMakeACopy2()
        {
            bool copied = false;
            try
            {
                using (var s = new StructForUsing())
                {
                    MutateStructIn(in s);
                    Assert.AreEqual(100, s.SomeInt);
                }
            }
            catch (InvalidOperationException ex) when (ex.Message == "Copied.")
            {
                copied = true;
            }
            catch (Exception)
            {
                // ignored.
            }
            Assert.IsFalse(copied);
        }
    }
}
