using LambdaCalc.Syntax;

namespace LambdaCalc;

public sealed class AddRequiredWhitespaceVisitor
{
    public static T Perform<T>(T syntax) where T : IGreenExpressionSyntax
    {
        var newText = string.Join("", Perform(syntax.ToTokenList()).Select(tok => tok.Generating));
        return (T)Parser.ParseExpression(newText, new Diagnostics.DiagnosticsBag()).Expression;
    }
    private static IEnumerable<IGreenToken> Perform(IEnumerable<IGreenToken> tokens)
    {
        var previous = default(IGreenToken?);
        foreach (var t in tokens)
        {
            if (t is GreenTokenIdentifier identifier && previous is GreenTokenIdentifier && identifier.LeadingWhitespace is null)
                previous = identifier.With(identifier.Value, new GreenTokenWhitespace(" ", null));
            else
                previous = t;
            yield return previous;
        }
    }
}