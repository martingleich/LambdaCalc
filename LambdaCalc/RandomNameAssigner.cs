using Common;
using Distributions;
using LambdaCalc.Syntax;
using System.Collections.Immutable;

namespace LambdaCalc;

public static class RandomNameAssigner
{
    public static IDistribution<IGreenExpressionSyntax> AssignRandomNames(
        this IDistribution<GreenLambdaExpression> structure,
        IDistribution<string> newNames)
    {
        var visitor = new Visitor(newNames, ImmutableHashSet<string>.Empty);
        return structure.SelectMany(visitor.Visit);
    }
    private sealed class Visitor : IGreenExpressionSyntax.IVisitor<IDistribution<IGreenExpressionSyntax>>
    {
        private readonly IDistribution<string> _newNames;
        private readonly ImmutableHashSet<string> _existingNames;
        private readonly IDistribution<string>? _existingNamesDistribution;

        public Visitor(IDistribution<string> newNames, ImmutableHashSet<string> existingNames)
        {
            _newNames = newNames ?? throw new ArgumentNullException(nameof(newNames));
            _existingNames = existingNames ?? throw new ArgumentNullException(nameof(existingNames));
            _existingNamesDistribution = _existingNames.Count > 0 ? _existingNames.ToUniformDistribution() : null;
        }

        public IDistribution<IGreenExpressionSyntax> Visit(GreenVariableExpression greenVariableExpression) =>
            from n in _existingNamesDistribution
            select (IGreenExpressionSyntax)greenVariableExpression.WithIdentifierValue(n);

        public IDistribution<IGreenExpressionSyntax> Visit(GreenCallExpression greenCallExpression) =>
            from left in greenCallExpression.Left.Accept(this)
            from right in greenCallExpression.Right.Accept(this)
            select (IGreenExpressionSyntax)greenCallExpression.With(left, right);

        public IDistribution<IGreenExpressionSyntax> Visit(GreenLambdaExpression greenLambdaExpression) =>
            from newName in _newNames
            from expression in greenLambdaExpression.Expression.Accept(new Visitor(_newNames, _existingNames.Add(newName)))
            select (IGreenExpressionSyntax)greenLambdaExpression.With(greenLambdaExpression.ParameterName.WithValue(newName), greenLambdaExpression.DoubleRightArrow, expression);

        public IDistribution<IGreenExpressionSyntax> Visit(GreenParenthesisExpression greenParenthesisExpression) =>
            from value in greenParenthesisExpression.Expression.Accept(this)
            select (IGreenExpressionSyntax)greenParenthesisExpression.WithExpression(value);
    }
}
