using LambdaCalc.Syntax;

namespace LambdaCalc;

public sealed class AddRequiredWhitespaceVisitor : IGreenExpressionSyntax.IVisitor<IGreenExpressionSyntax, IGreenToken?>
{
    private static readonly AddRequiredWhitespaceVisitor Instance = new();
    public IGreenExpressionSyntax Visit(GreenVariableExpression var, IGreenToken? context)
    {
        if (context is GreenTokenIdentifier && var.Identifier.LeadingWhitespace is null)
            return var.With(var.Identifier.With(var.Identifier.Value, new GreenTokenWhitespace(" ", null)));
        else
            return var;
    }

    public IGreenExpressionSyntax Visit(GreenCallExpression greenCallExpression, IGreenToken? context)
    {
        var newLeft = greenCallExpression.Left.Accept(this, context);
        var newRight = greenCallExpression.Right.Accept(this, newLeft.LastToken);
        return greenCallExpression.With(newLeft, newRight);
    }

    public IGreenExpressionSyntax Visit(GreenLambdaExpression greenLambdaExpression, IGreenToken? context)
    {
        var newExpr = greenLambdaExpression.Expression.Accept(this, greenLambdaExpression.DoubleRightArrow);
        return greenLambdaExpression.With(greenLambdaExpression.ParameterName, greenLambdaExpression.DoubleRightArrow, newExpr);
    }

    public IGreenExpressionSyntax Visit(GreenParenthesisExpression greenParenthesisExpression, IGreenToken? context)
    {
        var newExpr = greenParenthesisExpression.Expression.Accept(this, greenParenthesisExpression.ParenthesisOpen);
        return greenParenthesisExpression.With(greenParenthesisExpression.ParenthesisOpen, newExpr, greenParenthesisExpression.ParenthesisClose);
    }
    public static T Perform<T>(T syntax) where T : IGreenExpressionSyntax => (T)syntax.Accept(Instance, null);
}