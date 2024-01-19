using System.Collections;
using System.Runtime.CompilerServices;

namespace NullGC.Linq.Enumerators;

public struct ICollectionLinqValueEnumerator<T> : ILinqValueEnumerator<T>
{
    private readonly ICollection<T> _list;
    private IEnumerator<T>? _enumerator;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ICollectionLinqValueEnumerator(ICollection<T> list)
    {
        _list = list;
    }

    public void Dispose()
    {
        _enumerator?.Dispose();
        _enumerator = null;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Initialize()
    {
        _enumerator = _list.GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    { 
        if (_enumerator is null)
            Initialize();
        return _enumerator!.MoveNext();
    }

    public void Reset()
    {
        CommunityToolkit.Diagnostics.ThrowHelper.ThrowNotSupportedException();
    }

    object? IEnumerator.Current => Current;

    public readonly int? Count => _list.Count;
    public readonly int? MaxCount => _list.Count;

    public readonly T Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _enumerator!.Current;
    }

    public bool SetSkipCount(int count) => false;
    public bool SetTakeCount(int count) => false;
}