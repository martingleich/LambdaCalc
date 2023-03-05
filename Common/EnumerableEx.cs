namespace Common;

public static class EnumerableEx
{
    public static T AggregatePairwise<T>(this IEnumerable<T> values, Func<T, T, T> func, T zero)
    {
        var arr = values.ToArray();
        if (arr.Length == 0)
            return zero;
        var len = arr.Length;
        while (len > 1)
        {
            var newlen = (len + 1) / 2;
            for (int i = 0; i < newlen; ++i)
            {
                if (2 * i + 1 < len)
                    arr[i] = func(arr[2 * i], arr[2 * i + 1]);
                else
                    arr[i] = arr[2 * i];
            }
            len = newlen;
        }
        return arr[0];
    }
}