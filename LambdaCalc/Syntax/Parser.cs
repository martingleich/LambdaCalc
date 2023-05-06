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
    private static bool IsWhitespace(char c) => char.IsWhiteSpace(c);
    private static bool IsIdentifierStart(char c) => char.IsLetter(c);
    private static bool IsIdentifierContinue(char c) => IsIdentifierStart(c) || char.IsDigit(c);
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
        if (value.Length > 0 && !Keywords.Contains(value))
            return new GreenTokenIdentifier(value, leading);
        else
        {
            Cursor = backup;
            return null;
        }
    }
    private GreenTokenIdentifier MatchIdentifier() => TryMatchIdentifier() ?? throw new ParseException(Cursor,"Identifier");

    private T Match<T>(SymbolDefinition<T> def)
    {
#warning Merge with TryMatch
        var leading = TryMatchWhitespace();
        var backup = Cursor;
        var s = 0;
        while (Cursor < Input.Length && s < def.Text.Length && Input[Cursor] == def.Text[s])
        {
            ++Cursor;
            ++s;
        }
        if (s != def.Text.Length)
        {
            Cursor = backup;
            AddParseError(def.Text);
        }
        return def.Factory(leading);
    }

    private T? TryMatch<T>(KeywordDefinition<T> def) => TryMatchKeyword(def.Text, def.Factory);
    private T? TryMatch<T>(SymbolDefinition<T> def) => TryMatchSymbol(def.Text, def.Factory);
    private T? TryMatchSymbol<T>(string symbol, Func<GreenTokenWhitespace?, T> factory)
    {
#warning Parse whitespace only once.
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
    private T? TryMatchKeyword<T>(string word, Func<GreenTokenWhitespace?, T> factory)
    {
#warning Parse whitespace only once.
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

    private record struct SymbolDefinition<T>(string Text, Func<GreenTokenWhitespace?, T> Factory);
    private readonly static SymbolDefinition<GreenTokenAssign> SymbolAssign = new(GreenTokenAssign.FixedGenerating, GreenTokenAssign.New);
    private readonly static SymbolDefinition<GreenTokenBracketOpen> SymbolBracketOpen = new(GreenTokenBracketOpen.FixedGenerating, GreenTokenBracketOpen.New);
    private readonly static SymbolDefinition<GreenTokenBracketClose> SymbolBracketClose = new(GreenTokenBracketClose.FixedGenerating, GreenTokenBracketClose.New);
    private readonly static SymbolDefinition<GreenTokenDots> SymbolDots = new(GreenTokenDots.FixedGenerating, GreenTokenDots.New);
    private readonly static SymbolDefinition<GreenTokenComma> SymbolComma = new(GreenTokenComma.FixedGenerating, GreenTokenComma.New);
    private readonly static SymbolDefinition<GreenTokenDoubleRightArrow> SymbolDoubleRightArrow = new(GreenTokenDoubleRightArrow.FixedGenerating, GreenTokenDoubleRightArrow.New);
    private readonly static SymbolDefinition<GreenTokenParenthesisOpen> SymbolParenthesisOpen = new(GreenTokenParenthesisOpen.FixedGenerating, GreenTokenParenthesisOpen.New);
    private readonly static SymbolDefinition<GreenTokenParenthesisClose> SymbolParenthesisClose = new(GreenTokenParenthesisClose.FixedGenerating, GreenTokenParenthesisClose.New);
    private record struct KeywordDefinition<T>(string Text, Func<GreenTokenWhitespace?, T> Factory);
    private readonly static KeywordDefinition<GreenTokenDef> KeywordDef = new(GreenTokenDef.FixedGenerating, GreenTokenDef.New);
    private readonly static HashSet<string> Keywords = new ()
    {
        KeywordDef.Text,
    };

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
        // Operand = '(' Expr ')' | (Identifier ('=>' Expr)? | '[' Expr '..'? (',' Expr)* ']'
        if (TryMatch(SymbolParenthesisOpen) is { } parenOpen)
        {
            var expression = MatchExpression();
            var parenClose = Match(SymbolParenthesisClose);
            return new GreenParenthesisExpression(parenOpen, expression, parenClose);
        }
        else if (TryMatchIdentifier() is { } identifier)
        {
            if (TryMatch(SymbolDoubleRightArrow) is { } doubleArrowRight)
            {
                var expression = MatchExpression();
                return new GreenLambdaExpression(identifier, doubleArrowRight, expression);
            }
            else
            {
                return new GreenVariableExpression(identifier);
            }
        }
        else if (TryMatch(SymbolBracketOpen) is { } bracketOpen)
        {
            if (TryMatch(SymbolBracketClose) is { } bracketClose)
            {
                return new GreenListExpression(bracketOpen, null, bracketClose);
            }
            var expression = MatchExpression();
            IGreenListContentHead head = TryMatch(SymbolDots) is { } dots
                ? new GreenListContentHeadAppend(expression, dots)
                : new GreenListContentHeadValue(expression);
            var tails = ImmutableArray.CreateBuilder<GreenListContentTail>();
            while (TryMatch(SymbolComma) is { } comma)
            {
                var expression2 = MatchExpression();
                tails.Add(new GreenListContentTail(comma, expression2));
            }
            var bracketClose2 = Match(SymbolBracketClose);
            var listContent = new GreenListContent(head, tails.ToImmutable());
            return new GreenListExpression(bracketOpen, listContent, bracketClose2);
        }
        else
        {
            return null;
        }
    }
    private GreenDefinitionSyntax? TryMatchDefinition()
    {
        if (TryMatch(KeywordDef) is not { } def)
            return null;
        var name = MatchIdentifier();
        var assign = Match(SymbolAssign);
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
