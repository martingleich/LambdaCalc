using LambdaCalc.Syntax;

namespace LambdaCalc;

public sealed class AddRequiredParenthesisVisitor : IGreenExpressionSyntax.IVisitor<IGreenExpressionSyntax>
{
    public static readonly AddRequiredParenthesisVisitor Instance = new();
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
    public static T Perform<T>(T syntax) where T : IGreenExpressionSyntax => (T)syntax.Accept(Instance);
}