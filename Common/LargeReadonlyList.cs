using System.Collections;
using System.Collections.Immutable;
using System.Numerics;

namespace Common;

public interface ILargeReadonlyList<out T> : IEnumerable<T>
{
    BigInteger Count { get; }
    T this[BigInteger id] { get; }
}

public static class LargeReadonlyList
{
    public static ILargeReadonlyList<T> Empty<T>() => LargeReadonlyListEmpty<T>.Instance;
    public static ILargeReadonlyList<T> Singleton<T>(T syntax) => new LargeReadonlyListSingleton<T>(syntax);
    public static ILargeReadonlyList<T> Concat<T>(this ILargeReadonlyList<T> first, ILargeReadonlyList<T> second)
    {
        if (first.Count.IsZero)
            return second;
        else if (second.Count.IsZero)
            return first;
        else
            return new LargeReadonlyListConcat<T>(first, second);
    }
    public static ILargeReadonlyList<TResult> Select<T, TResult>(this ILargeReadonlyList<T> list, Func<T, TResult> map)
    {
        if (list.Count.IsZero)
            return Empty<TResult>();
        else if (list.Count.IsOne)
            return Singleton(map(list[0]));
        else
            return new LargeReadonlyListSelect<T, TResult>(list, map);
    }
    public static ILargeReadonlyList<TResult> Cross<TFirst, TSecond, TResult>(this ILargeReadonlyList<TFirst> first, ILargeReadonlyList<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
    {
        if (first.Count.IsZero || second.Count.IsZero)
            return Empty<TResult>();
        else if (first.Count == 1)
            return second.Select(s => resultSelector(first[0], s));
        else if (second.Count == 1)
            return first.Select(f => resultSelector(f, second[0]));
        else
            return new LargeReadonlyListCross<TFirst, TSecond, TResult>(first, second, resultSelector);
    }
    public static ILargeReadonlyList<TResult> CrossSum<TFirst, TSecond, TResult>(
        int sum,
        Func<int, ILargeReadonlyList<TFirst>> firsts,
        Func<int, ILargeReadonlyList<TSecond>> seconds,
        Func<TFirst, TSecond, TResult> result)
    {
        return new LargeReadonlyListCrossSum<TFirst, TSecond, TResult>(sum, firsts, seconds, result);
    }
    public static Func<int, ILargeReadonlyList<ImmutableArray<T>>> ArraySum<T>(Func<int, ILargeReadonlyList<T>> func)
    {
        var linkListFunc = FunctionalUtils.MemoizedRecursive(GetArrayLinkListRec(func));
        return sum => linkListFunc(sum).Select(stack => stack.ToImmutableArray());
    }
    private static Func<Func<int, ILargeReadonlyList<ImmutableStack<T>>>, int, ILargeReadonlyList<ImmutableStack<T>>> GetArrayLinkListRec<T>(Func<int, ILargeReadonlyList<T>> func) => (self, sum) =>
        sum == 0
        ? SingletonValues<T>.EmptyImmutableStack
        : CrossSum(sum, func, self, (value, stack) => stack.Push(value));

    private sealed class SingletonValues<T>
    {
        public static readonly ILargeReadonlyList<ImmutableStack<T>> EmptyImmutableStack = Singleton(ImmutableStack<T>.Empty);
    }
    private abstract class ALargeList<T> : ILargeReadonlyList<T>
    {
        public abstract T this[BigInteger id] { get; }
        public abstract BigInteger Count { get; }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = BigInteger.Zero; i < Count; ++i)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    private sealed class LargeReadonlyListEmpty<T> : ALargeList<T>
    {
        public static readonly LargeReadonlyListEmpty<T> Instance = new ();
        public override T this[BigInteger id] => throw new ArgumentOutOfRangeException(nameof(id));
        public override BigInteger Count => 0;
    }
    private sealed class LargeReadonlyListSingleton<T> : ALargeList<T>
    {
        private readonly T _value;

        public LargeReadonlyListSingleton(T value)
        {
            _value = value;
        }

        public override T this[BigInteger id] => _value;
        public override BigInteger Count => 1;
    }
    private sealed class LargeReadonlyListConcat<T> : ALargeList<T>
    {
        private readonly ILargeReadonlyList<T> _first;
        private readonly ILargeReadonlyList<T> _second;

        public LargeReadonlyListConcat(ILargeReadonlyList<T> left, ILargeReadonlyList<T> right)
        {
            _first = left ?? throw new ArgumentNullException(nameof(left));
            _second = right ?? throw new ArgumentNullException(nameof(right));
            Count = _first.Count + _second.Count;
        }

        public override T this[BigInteger id] => id < _first.Count ? _first[id] : _second[id - _first.Count];

        public override BigInteger Count { get; }
    }
    private sealed class LargeReadonlyListSelect<T, TResult> : ALargeList<TResult>
    {
        private readonly ILargeReadonlyList<T> _list;
        private readonly Func<T, TResult> _map;

        public LargeReadonlyListSelect(ILargeReadonlyList<T> list, Func<T, TResult> map)
        {
            _list = list ?? throw new ArgumentNullException(nameof(list));
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public override TResult this[BigInteger id] => _map(_list[id]);
        public override BigInteger Count => _list.Count;
    }
    private sealed class LargeReadonlyListCross<TFirst, TSecond, TResult> : ALargeList<TResult>
    {
        private readonly ILargeReadonlyList<TFirst> _first;
        private readonly ILargeReadonlyList<TSecond> _second;
        private readonly Func<TFirst, TSecond, TResult> _resultSelector;

        public LargeReadonlyListCross(ILargeReadonlyList<TFirst> first, ILargeReadonlyList<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            _first = first ?? throw new ArgumentNullException(nameof(first));
            _second = second ?? throw new ArgumentNullException(nameof(second));
            _resultSelector = resultSelector ?? throw new ArgumentNullException(nameof(resultSelector));

            Count = _first.Count * _second.Count;
        }

        public static TResult GetValue(ILargeReadonlyList<TFirst> first, ILargeReadonlyList<TSecond> second, Func<TFirst, TSecond, TResult> result, BigInteger id)
        {
            var quotient = BigInteger.DivRem(id, second.Count, out var remainder);
            return result(first[quotient], second[remainder]);
        }

        public override TResult this[BigInteger id] => GetValue(_first, _second, _resultSelector, id);
        public override BigInteger Count { get; }
    }
    private sealed class LargeReadonlyListCrossSum<TFirst, TSecond, TResult> : ALargeList<TResult>
    {
        private readonly int Sum;
        private readonly List<ILargeReadonlyList<TFirst>> Firsts;
        private readonly List<ILargeReadonlyList<TSecond>> Seconds;
        private readonly Func<TFirst, TSecond, TResult> Result;

        public LargeReadonlyListCrossSum(int sum, Func<int, ILargeReadonlyList<TFirst>> firsts, Func<int, ILargeReadonlyList<TSecond>> seconds, Func<TFirst, TSecond, TResult> result)
        {
            Sum = sum;
            Firsts = new();
            Seconds = new();
            Result = result ?? throw new ArgumentNullException(nameof(result));
            Count = BigInteger.Zero;
            for (int i = 0; i <= Sum; ++i)
            {
                // Only evaluate second if first if diffrent from zero, to allow structures that are infinite recursive in seconds
                Firsts.Add(firsts(i));
                var firstCount = Firsts[i].Count;
                if (firstCount == 0)
                {
                    // Any value will do, no one will read it.
                    Seconds.Add(LargeReadonlyListEmpty<TSecond>.Instance);
                }
                else
                {
                    Seconds.Add(seconds(Sum - i));
                    Count += firstCount * Seconds[i].Count;
                }
            }
        }

        public override TResult this[BigInteger id]
        {
            get
            {
                BigInteger c = BigInteger.Zero;
                for (int i = 0; i <= Sum; ++i)
                {
                    var pc = c;
                    // Only evaluate second if first if diffrent from zero, to allow structures that are infinite recursive in seconds
                    var firstCount = Firsts[i].Count;
                    c += firstCount == 0 ? 0 : firstCount * Seconds[i].Count;
                    if (id < c)
                        return LargeReadonlyListCross<TFirst, TSecond, TResult>.GetValue(Firsts[i], Seconds[i], Result, id - pc);
                }
                throw new ArgumentOutOfRangeException(nameof(id));
            }
        }

        public override BigInteger Count { get; }
    }
}
