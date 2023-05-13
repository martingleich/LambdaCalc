using Common;
using Distributions;
using LambdaCalc.Syntax;

namespace LambdaCalc;

public static class ProgramListExt
{
    public static IDistribution<IGreenExpressionSyntax> GetStructureExpressionSyntax(this ProgramList self, int numTokens) =>
        self.GetExpressionSyntax(numTokens).ToUniformDistribution();
    public static IDistribution<GreenLambdaExpression> GetStructureLambdaExpressionSyntax(this ProgramList self, int numTokens) =>
        self.GetLambdaExpression(numTokens).ToUniformDistribution();
}

public sealed class ProgramList
{
    private Func<int, ILargeReadonlyList<T>> Memoize<T>(Func<ProgramList, int, ILargeReadonlyList<T>> func) => FunctionalUtils.Memoize<int, ILargeReadonlyList<T>>(arg => func(this, arg));

    private static ILargeReadonlyList<T?> GetNull<T>(int numNodes)
    {
        if (numNodes == 0)
            return LargeReadonlyList.Singleton<T?>(default);
        else
            return LargeReadonlyList.Empty<T?>();
    }

    public ProgramList()
    {
        GetCallExpression = Memoize((self, numNodes) =>
        {
            if (numNodes >= 1)
                return LargeReadonlyList.CrossSum(numNodes - 1, self.GetExpressionSyntax, self.GetExpressionSyntax, (left, right) => new GreenCallExpression(left, right));
            else
                return LargeReadonlyList.Empty<GreenCallExpression>();
        });
        GetDefinitionSyntax = Memoize((self, numNodes) =>
            self.GetExpressionSyntax(numNodes - 3)
            .Select(expr => new GreenDefinitionSyntax(GreenTokenDef.Instance, GreenIdentifierX, GreenTokenAssign.Instance, expr))
        );
        GetExpressionSyntax = Memoize((self, numNodes) =>
        {
            var result = LargeReadonlyList.Empty<IGreenExpressionSyntax>();
            result = result.Concat(self.GetVariableExpression(numNodes));
            result = result.Concat(self.GetLambdaExpression(numNodes));
            result = result.Concat(self.GetCallExpression(numNodes));
            result = result.Concat(self.GetListExpression(numNodes));
            return result;
        });

        GetFileSyntax = Memoize((self, numNodes) =>
            from defList in LargeReadonlyList.ArraySum(self.GetDefinitionSyntax)(numNodes)
            select new GreenFileSyntax(defList, GreenTokenEndOfFile.Instance));

        GetLambdaExpression = Memoize((self, numNodes) =>
        {
            if (numNodes >= 2)
                return self.GetExpressionSyntax(numNodes - 2).Select(expr => new GreenLambdaExpression(GreenIdentifierX, GreenTokenDoubleRightArrow.Instance, expr));
            else
                return LargeReadonlyList.Empty<GreenLambdaExpression>();
        });

        GetVariableExpression = Memoize((self, numNodes) =>
        {
            if (numNodes == 1)
                return AllVariableExpressions;
            else
                return LargeReadonlyList.Empty<GreenVariableExpression>();
        });
        GetListContentHeadValue = Memoize((self, numNodes) =>
        {
            return self.GetExpressionSyntax(numNodes).Select(v => new GreenListContentHeadValue(v));
        });
        GetListContentHeadAppend = Memoize((self, numNodes) =>
        {
            if (numNodes >= 1)
                return self.GetExpressionSyntax(numNodes - 1).Select(v => new GreenListContentHeadAppend(v, GreenTokenDots.Instance));
            else
                return LargeReadonlyList.Empty<GreenListContentHeadAppend>();
        });
        GetListContentHead = Memoize((self, numNodes) =>
        {
            var result = LargeReadonlyList.Empty<IGreenListContentHead>();
            result = result.Concat(self.GetListContentHeadValue(numNodes));
            result = result.Concat(self.GetListContentHeadAppend(numNodes));
            return result;
        });
        GetListContentTail = Memoize((self, numNodes) =>
        {
            if (numNodes >= 1)
                return self.GetExpressionSyntax(numNodes - 1).Select(expr => new GreenListContentTail(GreenTokenComma.Instance, expr));
            else
                return LargeReadonlyList.Empty<GreenListContentTail>();
        });
        GetListContent = Memoize((self, numNodes) =>
        {
            return LargeReadonlyList.CrossSum(numNodes, self.GetListContentHead, LargeReadonlyList.ArraySum(self.GetListContentTail), (h, r) => new GreenListContent(h, r));
        });
        GetListExpression = Memoize((self, numNodes) =>
        {
            if (numNodes >= 2)
                return WithNullable(GetListContent)(numNodes - 2).Select(content => new GreenListExpression(GreenTokenBracketOpen.Instance, content, GreenTokenBracketClose.Instance));
            else
                return LargeReadonlyList.Empty<GreenListExpression>();
        });
    }
    private static readonly GreenTokenIdentifier GreenIdentifierX = new("x", null);
    private static readonly GreenVariableExpression GreenVariableX = new(GreenIdentifierX);
    private static readonly ILargeReadonlyList<GreenVariableExpression> AllVariableExpressions = LargeReadonlyList.Singleton(GreenVariableX);
    
    public readonly Func<int, ILargeReadonlyList<GreenCallExpression>> GetCallExpression;
    public readonly Func<int, ILargeReadonlyList<GreenDefinitionSyntax>> GetDefinitionSyntax;
    public readonly Func<int, ILargeReadonlyList<IGreenExpressionSyntax>> GetExpressionSyntax;
    public readonly Func<int, ILargeReadonlyList<GreenFileSyntax>> GetFileSyntax;
    public readonly Func<int, ILargeReadonlyList<GreenLambdaExpression>> GetLambdaExpression;
    public readonly Func<int, ILargeReadonlyList<GreenVariableExpression>> GetVariableExpression;
    public readonly Func<int, ILargeReadonlyList<GreenListContentHeadValue>> GetListContentHeadValue;
    public readonly Func<int, ILargeReadonlyList<GreenListContentHeadAppend>> GetListContentHeadAppend;
    public readonly Func<int, ILargeReadonlyList<IGreenListContentHead>> GetListContentHead;
    public readonly Func<int, ILargeReadonlyList<GreenListContentTail>> GetListContentTail;
    public readonly Func<int, ILargeReadonlyList<GreenListContent>> GetListContent;
    public readonly Func<int, ILargeReadonlyList<GreenListExpression>> GetListExpression;

    private static Func<int, ILargeReadonlyList<T?>> WithNullable<T>(Func<int, ILargeReadonlyList<T>> func) where T:class => numNodes =>
    {
        var result = LargeReadonlyList.Empty<T?>();
        result = result.Concat(GetNull<T>(numNodes));
        result = result.Concat(func(numNodes));
        return result;
    };
}
