﻿using Common;
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
    private sealed class Visitor : IGreenExpressionSyntax.IVisitor<IDistribution<IGreenExpressionSyntax>>, IGreenListContentHead.IVisitor<IDistribution<IGreenListContentHead>>
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

        private static IDistribution<T?> VisitNullable<T>(T? value, Func<T, IDistribution<T>> map) => value != null ? map(value) : Distribution.Singleton<T?>(default);
        private static IDistribution<ImmutableArray<T>> VisitImmutableArray<T>(ImmutableArray<T> values, Func<T, IDistribution<T>> map) => values.Select(map).FlattenToImmutable();

        public IDistribution<IGreenExpressionSyntax> Visit(GreenVariableExpression greenVariableExpression) =>
            from n in _existingNamesDistribution
            select (IGreenExpressionSyntax)greenVariableExpression.WithIdentifierValue(n);
        public IDistribution<IGreenExpressionSyntax> Visit(GreenLambdaExpression greenLambdaExpression) =>
            from newName in _newNames
            from expression in greenLambdaExpression.Expression.Accept(new Visitor(_newNames, _existingNames.Add(newName)))
            select (IGreenExpressionSyntax)greenLambdaExpression.With(greenLambdaExpression.ParameterName.WithValue(newName), greenLambdaExpression.DoubleRightArrow, expression);

        public IDistribution<IGreenExpressionSyntax> Visit(GreenCallExpression greenCallExpression) =>
            from left in greenCallExpression.Left.Accept(this)
            from right in greenCallExpression.Right.Accept(this)
            select (IGreenExpressionSyntax)greenCallExpression.With(left, right);
        public IDistribution<IGreenExpressionSyntax> Visit(GreenParenthesisExpression greenParenthesisExpression) =>
            from value in greenParenthesisExpression.Expression.Accept(this)
            select (IGreenExpressionSyntax)greenParenthesisExpression.WithExpression(value);
        public IDistribution<GreenListContent> Visit(GreenListContent content) =>
            from head in content.Head.Accept(this)
            from rest in VisitImmutableArray(content.Rest, Visit)
            select new GreenListContent(head, rest);
        private IDistribution<GreenListContentTail> Visit(GreenListContentTail tail) =>
            from value in tail.Value.Accept(this)
            select tail.WithValue(value);
        public IDistribution<IGreenListContentHead> Visit(GreenListContentHeadAppend head) =>
            from value in head.Value.Accept(this)
            select head.WithValue(value);
        public IDistribution<IGreenListContentHead> Visit(GreenListContentHeadValue head) =>
            from value in head.Value.Accept(this)
            select head.WithValue(value);
        public IDistribution<IGreenExpressionSyntax> Visit(GreenListExpression greenListExpression) =>
            from newContent in VisitNullable(greenListExpression.Content, Visit)
            select greenListExpression.WithContent(newContent);
    }
}
