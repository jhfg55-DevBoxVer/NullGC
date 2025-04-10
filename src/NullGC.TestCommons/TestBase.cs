using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NullGC.TestCommons;
// 在 TestCommons 命名空间中定义日志辅助接口及适配器，实现 MSTest 模式下的输出
public interface ITestOutputHelper
{
    void WriteLine(string message);
}

public class MSTestOutputHelper : ITestOutputHelper
{
    private readonly TestContext _testContext;

    public MSTestOutputHelper(TestContext testContext)
    {
        _testContext = testContext;
    }

    public void WriteLine(string message)
    {
        _testContext.WriteLine(message);
    }
}
public abstract class TestBase : IDisposable
{
    private readonly ITestOutputHelper _logger;

    protected TestBase(ITestOutputHelper logger)
    {
        _logger = logger;
        _logger.WriteLine("Starting test");
    }

    public virtual void Dispose()
    {
        _logger.WriteLine("Finished test");
    }
}
