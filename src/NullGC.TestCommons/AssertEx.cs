using CommunityToolkit.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NullGC.TestCommons;

public static class AssertEx
{
    private static Exception Throws(Type exceptionType, Exception? exception)
    {
        Guard.IsNotNull(exceptionType);

        if (exception == null)
            Assert.Fail($"Expected exception of type '{exceptionType.FullName}' but no exception was thrown.");

        if (exceptionType != exception.GetType())
            Assert.Fail($"Expected exception of type '{exceptionType.FullName}', but exception of type '{exception.GetType().FullName}' was thrown.");

        return exception;
    }

    private static Exception? RecordException<TArg>(ActionT1Ref<TArg> testCode, ref TArg arg)
    {
        Guard.IsNotNull(testCode);

        try
        {
            testCode(ref arg);
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public static T Throws<T, TArg>(ActionT1Ref<TArg> testCode, ref TArg arg)
        where T : Exception
    {
        return (T)Throws(typeof(T), RecordException(testCode, ref arg));
    }
}
