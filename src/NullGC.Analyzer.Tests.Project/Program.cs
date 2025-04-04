using System.Runtime.CompilerServices;
using NullGC.Allocators;
using NullGC.Allocators.Extensions;
using NullGC.Collections;
using NullGC.Linq;

namespace NullGC.Analyzer.Tests.Project;

public static class Program
{
    private static UList<int> _list;
    private static int _key;
    private static UList<int> _list2;

    public static void Main(string[] args)
    {
        AllocatorContext.SetImplementation(new DefaultAllocatorContextImpl());
        AllocatorContext.Impl.ConfigureDefault();
        _list = new UList<int>(AllocatorTypes.DefaultUnscoped) {1, 2, 3, 4, 5, 6};
        _list2 = new UList<int>(AllocatorTypes.DefaultUnscoped) {1};
        UseValueList(_list); // should borrow

        var stA = new StructA(_list);
        Console.WriteLine(/*ReadOnly*/stA.NoBorrowList); // ok
        Console.WriteLine(stA.NoBorrowList); // should borrow
        Console.WriteLine(stA.BorrowList); // ok
        Console.WriteLine(stA.Borrow()); // ok
        Console.WriteLine(stA.BorrowNotInterface()); // ok
        Console.WriteLine(stA.PartiallyExplicit); // partially explicit
        Console.WriteLine(stA.BorrowNoAttribute()); // should have attribute

        // ref UList<int> localList = ref _list2;
        UList<int> localList = default;
        stA.RefParamMethod(ref localList);
        if (localList.SequenceEqual(new[] {1})) throw new InvalidOperationException();
        if (!localList.SequenceEqual(new[] {1, 2, 3, 4, 5, 6})) throw new InvalidOperationException();
        if (Unsafe.IsNullRef(ref localList)) throw new InvalidOperationException();
        localList.Dispose();
        stA.Dispose();
        _list.Dispose();
    }

    private static void UseValueList(UList<int> lst)
    {
        _key = lst.LinqRef().GroupBy(x => x).First().Key; // GroupBy not implemented
        lst.LinqRef().WorkOnIEnumerable(); // GroupBy not implemented
        
    }
}

internal static class Extensions
{
    public static void WorkOnIEnumerable<T>(this IEnumerable<T> obj)
    {
        
    }
}

struct StructA : IExplicitOwnership<StructA>
{
    private bool flag;
    private UList<int> _list;
    public UList<int> NoBorrowList => _list;

    public UList<int> BorrowList
    {
        get { return _list.Borrow(); }
    }

    public UList<int> PartiallyExplicit
    {
        get
        {
            if (flag)
                return _list.Borrow();
            else
                return _list;
        }
    }

    public StructA(UList<int> list)
    {
        _list = list;
    }

    // [UnscopedRef]
    // public void RefAssignRefParamMethod(ref UList<int> refParam)
    // {
    //     if (Unsafe.IsNullRef(ref _list)) throw new InvalidOperationException();
    //     if (!_list.SequenceEqual(new []{1,2,3,4,5,6})) throw new InvalidOperationException();
    //     refParam = ref _list;
    // }

    public void RefParamMethod(ref UList<int> refParam)
    {
        refParam = _list;
    }

    public void Dispose()
    {
        _list.Dispose();
    }

    public StructA Borrow()
    {
        return new StructA(_list.Borrow());
    }

    [return: Borrowed]
    public StructA BorrowNotInterface()
    {
        return new StructA(_list.Borrow());
    }

    public StructA BorrowNoAttribute()
    {
        return new StructA(_list.Borrow());
    }

    [return: Owned]
    public StructA Take()
    {
        return new StructA(_list.Take());
    }
}