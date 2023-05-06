using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Common;
using LambdaCalc.Diagnostics;

namespace LambdaCalc.Syntax;

#region Green
#region Interfaces
public interface IGreenNode
{
    int Length { get; }
    string Generating { get; }
    IGreenToken LastToken { get; }
    IGreenToken FirstToken { get; }
    //INode GetRed(ISyntax? parent, int offset);
    string ToString();
}
public interface IGreenToken : IGreenNode
{
    IGreenToken? LeadingWhitespace { get; }
    IToken GetRed(ISyntax? parent, int offset);
}
public interface IGreenTokenWithValue<out T> : IGreenToken
{
    T Value { get; }
    new IToken GetRed(ISyntax? parent, int offset);
}
public interface IGreenSyntax : IGreenNode
{
    IEnumerable<IGreenNode> Children { get; }
    //new ISyntax GetRed(ISyntax? parent, int offset);
    int GetLengthAt(int index);
}
public interface IGreenExpressionSyntax : IGreenSyntax
{
    IExpressionSyntax GetRed(ISyntax? parent, int offset);
    T Accept<T>(IVisitor<T> visitor);
    T Accept<T, TContext>(IVisitor<T, TContext> visitor, TContext context);
    interface IVisitor<out T>
    {
        T Visit(GreenVariableExpression greenVariableExpression);
        T Visit(GreenCallExpression greenCallExpression);
        T Visit(GreenLambdaExpression greenLambdaExpression);
        T Visit(GreenParenthesisExpression greenParenthesisExpression);
        T Visit(GreenListExpression greenListExpression);
    }
    interface IVisitor<out T, in TContext>
    {
        T Visit(GreenVariableExpression greenVariableExpression, TContext context);
        T Visit(GreenCallExpression greenCallExpression, TContext context);
        T Visit(GreenLambdaExpression greenLambdaExpression, TContext context);
        T Visit(GreenParenthesisExpression greenParenthesisExpression, TContext context);
        T Visit(GreenListExpression greenListExpression, TContext context);
    }
}
#endregion

#region Abstract base classes
public abstract class AGreenNode : IGreenNode
{
    public virtual int Length => Generating.Length;
    public abstract string Generating { get; }
    public abstract IGreenToken LastToken { get; }
    public abstract IGreenToken FirstToken { get; }

    //public abstract INode GetRed(ISyntax? parent, int offset);
    public override string ToString() => Generating;
}
public abstract class AGreenToken : AGreenNode, IGreenToken
{
    protected AGreenToken(IGreenToken? leadingWhitespace)
    {
        LeadingWhitespace = leadingWhitespace;
    }

    public virtual IGreenToken? LeadingWhitespace { get; }
    public override string Generating => (LeadingWhitespace?.Generating ?? "") + OnlyGenerating;
    protected abstract string OnlyGenerating { get; }
    public override IGreenToken FirstToken => LeadingWhitespace?.LastToken ?? this;
    public override IGreenToken LastToken => this;
    public override int Length => (LeadingWhitespace?.Length ?? 0) + OnlyGenerating.Length;

    public abstract IToken GetRed(ISyntax? parent, int offset);
}
public abstract class AGreenTokenWithValue<T> : AGreenToken, IGreenTokenWithValue<T>
{
    protected AGreenTokenWithValue(T value, IGreenToken? leadingWhitespace) : base(leadingWhitespace)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public virtual T Value { get; }

    public override abstract ITokenWithValue<T> GetRed(ISyntax? parent, int offset);
}
public abstract class AGreenSyntax : AGreenNode, IGreenSyntax
{
    public abstract IEnumerable<IGreenNode> Children { get; }
    public override string Generating => string.Concat(Children);
    public override int Length => Children.Sum(c => c.Length);
    public override IGreenToken FirstToken => FirstChild.FirstToken;
    public override IGreenToken LastToken => LastChild.LastToken;
    public abstract IGreenNode FirstChild { get; }
    public abstract IGreenNode LastChild { get; }

    public int GetLengthAt(int index)
    {
        int l = 0;
        foreach (var c in Children)
        {
            if (index == 0)
                return l;
            --index;
            l += c.Length;
        }
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    //public override abstract ISyntax GetRed(ISyntax? parent, int offset);
}
public abstract class AGreenExpressionSyntax : AGreenSyntax, IGreenExpressionSyntax
{
    public abstract T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor);
    public abstract T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context);
    public abstract IExpressionSyntax GetRed(ISyntax? parent, int offset);
}
#endregion

