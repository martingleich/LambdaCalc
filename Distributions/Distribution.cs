using Common;
using System.Collections.Immutable;
using System.Numerics;
using System.Security.Cryptography;

namespace Distributions
{
    public interface IDistribution<out T>
    {
        T Sample(RandomNumberGenerator gen);
    }

    public static class Distribution
    {
        public static IDistribution<T> Singleton<T>(T value) =>
            new SingletonDistribution<T>(value);
        public static IDistribution<int> UnsignedInteger(int upperBound) => UnsignedBigInteger(upperBound).Select(b => (int)b);
        public static IDistribution<BigInteger> UnsignedBigInteger(BigInteger upperBound)
        {
            if (upperBound <= 0)
                throw new ArgumentException($"{nameof(upperBound)}({upperBound}) must be bigger than zero.");
            return new UnsignedBigIntegerDistribution(upperBound);
        }
        public static IDistribution<TResult> Select<T, TResult>(this IDistribution<T> self, Func<T, TResult> map) =>
            new SelectDistribution<T, TResult>(self, map);
        public static IDistribution<TResult> SelectWhere<T, TResult>(this IDistribution<T> self, Func<T, TResult?> map) =>
            from x in self
            let y = map(x)
            where y is not null
            select y;
        public static IDistribution<T> Where<T>(this IDistribution<T> self, Func<T, bool> filter) => new WhereDistribution<T>(self, filter);
        public static IDistribution<T> NotNull<T>(this IDistribution<T?> self) =>
            from x in self
            where x != null
            select x;
        public static IDistribution<TResult> SelectMany<T, TCollection, TResult>(this IDistribution<T> self, Func<T, IDistribution<TCollection>> map, Func<T, TCollection, TResult> result) =>
            new SelectManyDistribution<T, TCollection, TResult>(self, map, result);
        public static IDistribution<TResult> SelectMany<T, TResult>(this IDistribution<T> self, Func<T, IDistribution<TResult>> map) =>
            new SelectManyDistribution<T, TResult, TResult>(self, map, (_, e) => e);
        public static IDistribution<T> ToUniformDistribution<T>(this ILargeReadonlyList<T> values) =>
            from x in UnsignedBigInteger(values.Count) select values[x];
        public static IDistribution<T> ToUniformDistribution<T>(this ImmutableArray<T> values) =>
            from x in UnsignedBigInteger(values.Length) select values[(int)x];
        public static IDistribution<T> ToUniformDistribution<T>(this IEnumerable<T> values) =>
            values.ToImmutableArray().ToUniformDistribution();
        public static readonly IDistribution<string> UniformLowerCaseLetterString = UnsignedInteger(26).Select(i => ((char)('a' + i)).ToString());
        public static IDistribution<ImmutableArray<T>> FlattenToImmutable<T>(this IEnumerable<IDistribution<T>> values) => new ImmutableFlattenDistribution<T>(values);

        private class ImmutableFlattenDistribution<T> : IDistribution<ImmutableArray<T>>
        {
            private readonly IEnumerable<IDistribution<T>> _values;

            public ImmutableFlattenDistribution(IEnumerable<IDistribution<T>> values)
            {
                _values = values;
            }

            public ImmutableArray<T> Sample(RandomNumberGenerator gen)
            {
                var builder = ImmutableArray.CreateBuilder<T>();
                foreach (var value in _values)
                    builder.Add(value.Sample(gen));
                return builder.ToImmutable();
            }
        }

        private sealed record class SingletonDistribution<T>(T Value) : IDistribution<T>
        {
            public T Sample(RandomNumberGenerator gen) => Value;
        }
        private sealed record class UnsignedBigIntegerDistribution(BigInteger UpperBound) : IDistribution<BigInteger>
        {
            public BigInteger Sample(RandomNumberGenerator gen)
            {
                // We need at least one bit more the UpperBound because we have to force it to zero.
                const int MaxStackLimit = 128;
                var numBytes = (int)(UpperBound.GetBitLength() + 7) / 8;
                var bytes = numBytes < MaxStackLimit ? stackalloc byte[numBytes] : new byte[numBytes];
                // Mask all bits outside of the bitlength to zero
                var bitMask = (byte)((1 << (int)UpperBound.GetBitLength() % 8) - 1);
                while (true)
                {
                    gen.GetBytes(bytes);
                    bytes[^1] &= bitMask;
                    var result = new BigInteger(bytes, true, false);
                    if (result < UpperBound)
                        return result;
                }
            }
        }
        private sealed record class SelectManyDistribution<T, TDistribution, TResult>(IDistribution<T> Distribution, Func<T, IDistribution<TDistribution>> Map, Func<T, TDistribution, TResult> Result) : IDistribution<TResult>
        {
            public TResult Sample(RandomNumberGenerator gen)
            {
                var x = Distribution.Sample(gen);
                var d = Map(x);
                return Result(x, d.Sample(gen));
            }
        }
        private sealed record class SelectDistribution<T, TResult>(IDistribution<T> Distribution, Func<T, TResult> Map) : IDistribution<TResult>
        {
            public TResult Sample(RandomNumberGenerator gen) => Map(Distribution.Sample(gen));
        }
        private sealed record class WhereDistribution<T>(IDistribution<T> Distribution, Func<T, bool> Filter) : IDistribution<T>
        {
            public T Sample(RandomNumberGenerator gen)
            {
                while (true)
                {
                    var x = Distribution.Sample(gen);
                    if (Filter(x))
                        return x;
                }
            }
        }
    }
}