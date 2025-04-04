using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NullGC.TestCommons;

namespace NullGC.Collections.Tests
{
    [TestClass]
    public class ValueListTests : AssertMemoryAllFreedBase
    {
        public ValueListTests()
            : base(new MSTestOutputHelper(), false, true)
        {
        }

        [TestMethod]
        public void DefaultListIsEmptyList()
        {
            ValueList<int> defaultList = default;
            Assert.IsFalse(defaultList.IsAllocated);
            Assert.IsTrue(EqualityComparer<ValueList<int>>.Default.Equals(ValueList<int>.Empty, defaultList));
            Assert.AreEqual(0, defaultList.Count);
            foreach (ref var _ in defaultList)
            {
                Assert.Fail("Should be empty.");
            }
            defaultList.Dispose();
        }

        [TestMethod]
        public void SpecificCapacityListCanBeConstructed()
        {
            var arr = new ValueList<int>(0);
            arr.Dispose();
            arr = new ValueList<int>(1);
            arr.Dispose();
            arr = new ValueList<int>(2);
            arr.Dispose();
            arr = new ValueList<int>(200);
            arr.Dispose();
            arr = new ValueList<int>(20000);
            arr.Dispose();
            arr = new ValueList<int>(500000);
            arr.Dispose();
            arr = new ValueList<int>(50000000);
            arr.Dispose();
        }

        [TestMethod]
        public void ValueSetShouldBePreservedOnGrowingList()
        {
            var count = 10_000_000;
            var arr = new ValueList<int>();
            for (var i = 0; i < count; i++)
            {
                arr.Add(i);
            }

            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(i, arr[i]);
            }

            arr.Dispose();
        }

        [TestMethod]
        public void IndexOfAndContainsFacts()
        {
            var list = new ValueList<int>(100);
            for (var i = 0; i < list.Capacity; i++)
            {
                list.Add(i);
            }

            for (var i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(i, list.IndexOf(i));
            }

            Assert.AreNotEqual(-1, list.IndexOf(1, 1, 10000));

            list.RemoveAt(list.Count - 1);
            Assert.IsTrue(list.Any(x => x == list.Count - 1));
            Assert.IsFalse(list.Any(x => x == list.Capacity - 1));

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.IndexOf(99999, list.Count));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.IndexOf(99999, list.Count, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.LastIndexOf(99999, list.Capacity - 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => list.LastIndexOf(99999, list.Capacity - 1, 10000));

            for (var i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(i, list.LastIndexOf(i));
            }

            var rand = new Random();
            for (var i = 0; i < list.Count; i++)
            {
                list[i] = rand.Next(0, 50);
            }

            list[47] = 11111;
            Assert.AreEqual(47, list.IndexOf(11111));
            Assert.AreEqual(47, list.LastIndexOf(11111));
            Assert.IsTrue(list.Contains(11111));
            list.Dispose();
        }

        // 一个简单的 MSTest 输出工具实现，用于适配 AssertMemoryAllFreedBase 基类。
        private class MSTestOutputHelper : Xunit.Abstractions.ITestOutputHelper
        {
            public void WriteLine(string message)
            {
                Console.WriteLine(message);
            }

            public void WriteLine(string format, params object[] args)
            {
                Console.WriteLine(format, args);
            }
        }
    }
}
