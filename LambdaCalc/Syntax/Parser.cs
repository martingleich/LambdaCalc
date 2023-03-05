using LambdaCalc.Diagnostics;
using System.Collections.Immutable;

namespace LambdaCalc.Syntax;

internal sealed class Parser
{
    private readonly string Input;
    private readonly DiagnosticsBag Diagnostics;
    private int Cursor;

    private Parser(string input, int cursor, DiagnosticsBag diagnostics)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        Cursor = cursor;
        Diagnostics = diagnostics;
    }
    private void AddParseError(string expected)
    {
        Diagnostics.Add(new ParserErrorDiagnostic(Location.FromOffsetLength(Cursor, 0), expected));
    }
    private GreenTokenWhitespace? TryMatchWhitespace()
    {
        int start = Cursor;
        int roundStart;
        do
        {
            roundStart = Cursor;
            // Any whitespace
            while (Cursor < Input.Length && IsWhitespace(Input[Cursor]))
                ++Cursor;
            // Line comment
            if (Cursor < Input.Length && Input[Cursor] == '#')
            {
                ++Cursor;
                while (Cursor < Input.Length && Input[Cursor] != '\n')
                    ++Cursor;
            }
        } while (roundStart != Cursor);
        if (start != Cursor)
            return new GreenTokenWhitespace(Input[start..Cursor], null);
        else
            return null;
    }
    private static bool IsWhitespace(char c)
    {
        //return (c >= '\x0009' && c <= '\x000D') || c == '\x0020' || c == '\x0085' || (c >= '\x200E' || c <= '\x200F') || c == '\x2028' || c == '\x2029';
        return char.IsWhiteSpace(c);
    }
    private static bool IsIdentifierStart(char c) => char.IsLetter(c);
    private static bool IsIdentifierContinue(char c) => IsIdentifierStart(c) || char.IsDigit(c);
    private GreenTokenDef? TryMatchDef() => TryMatchWord(GreenTokenDef.FixedGenerating, l => new GreenTokenDef(l));
    private GreenTokenIdentifier? TryMatchIdentifier()
    {
        var backup = Cursor;
        var leading = TryMatchWhitespace();
        var start = Cursor;
        if (Cursor < Input.Length && IsIdentifierStart(Input[Cursor]))
        {
            ++Cursor;
            while (Cursor < Input.Length && IsIdentifierContinue(Input[Cursor]))
                ++Cursor;
        }
        var value = Input[start..Cursor];
        if (value.Length > 0 && value != GreenTokenDef.FixedGenerating)
            return new GreenTokenIdentifier(value, leading);
        else
        {
            Cursor = backup;
            return null;
        }
    }
    private GreenTokenIdentifier MatchIdentifier() => TryMatchIdentifier() ?? throw new ParseException(Cursor,"Identifier");

    private T MatchSymbol<T>(string symbol, Func<GreenTokenWhitespace?, T> factory)
    {
        var backup = Cursor;
        var leading = TryMatchWhitespace();
        int s = 0;
        while (Cursor < Input.Length && s < symbol.Length && Input[Cursor] == symbol[s])
        {
            ++Cursor;
            ++s;
        }
        if (s != symbol.Length)
        {
            Cursor = backup;
            AddParseError(GreenTokenAssign.FixedGenerating);
        }
        return factory(leading);
    }
    private T? TryMatchSymbol<T>(string symbol, Func<GreenTokenWhitespace?, T> factory)
    {
        var backup = Cursor;
        var leading = TryMatchWhitespace();
        int s = 0;
        while (Cursor < Input.Length && s < symbol.Length && Input[Cursor] == symbol[s])
        {
            ++Cursor;
            ++s;
        }
        if (s == symbol.Length)
        {
            return factory(leading);
        }
        else
        {
            Cursor = backup;
            return default;
        }
    }
    private T? TryMatchWord<T>(string word, Func<GreenTokenWhitespace?, T> factory)
    {
        var backup = Cursor;
        var leading = TryMatchWhitespace();
        int s = 0;
        if (Cursor < Input.Length && s < word.Length && word[s] == Input[Cursor])
        {
            while (Cursor < Input.Length && s < word.Length && Input[Cursor] == word[s])
            {
                ++Cursor;
                ++s;
            }
        }
        if (s == word.Length && !(Cursor < Input.Length && IsIdentifierContinue(Input[Cursor])))
            return factory(leading);
        else
        {
            Cursor = backup;
            return default;
        }
    }
    private GreenTokenAssign MatchAssign() => MatchSymbol(GreenTokenAssign.FixedGenerating, l => new GreenTokenAssign(l));
    private GreenTokenParenthesisOpen? TryMatchParenthesisOpen() => TryMatchSymbol(GreenTokenParenthesisOpen.FixedGenerating, l => new GreenTokenParenthesisOpen(l));
    private GreenTokenDoubleRightArrow? TryMatchDoubleArrowRight() => TryMatchSymbol(GreenTokenDoubleRightArrow.FixedGenerating, l => new GreenTokenDoubleRightArrow(l));
    private GreenTokenParenthesisClose MatchParenthesisClose() => MatchSymbol(GreenTokenParenthesisClose.FixedGenerating, l => new GreenTokenParenthesisClose(l));

    private GreenTokenEndOfFile? TryMatchEndOfFile()
    {
        var backup = Cursor;
        var leading = TryMatchWhitespace();
        if (Cursor == Input.Length)
            return new GreenTokenEndOfFile(leading);
        else
        {
            Cursor = backup;
            return null;
        }
    }
    private GreenTokenEndOfFile MatchEndOfFile()
    {
        if (TryMatchEndOfFile() is { } eof)
            return eof;
        AddParseError("End of file");
        eof = new GreenTokenEndOfFile(new GreenTokenWhitespace(Input[Cursor..], null));
        Cursor = Input.Length;
        return eof;
    }
    private IGreenExpressionSyntax? TryMatchExpression()
    {
        if (TryMatchOperand() is { } left)
        {
            while (TryMatchOperand() is { } right)
                left = new GreenCallExpression(left, right);
            return left;
        }
        else
        {
            return null;
        }
    }
    private GreenTopLevelExpressionSyntax MatchTopLevelExpression()
    {
        IGreenExpressionSyntax expr;
        try
        {
            expr = MatchExpression();
        }
        catch (ParseException exp2)
        {
            expr = new GreenVariableExpression(new GreenTokenIdentifier("", null));
            Diagnostics.Add(exp2.Error);
        }
        GreenTokenEndOfFile eof;
        try
        {
            eof = MatchEndOfFile();
        }
        catch (ParseException exp)
        {
            eof = new GreenTokenEndOfFile(new GreenTokenWhitespace(Input[Cursor..], null));
            Cursor = Input.Length;
            Diagnostics.Add(exp.Error);
        }
        return new GreenTopLevelExpressionSyntax(expr, eof);
    }
    private IGreenExpressionSyntax MatchExpression() => TryMatchExpression() ?? throw new ParseException(Cursor, "Expression");
    private IGreenExpressionSyntax? TryMatchOperand()
    {
        // Expr = '(' Expr ')' | (Identifier ('=>' Expr)?
        if (TryMatchParenthesisOpen() is { } parenOpen)
        {
            var expression = MatchExpression();
            var parenClose = MatchParenthesisClose();
            return new GreenParenthesisExpression(parenOpen, expression, parenClose);
        }
        else if (TryMatchIdentifier() is { } identifier)
        {
            if (TryMatchDoubleArrowRight() is { } doubleArrowRight)
            {
                var expression = MatchExpression();
                return new GreenLambdaExpression(identifier, doubleArrowRight, expression);
            }
            else
            {
                return new GreenVariableExpression(identifier);
            }
        }
        else
        {
            return null;
        }
    }
    private GreenDefinitionSyntax? TryMatchDefinition()
    {
        if (TryMatchDef() is not { } def)
            return null;
        var name = MatchIdentifier();
        var assign = MatchAssign();
        var value = MatchExpression();
        return new GreenDefinitionSyntax(def, name, assign, value);
    }
    private GreenFileSyntax MatchFile()
    {
        var definitions = ImmutableArray.CreateBuilder<GreenDefinitionSyntax>();
        GreenTokenEndOfFile eof;
        try
        {
            while (TryMatchDefinition() is { } def)
                definitions.Add(def);
            eof = MatchEndOfFile();
        }
        catch (ParseException exp)
        {
            eof = new GreenTokenEndOfFile(new GreenTokenWhitespace(Input[Cursor..], null));
            Cursor = Input.Length;
            Diagnostics.Add(exp.Error);
        }
        return new GreenFileSyntax(definitions.ToImmutable(), eof);
    }
    public static GreenFileSyntax ParseFile(string input, DiagnosticsBag diagnostics) => new Parser(input, 0, diagnostics).MatchFile();
    public static GreenTopLevelExpressionSyntax ParseExpression(string input, DiagnosticsBag diagnostics) => new Parser(input, 0, diagnostics).MatchTopLevelExpression();
    private class ParseException : Exception
    {
        public readonly ParserErrorDiagnostic Error;

        public ParseException(int offset, string expected)
        {
            Error = new ParserErrorDiagnostic(Location.FromOffsetLength(offset, 0), expected);
        }
    }
}
