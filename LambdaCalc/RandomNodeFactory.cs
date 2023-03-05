using Common;
using Distributions;
using LambdaCalc.Syntax;
using System.Collections;

namespace LambdaCalc;

public interface IProgramLists
{
    ILargeReadonlyList<GreenLambdaExpression> GetLambdaExpressions(int numNodes);
    ILargeReadonlyList<GreenCallExpression> GetCallExpressions(int numNodes);
    ILargeReadonlyList<IGreenExpressionSyntax> GetExpressions(int numNodes);
    ILargeReadonlyList<GreenDefinitionSyntax> GetDefinitions(int numNodes);
    ILargeReadonlyList<GreenFileSyntax> GetFiles(int numNodes);
}

public static class ProgramListExt
{
    public static IDistribution<IGreenExpressionSyntax> GetStructureExpressionSyntax(this IProgramLists self, int numTokens) =>
        self.GetExpressions(numTokens).ToUniformDistribution();
    public static IDistribution<GreenLambdaExpression> GetStructureLambdaExpressionSyntax(this IProgramLists self, int numTokens) =>
        self.GetLambdaExpressions(numTokens).ToUniformDistribution();
}

public sealed class ProgramList : IProgramLists
{
    private sealed class Cache : IProgramLists
    {
        private readonly IProgramLists Computer;
        private readonly Dictionary<int, ILargeReadonlyList<GreenCallExpression>> _getCallExpressions = new ();
        public ILargeReadonlyList<GreenCallExpression> GetCallExpressions(int numNodes)
        {
            if (!_getCallExpressions.TryGetValue(numNodes, out var value))
                _getCallExpressions[numNodes] = value = Computer.GetCallExpressions(numNodes);
            return value;
        }

        private readonly Dictionary<int, ILargeReadonlyList<GreenDefinitionSyntax>> _getDefinitions = new ();
        public ILargeReadonlyList<GreenDefinitionSyntax> GetDefinitions(int numNodes)
        {
            if (!_getDefinitions.TryGetValue(numNodes, out var value))
                _getDefinitions[numNodes] = value = Computer.GetDefinitions(numNodes);
            return value;
        }

        private readonly Dictionary<int, ILargeReadonlyList<IGreenExpressionSyntax>> _getExpressions = new ();
        public ILargeReadonlyList<IGreenExpressionSyntax> GetExpressions(int numNodes)
        {
            if (!_getExpressions.TryGetValue(numNodes, out var value))
                _getExpressions[numNodes] = value = Computer.GetExpressions(numNodes);
            return value;
        }

        private readonly Dictionary<int, ILargeReadonlyList<GreenFileSyntax>> _getFiles = new ();
        public ILargeReadonlyList<GreenFileSyntax> GetFiles(int numNodes)
        {
            if (!_getFiles.TryGetValue(numNodes, out var value))
                _getFiles[numNodes] = value = Computer.GetFiles(numNodes);
            return value;
        }

        private readonly Dictionary<int, ILargeReadonlyList<GreenLambdaExpression>> _getLambdaExpressions = new ();

        public Cache(IProgramLists computer)
        {
            Computer = computer ?? throw new ArgumentNullException(nameof(computer));
        }

        public ILargeReadonlyList<GreenLambdaExpression> GetLambdaExpressions(int numNodes)
        {
            if (!_getLambdaExpressions.TryGetValue(numNodes, out var value))
                _getLambdaExpressions[numNodes] = value = Computer.GetLambdaExpressions(numNodes);
            return value;
        }
        public IReadOnlyList<ILargeReadonlyList<IGreenExpressionSyntax>> GetAllExpressionsLists(int maxNumNodes) =>
            new AllExpressionsLists(this, maxNumNodes);
        private sealed class AllExpressionsLists : IReadOnlyList<ILargeReadonlyList<IGreenExpressionSyntax>>
        {
            private readonly Cache Cache;

            public AllExpressionsLists(Cache cache, int count)
            {
                Cache = cache ?? throw new ArgumentNullException(nameof(cache));
                Count = count;
            }

            public int Count { get; }
            public ILargeReadonlyList<IGreenExpressionSyntax> this[int index] => Cache.GetExpressions(index);
            public IEnumerator<ILargeReadonlyList<IGreenExpressionSyntax>> GetEnumerator()
            {
                for (int i = 0; i < Count; ++i)
                    yield return this[i];
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }

    private readonly Cache _cache;
    private ProgramList()
    {
        _cache = new Cache(this);
    }
    public static IProgramLists CreateInstance() => new ProgramList()._cache;
    private static readonly GreenTokenIdentifier GreenIdentifierX = new("x", null);
    private static readonly GreenVariableExpression GreenVariableX = new(GreenIdentifierX);
    private static readonly GreenTokenDoubleRightArrow GreenDoubleRightArrow = new(null);
    private static readonly GreenTokenDef GreenTokenDef = new(null);
    private static readonly GreenTokenAssign GreenTokenAssign = new(null);
    private static readonly ILargeReadonlyList<IGreenExpressionSyntax> AllVariableExpressions = LargeReadonlyList.Singleton<IGreenExpressionSyntax>(GreenVariableX);
    public ILargeReadonlyList<GreenCallExpression> GetCallExpressions(int numNodes)
    {
        var allExpressions = _cache.GetAllExpressionsLists(numNodes - 1);
        return LargeReadonlyList.CrossSum(numNodes - 1, allExpressions, allExpressions, (left, right) => new GreenCallExpression(left, right));
    }

    public ILargeReadonlyList<GreenDefinitionSyntax> GetDefinitions(int numNodes) =>
        _cache.GetExpressions(numNodes - 1)
        .Select(expr => new GreenDefinitionSyntax(GreenTokenDef, GreenIdentifierX, GreenTokenAssign, expr));

    public ILargeReadonlyList<IGreenExpressionSyntax> GetExpressions(int numNodes)
    {
        if (numNodes == 0)
            return LargeReadonlyList.Empty<IGreenExpressionSyntax>();
        else if (numNodes == 1)
            return AllVariableExpressions;
        else if (numNodes == 2)
            return _cache.GetLambdaExpressions(numNodes);
        else
            return _cache.GetCallExpressions(numNodes).Concat<IGreenExpressionSyntax>(_cache.GetLambdaExpressions(numNodes));
    }

    public ILargeReadonlyList<GreenFileSyntax> GetFiles(int numNodes)
    {
        throw new NotImplementedException();
    }

    public ILargeReadonlyList<GreenLambdaExpression> GetLambdaExpressions(int numNodes) =>
        _cache.GetExpressions(numNodes - 1)
        .Select(expr => new GreenLambdaExpression(GreenIdentifierX, GreenDoubleRightArrow, expr));
}
