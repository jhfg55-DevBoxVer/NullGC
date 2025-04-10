using Microsoft.VisualStudio.TestTools.UnitTesting;
using NullGC.Diagnostics;
using NullGC.TestCommons;
using System.Threading;

    namespace NullGC.Collections.Tests
    {
        [TestClass]
        public class SlidingTimeWindowTests : AssertMemoryAllFreedBase
        {
            private readonly TestContext _testContext;

            public SlidingTimeWindowTests(TestContext testContext)
                : base(new NullGC.TestCommons.MSTestOutputHelper(testContext), false)
            {
                _testContext = testContext;
            }

            [TestMethod]
            public void GeneralFacts()
            {
                var stw = new SlidingTimeWindow<int>(1000, 5, default);

                while (Environment.TickCount64 % 1000 > 10) Thread.Yield();
                var w = stw.Update(1);
                Assert.IsTrue(w.Count == 1);
                Assert.IsTrue(w.Buckets.Count == 1);
                Assert.IsTrue(w.Sum == 1);
                Assert.IsTrue(w.Buckets.TailRef.Count == 1);
                Assert.IsTrue(w.Buckets.TailRef.Sum == 1);

                w = stw.Update(2);
                Assert.IsTrue(w.Count == 2);
                Assert.IsTrue(w.Buckets.Count == 1);
                Assert.IsTrue(w.Sum == 3);
                Assert.IsTrue(w.Buckets.TailRef.Count == 2);
                Assert.IsTrue(w.Buckets.TailRef.Sum == 3);

                Thread.Sleep(500); // 0.5秒
                w = stw.Update(1);
                Assert.IsTrue(w.Count == 3);
                Assert.IsTrue(w.Buckets.Count == 1);
                Assert.IsTrue(w.Sum == 4);
                Assert.IsTrue(w.Buckets.TailRef.Count == 3);
                Assert.IsTrue(w.Buckets.TailRef.Sum == 4);

                Thread.Sleep(1000); // 1.5秒
                w = stw.Update(1);
                Assert.IsTrue(w.Count == 4);
                Assert.IsTrue(w.Buckets.Count == 2);
                Assert.IsTrue(w.Sum == 5);
                Assert.IsTrue(w.Buckets.GetNthItemRefFromTail(1).Count == 3);
                Assert.IsTrue(w.Buckets.GetNthItemRefFromTail(1).Sum == 4);
                Assert.IsTrue(w.Buckets.TailRef.Count == 1);
                Assert.IsTrue(w.Buckets.TailRef.Sum == 1);

                Thread.Sleep(2000); // 3.5秒
                w = stw.Update(1);
                Assert.AreEqual(5, w.Count);
                Assert.AreEqual(4, w.Buckets.Count);
                Assert.AreEqual(6, w.Sum);
                Assert.AreEqual(1, w.Buckets.GetNthItemRefFromTail(2).Count);
                Assert.AreEqual(1, w.Buckets.GetNthItemRefFromTail(2).Sum);
                Assert.AreEqual(0, w.Buckets.GetNthItemRefFromTail(1).Count);
                Assert.AreEqual(0, w.Buckets.GetNthItemRefFromTail(1).Sum);
                Assert.AreEqual(1, w.Buckets.TailRef.Count);
                Assert.AreEqual(1, w.Buckets.TailRef.Sum);

                Thread.Sleep(5000); // 8.5秒
                w = stw.Update(1);
                Assert.AreEqual(1, w.Count);
                Assert.AreEqual(5, w.Buckets.Count);
                Assert.AreEqual(1, w.Sum);
                Assert.AreEqual(0, w.Buckets.GetNthItemRefFromTail(2).Count);
                Assert.AreEqual(0, w.Buckets.GetNthItemRefFromTail(2).Sum);
                Assert.AreEqual(0, w.Buckets.GetNthItemRefFromTail(1).Count);
                Assert.AreEqual(0, w.Buckets.GetNthItemRefFromTail(1).Sum);
                Assert.AreEqual(1, w.Buckets.TailRef.Count);
                Assert.AreEqual(1, w.Buckets.TailRef.Sum);

                stw.Dispose();
            }
        }
    }

