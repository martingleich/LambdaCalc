namespace Common;

public static class FunctionalUtils
{
    public static Func<TArg, TResult> Memoize<TArg, TResult>(Func<TArg, TResult> func) where TArg : notnull, IComparable<TArg>
    {
        var dict = new Dictionary<TArg, TResult>();
        return arg =>
        {
            if (!dict.TryGetValue(arg, out var result))
                dict[arg] = result = func(arg);
            return result;
        };
    }
    public static Func<TArg, TResult> MemoizedRecursive<TArg, TResult>(Func<Func<TArg, TResult>, TArg, TResult> func) where TArg : notnull, IComparable<TArg>
    {
        Func<TArg, TResult> result = default!;
        result = Memoize<TArg, TResult>(arg => func(result, arg));
        return result;
    }
}