#region Tokens
public sealed class GreenTokenWhitespace : AGreenTokenWithValue<string>
{
    public GreenTokenWhitespace(string value, IGreenToken? leadingWhitespace) : base(value, leadingWhitespace) { }
    protected override string OnlyGenerating => Value;
    public override TokenWhitespace GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenTokenIdentifier : AGreenTokenWithValue<string>
{
    public GreenTokenIdentifier(string value, IGreenToken? leadingWhitespace) : base(value, leadingWhitespace) { }
    protected override string OnlyGenerating => Value;
    public override TokenIdentifier GetRed(ISyntax? parent, int offset) => new(parent, offset, this);

    public GreenTokenIdentifier WithValue(string value) => With(value, LeadingWhitespace);
    public GreenTokenIdentifier With(string value, IGreenToken? leadingWhitespace) =>
        ReferenceEquals(value, Value)
        && ReferenceEquals(leadingWhitespace, LeadingWhitespace)
        ? this
        : new GreenTokenIdentifier(value, leadingWhitespace);
}
#endregion

#region Syntax
public sealed partial class GreenVariableExpression
{
    public static readonly GreenVariableExpression ListEmpty = GreenVariableExpression.New("ListEmpty");
    public static readonly GreenVariableExpression ListHead = GreenVariableExpression.New("ListHead");
    public static GreenVariableExpression New(string name) => new(new GreenTokenIdentifier(name, null));
    public GreenVariableExpression WithIdentifierValue(string value) =>
        With(Identifier.WithValue(value));
    public GreenVariableExpression With(GreenTokenIdentifier identifier) =>
        ReferenceEquals(identifier, this.Identifier)
            ? this
            : new GreenVariableExpression(identifier);
}
public sealed partial class GreenCallExpression
{
    public GreenCallExpression With(IGreenExpressionSyntax left, IGreenExpressionSyntax right) =>
        ReferenceEquals(left, this.Left)
        && ReferenceEquals(right, this.Right)
            ? this
            : new GreenCallExpression(left, right);
}
public sealed partial class GreenLambdaExpression
{
    public GreenLambdaExpression WithExpression(IGreenExpressionSyntax expression) => With(ParameterName, DoubleRightArrow, expression);
    public GreenLambdaExpression With(GreenTokenIdentifier parameterName, GreenTokenDoubleRightArrow doubleRightArrow, IGreenExpressionSyntax expression) =>
        ReferenceEquals(parameterName, this.ParameterName)
        && ReferenceEquals(doubleRightArrow, this.DoubleRightArrow)
        && ReferenceEquals(expression, this.Expression)
            ? this
            : new GreenLambdaExpression(parameterName, doubleRightArrow, expression);
}
public sealed partial class GreenParenthesisExpression
{
    public GreenParenthesisExpression WithExpression(IGreenExpressionSyntax expression) =>
        With(ParenthesisOpen, expression, ParenthesisClose);
    public GreenParenthesisExpression With(GreenTokenParenthesisOpen parenthesisOpen, IGreenExpressionSyntax expression, GreenTokenParenthesisClose parenthesisClose) =>
        ReferenceEquals(parenthesisOpen, this.ParenthesisOpen)
        && ReferenceEquals(expression, this.Expression)
        && ReferenceEquals(parenthesisClose, this.ParenthesisClose)
            ? this
            : new GreenParenthesisExpression(parenthesisOpen, expression, parenthesisClose);
}

public sealed partial class GreenCallExpression
{
    public static GreenCallExpression New(IGreenExpressionSyntax left, IGreenExpressionSyntax right) => new (left, right);
    public static GreenCallExpression New(IGreenExpressionSyntax a, IGreenExpressionSyntax b, IGreenExpressionSyntax c) => New(New(a, b), c);
}
public interface IGreenListContentHead : IGreenSyntax
{
    IListContentHead GetRed(ISyntax parent, int offset);
    IGreenExpressionSyntax Desugared { get; }
    T Accept<T>(IVisitor<T> visitor);
    interface IVisitor<T>
    {
        T Visit(GreenListContentHeadAppend append);
        T Visit(GreenListContentHeadValue value);
    }
}
public sealed partial class GreenListContentHeadAppend : IGreenListContentHead
{
    IListContentHead IGreenListContentHead.GetRed(ISyntax parent, int offset) => GetRed(parent, offset);

