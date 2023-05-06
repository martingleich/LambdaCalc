using LambdaCalc.Syntax;
using System.Collections.Immutable;

namespace LambdaCalc;

public sealed class AddRequiredParenthesisVisitor : IGreenExpressionSyntax.IVisitor<IGreenExpressionSyntax>, IGreenListContentHead.IVisitor<IGreenListContentHead>
{
    public static readonly AddRequiredParenthesisVisitor Instance = new();
    private static T? VisitNullable<T>(T? value, Func<T, T> map) => value is null ? value : map(value);
    private static ImmutableArray<T> VisitImmutableArray<T>(ImmutableArray<T> values, Func<T, T> map) => values.Select(map).ToImmutableArray();
    private static GreenParenthesisExpression AddParenthesis(IGreenExpressionSyntax expression) => new(
        new GreenTokenParenthesisOpen(null),
        expression,
        new GreenTokenParenthesisClose(null));
    public IGreenExpressionSyntax Visit(GreenVariableExpression greenVariableExpression) => greenVariableExpression;
    public IGreenExpressionSyntax Visit(GreenCallExpression greenCallExpression)
    {
        var newLeft = greenCallExpression.Left.Accept(this);
        if (newLeft is GreenLambdaExpression)
            newLeft = AddParenthesis(newLeft);
        var newRight = greenCallExpression.Right.Accept(this);
        if (newRight is GreenLambdaExpression or GreenCallExpression)
            newRight = AddParenthesis(newRight);
        return greenCallExpression.With(newLeft, newRight);
    }
    public IGreenExpressionSyntax Visit(GreenLambdaExpression greenLambdaExpression)
    {
        var newExpr = greenLambdaExpression.Expression.Accept(this);
        return greenLambdaExpression.With(greenLambdaExpression.ParameterName, greenLambdaExpression.DoubleRightArrow, newExpr);
    }
    public IGreenExpressionSyntax Visit(GreenParenthesisExpression greenParenthesisExpression)
    {
        var newExpr = greenParenthesisExpression.Expression.Accept(this);
        return greenParenthesisExpression.With(greenParenthesisExpression.ParenthesisOpen, newExpr, greenParenthesisExpression.ParenthesisClose);
    }
    public IGreenExpressionSyntax Visit(GreenListExpression greenListExpression)
    {
        var newContent = VisitNullable(greenListExpression.Content, Visit);
        return new GreenListExpression(greenListExpression.BracketOpen, newContent, greenListExpression.BracketClose);
    }
    public GreenListContentTail Visit(GreenListContentTail tail) =>
        new GreenListContentTail(tail.Comma, tail.Value.Accept(this));
    public IGreenListContentHead Visit(GreenListContentHeadAppend append) =>
        new GreenListContentHeadAppend(append.Value.Accept(this), append.Dots);
    public IGreenListContentHead Visit(GreenListContentHeadValue value) =>
        new GreenListContentHeadValue(value.Value.Accept(this));
    public GreenListContent Visit(GreenListContent content)
    {
        var newHead = content.Head.Accept(this);
        var newRest = VisitImmutableArray(content.Rest, Visit);
        return new GreenListContent(newHead, newRest);
    }
    public static T Perform<T>(T syntax) where T : IGreenExpressionSyntax => (T)syntax.Accept(Instance);
}