using NullGC.TestCommons;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NullGC.Collections.Tests;

[TestClass]
public class ValueQueueTests : AssertMemoryAllFreedBase
{
    public static TestContext SharedTestContext { get; set; }
    public TestContext TestContext { get; set; }

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        SharedTestContext = context;
    }

    public ValueQueueTests() : base(new MSTestOutputHelper(SharedTestContext), false)
    {
    }

    [TestMethod]
    public void EmptyQueueFacts()
    {
        var queue = new ValueQueue<int>();
        Assert.IsFalse(queue.IsAllocated);
        Assert.AreEqual(0, queue.Count);
        Assert.AreEqual(0, queue.Count); // 用 Count 判断是否为空
        Assert.ThrowsException<InvalidOperationException>(() => queue.Peek());
        Assert.ThrowsException<InvalidOperationException>(() => queue.Dequeue());
        queue.Dispose();
        Assert.IsTrue(AllocTracker.ClientIsAllFreed);
        queue.Dispose();
    }

    [TestMethod]
    public void EnqueueDequeueFacts()
    {
        var queue = new ValueQueue<int>();
        queue.Enqueue(1);
        Assert.AreEqual(1, queue.Dequeue());
        Assert.AreEqual(0, queue.Count);
        Assert.AreEqual(0, queue.Count); // 判空
        Assert.ThrowsException<InvalidOperationException>(() => queue.Dequeue());
        queue.Enqueue(1);
        Assert.AreEqual(1, queue.Count); // Assert.Single 改为判断 Count==1
        queue.Enqueue(2);
        queue.Enqueue(3);
        queue.Enqueue(4);
        queue.Enqueue(5);
        queue.Enqueue(6);
        Assert.AreEqual(6, queue.Count);
        Assert.AreEqual(6, queue.Count()); // 若 Count() 为 IEnumerable 扩展方法调用
        Assert.AreEqual(1, queue.Dequeue());
        queue.Enqueue(7);
        Assert.AreEqual(2, queue.Dequeue());
        Assert.AreEqual(3, queue.Dequeue());
        Assert.AreEqual(4, queue.Dequeue());
        Assert.AreEqual(5, queue.Dequeue());
        Assert.AreEqual(6, queue.Dequeue());
        Assert.AreEqual(7, queue.Dequeue());

        queue.Dispose();
        Assert.IsTrue(AllocTracker.ClientIsAllFreed);
        queue.Dispose();
    }
}