    public GreenListContentHeadAppend WithValue(IGreenExpressionSyntax value) =>
        ReferenceEquals(Value, value)
        ? this
        : new GreenListContentHeadAppend(value, Dots);

    public T Accept<T>(IGreenListContentHead.IVisitor<T> visitor) => visitor.Visit(this);
    public IGreenExpressionSyntax Desugared => this.Value;
}

public sealed partial class GreenListContentTail
{
    public GreenListContentTail WithValue(IGreenExpressionSyntax value) =>
        ReferenceEquals(Value, value)
        ? this
        : new GreenListContentTail(Comma, value);
}
public sealed partial class GreenListContentHeadValue : IGreenListContentHead
{
    IListContentHead IGreenListContentHead.GetRed(ISyntax parent, int offset) => GetRed(parent, offset);

    public GreenListContentHeadValue WithValue(IGreenExpressionSyntax value) =>
        ReferenceEquals(Value, value)
        ? this
        : new GreenListContentHeadValue(value);

    public T Accept<T>(IGreenListContentHead.IVisitor<T> visitor) => visitor.Visit(this);
    private static readonly GreenCallExpression ListHeadListEmpty = GreenCallExpression.New(GreenVariableExpression.ListHead, GreenVariableExpression.ListEmpty);
    public IGreenExpressionSyntax Desugared => GreenCallExpression.New(ListHeadListEmpty, this.Value);
}

public sealed partial class GreenListContent
{
    public IGreenExpressionSyntax Desugared => Rest.Aggregate(this.Head.Desugared, (accum, tail) =>
        GreenCallExpression.New(GreenVariableExpression.ListHead, accum, tail.Value));
}
public sealed partial class GreenListExpression : AGreenExpressionSyntax
{
    public IGreenExpressionSyntax Desugared => Content?.Desugared ?? GreenVariableExpression.ListEmpty;

    public GreenListExpression WithContent(GreenListContent newContent) =>
        ReferenceEquals(newContent, Content)
        ? this
        : new GreenListExpression(BracketOpen, newContent, BracketClose);
}

#endregion

public static class ToTokenListEx
{
    public static IEnumerable<IGreenToken> ToTokenList(this IGreenNode self) => self switch
    {
        IGreenToken tok => tok.ToTokenList(),
        IGreenSyntax syntax => syntax.ToTokenList(),
        _ => throw new InvalidOperationException()
    };
    public static IEnumerable<IGreenToken> ToTokenList(this IGreenToken self) => new[] { self };
    public static IEnumerable<IGreenToken> ToTokenList(this IGreenSyntax self) => self.Children.SelectMany(ToTokenList);
}
#endregion

#region Red
public interface INode
{
    ISyntax? Parent { get; }
    IGreenNode Green { get; }
    int Offset { get; }
    int Length => Green.Length;
    string Generating => Green.Generating;
    string ToString();
}
public interface IToken : INode
{
    new IGreenToken Green { get; }
    IToken? LeadingWhitespace { get; }
}
public interface ITokenWithValue<out T> : IToken
{
    new IGreenTokenWithValue<T> Green { get; }
    T Value { get; }
}
public interface ISyntax : INode
{
    IEnumerable<INode> Children { get; }
    T Accept<T>(IVisitor<T> visitor);
    interface IVisitor<out T>
    {
        T Visit(VariableExpression variableExpression);
        T Visit(CallExpression callExpression);
        T Visit(LambdaExpression lambdaExpression);
        T Visit(ParenthesisExpression parenthesisExpression);
        T Visit(FileSyntax file);
        T Visit(DefinitionSyntax definition);
        T Accept(TopLevelExpressionSyntax topLevelExpressionSyntax);
        T Visit(ListContentHeadAppend listContentHeadAppend);
        T Visit(ListContentHeadValue listContentHeadValue);
        T Visit(ListContentTail listContentTail);
        T Visit(ListContent listContent);
        T Visit(ListExpression listExpression);
        T Visit(TopLevelExpressionSyntax topLevelExpressionSyntax);
    }
}
public interface IRootSyntax : ISyntax
{
    Project Project { get; }
}

public interface IExpressionSyntax : ISyntax
{
    T Accept<T>(IVisitor<T> visitor);
    T Accept<T, TContext>(IVisitor<T, TContext> visitor, TContext context);
    new interface IVisitor<out T>
    {
        T Visit(VariableExpression variableExpression);
        T Visit(CallExpression callExpression);
        T Visit(LambdaExpression lambdaExpression);
        T Visit(ParenthesisExpression parenthesisExpression);
        T Visit(ListExpression listExpression);
    }
    interface IVisitor<out T, in TContext>
    {
        T Visit(VariableExpression variableExpression, TContext ctx);
        T Visit(CallExpression callExpression, TContext ctx);
        T Visit(LambdaExpression lambdaExpression, TContext ctx);
        T Visit(ParenthesisExpression parenthesisExpression, TContext ctx);
        T Visit(ListExpression listExpression, TContext context);
    }
}

#region Token
public abstract class ANode : INode
{
    protected ANode(ISyntax? parent, int offset)
    {
        Parent = parent;
        Offset = offset;
    }

    public ISyntax? Parent { get; }
    public int Offset { get; }
    public abstract IGreenNode Green { get; }
    public override string ToString() => Green.ToString();
}
public abstract class AToken : ANode, IToken
{
    public AToken(ISyntax? parent, int offset) : base(parent, offset) { }

    private IToken? _leadingWhitespace;
    public IToken? LeadingWhitespace
    {
        get
        {
            if (Green.LeadingWhitespace is { } greenLeadingWhitespace)
                _leadingWhitespace = greenLeadingWhitespace.GetRed(Parent, Offset + (greenLeadingWhitespace.LeadingWhitespace?.Length ?? 0));
            return _leadingWhitespace;
        }
    }
    public abstract override IGreenToken Green { get; }
}
public abstract class ATokenWithValue<T> : AToken, ITokenWithValue<T>
{
    public ATokenWithValue(ISyntax? parent, int offset) : base(parent, offset) { }
    public T Value => Green.Value;
    public override abstract IGreenTokenWithValue<T> Green { get; }
}
public abstract class ASyntax : ANode, ISyntax
{
    protected ASyntax(ISyntax? parent, int offset) : base(parent, offset)
    {
    }

    public abstract IEnumerable<INode> Children { get; }
    public override abstract IGreenSyntax Green { get; }
    public abstract T Accept<T>(ISyntax.IVisitor<T> visitor);
    public int GetOffsetAt(int index) => Offset + Green.GetLengthAt(index);
}
public sealed class TokenWhitespace : ATokenWithValue<string>
{
    public TokenWhitespace(ISyntax? parent, int offset, GreenTokenWhitespace green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenWhitespace Green { get; }
}
public sealed class TokenIdentifier : ATokenWithValue<string>
{
    public TokenIdentifier(ISyntax? parent, int offset, GreenTokenIdentifier green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenIdentifier Green { get; }
}
#endregion
#region Syntax
public interface IListContentHead : ISyntax
{
    (IExpressionSyntax? AppendTo, IEnumerable<IExpressionSyntax> Values) GetValues(IEnumerable<IExpressionSyntax> tail);
}
public sealed partial class ListContentHeadAppend : IListContentHead
{
    public (IExpressionSyntax? AppendTo, IEnumerable<IExpressionSyntax> Values) GetValues(IEnumerable<IExpressionSyntax> tail) =>
        (Value, tail);
}
public sealed partial class ListContentHeadValue : IListContentHead
{
    public (IExpressionSyntax? AppendTo, IEnumerable<IExpressionSyntax> Values) GetValues(IEnumerable<IExpressionSyntax> tail) =>
        (null, tail.Prepend(Value));
}

#endregion
#endregion

public sealed class Project
{
    public readonly GreenFileSyntax GreenFile;
    private FileSyntax? _file;
    public FileSyntax File => _file ??= GreenFile.GetRed(this, 0);

    public readonly GreenFileSyntax GreenSystemFile;
    private FileSyntax? _systemFile;
    public FileSyntax SystemFile => _systemFile ??= GreenSystemFile.GetRed(this, 0);

    private Project(GreenFileSyntax greenFile, GreenFileSyntax greenSystemFile, ImmutableArray<IDiagnostic> parserDiagnostics)
    {
        GreenFile = greenFile ?? throw new ArgumentNullException(nameof(greenFile));
        GreenSystemFile = greenSystemFile;
        _parserDiagnostics = parserDiagnostics;
    }

    private readonly ImmutableArray<IDiagnostic> _parserDiagnostics;
    public static readonly Project Empty = new(
        new GreenFileSyntax(ImmutableArray<GreenDefinitionSyntax>.Empty, GreenTokenEndOfFile.Instance),
        ParseNoError(@"
def ListEmpty =           x => y => x
def ListHead  = h => v => x => y => y h v"),
        ImmutableArray<IDiagnostic>.Empty);
    private static GreenFileSyntax ParseNoError(string text)
    {
        var bag = new DiagnosticsBag();
        var result = Parser.ParseFile(text, bag);
        if (bag.Count > 0)
            throw new ArgumentException(string.Join(Environment.NewLine, bag));
        return result;
    }

    public Project SetFileText(string text)
    {
        var parserDiagnostics = new DiagnosticsBag();
        var syntax = Parser.ParseFile(text, parserDiagnostics);
        return new Project(syntax, GreenSystemFile, parserDiagnostics.ToImmutableArray());
    }

    // Compiler
    private CompilerCache? _compiler;
    private CompilerCache CompilerCache => _compiler ??= new CompilerCache(SystemFile.Definitions[0], SystemFile.Definitions[1]);
    internal IStructural? UnsafeCompile(DefinitionSyntax definition)
    {
        if (HasErrors)
            return null;
        return CompilerCache.Compile(definition);
    }

    // Errors
    public bool HasErrors => GetAllErrors().Any();
    public IEnumerable<IDiagnostic> GetAllErrors() => GetAllErrorsBlocks().SelectMany(x => x);
    private IEnumerable<IEnumerable<IDiagnostic>> GetAllErrorsBlocks()
    {
        yield return _parserDiagnostics;
        yield return GetErrors(File);
        yield return GetErrors(SystemFile);
    }
    private IEnumerable<IDiagnostic> GetErrors(INode node)
    {
        return from variable in node.AllNodesOfType<VariableExpression>()
               where variable.GetDefinition() is null
               select new UnknownVariableDiagnostic(variable);
    }

    // Asserts
    public CheckEqualResult CheckEqual(string left, string right)
    {
        var parserDiagnostics = new DiagnosticsBag();
        var leftExpr = Parser.ParseExpression(left, parserDiagnostics);
        var rightExpr = Parser.ParseExpression(right, parserDiagnostics);
        var parseErrors = parserDiagnostics.ToImmutableArray();
        if (parseErrors.Length > 0)
            return new CheckEqualResult(parseErrors, false);
        var redLeftExpr = leftExpr.GetRed(this, 0);
        var redRightExpr = rightExpr.GetRed(this, 0);
        var typeDiagnostics = GetErrors(redLeftExpr).Concat(GetErrors(redRightExpr)).ToImmutableArray();
        if (typeDiagnostics.Length > 0)
            return new CheckEqualResult(typeDiagnostics, false);
        var leftCompiled = new CompiledLambda(CompilerCache.Compile(redLeftExpr.Expression).Evaluate());
        var rightCompiled = new CompiledLambda(CompilerCache.Compile(redRightExpr.Expression).Evaluate());
        var result = leftCompiled.Equals(rightCompiled);
        return new CheckEqualResult(ImmutableArray<IDiagnostic>.Empty, result);
    }
}

public sealed class CheckEqualResult
{
    public readonly ImmutableArray<IDiagnostic> Diagnostics;
    public readonly bool RunResult;

    internal CheckEqualResult(ImmutableArray<IDiagnostic> diagnostics, bool runResult)
    {
        Diagnostics = diagnostics;
        RunResult = runResult;
    }

    public override string ToString()
    {
        if (Diagnostics.Length > 0)
            return $"FAILED: [{string.Join(", ", Diagnostics)}]";
        else if (!RunResult)
            return $"FAILED";
        else
            return "SUCCEEDED";
    }
}
