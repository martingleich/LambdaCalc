using System.Collections.Immutable;
using System.Reflection.Metadata;
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
    INode GetRed(ISyntax? parent, int offset);
    string ToString();
}
public interface IGreenToken : IGreenNode
{
    IGreenToken? LeadingWhitespace { get; }
    new IToken GetRed(ISyntax? parent, int offset);
}
public interface IGreenTokenWithValue<out T> : IGreenToken
{
    T Value { get; }
    new IToken GetRed(ISyntax? parent, int offset);
}
public interface IGreenSyntax : IGreenNode
{
    IEnumerable<IGreenNode> Children { get; }
    new ISyntax GetRed(ISyntax? parent, int offset);
    int GetLengthAt(int index);
}
public interface IGreenExpressionSyntax : IGreenSyntax
{
    new IExpressionSyntax GetRed(ISyntax? parent, int offset);
    T Accept<T>(IVisitor<T> visitor);
    T Accept<T, TContext>(IVisitor<T, TContext> visitor, TContext context);
    interface IVisitor<out T>
    {
        T Visit(GreenVariableExpression greenVariableExpression);
        T Visit(GreenCallExpression greenCallExpression);
        T Visit(GreenLambdaExpression greenLambdaExpression);
        T Visit(GreenParenthesisExpression greenParenthesisExpression);
    }
    interface IVisitor<out T, in TContext>
    {
        T Visit(GreenVariableExpression greenVariableExpression, TContext context);
        T Visit(GreenCallExpression greenCallExpression, TContext context);
        T Visit(GreenLambdaExpression greenLambdaExpression, TContext context);
        T Visit(GreenParenthesisExpression greenParenthesisExpression, TContext context);
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

    public abstract INode GetRed(ISyntax? parent, int offset);
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

    public override abstract IToken GetRed(ISyntax? parent, int offset);
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

    public override abstract ISyntax GetRed(ISyntax? parent, int offset);
}
public abstract class AGreenExpressionSyntax : AGreenSyntax, IGreenExpressionSyntax
{
    public abstract T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor);
    public abstract T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context);
    public override abstract IExpressionSyntax GetRed(ISyntax? parent, int offset);
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
public sealed class GreenTokenParenthesisOpen : AGreenToken
{
    public GreenTokenParenthesisOpen(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly string FixedGenerating = "(";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenParenthesisOpen GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenTokenParenthesisClose : AGreenToken
{
    public GreenTokenParenthesisClose(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly string FixedGenerating = ")";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenParenthesisClose GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenTokenDoubleRightArrow : AGreenToken
{
    public static readonly GreenTokenDoubleRightArrow Instance = new (default);
    public GreenTokenDoubleRightArrow(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly string FixedGenerating = "=>";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenDoubleRightArrow GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenTokenDef : AGreenToken
{
    public GreenTokenDef(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly string FixedGenerating = "def";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenDef GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenTokenAssign : AGreenToken
{
    public GreenTokenAssign(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly string FixedGenerating = "=";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenAssign GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenTokenEndOfFile : AGreenToken
{
    public static readonly GreenTokenEndOfFile Instance = new(null);
    public GreenTokenEndOfFile(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    protected override string OnlyGenerating => "";
    public override TokenEndOfFile GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
#endregion

#region Syntax
public sealed class GreenVariableExpression : AGreenExpressionSyntax
{
    public readonly GreenTokenIdentifier Identifier;

    public static GreenVariableExpression New(string name) => new (new GreenTokenIdentifier(name, null));

    public GreenVariableExpression(GreenTokenIdentifier identifier)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Identifier;
        }
    }

    public override IGreenNode FirstChild => Identifier;
    public override IGreenNode LastChild => Identifier;

    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
    public GreenVariableExpression WithIdentifierValue(string value) =>
        With(Identifier.WithValue(value));
    public GreenVariableExpression With(GreenTokenIdentifier identifier) =>
        ReferenceEquals(identifier, this.Identifier)
            ? this
            : new GreenVariableExpression(identifier);
    public override VariableExpression GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenCallExpression : AGreenExpressionSyntax, IGreenSyntax
{
    public readonly IGreenExpressionSyntax Left;
    public readonly IGreenExpressionSyntax Right;

    public GreenCallExpression(IGreenExpressionSyntax left, IGreenExpressionSyntax right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public override IEnumerable<IGreenExpressionSyntax> Children
    {
        get
        {
            yield return Left;
            yield return Right;
        }
    }
    public override IGreenNode FirstChild => Left;
    public override IGreenNode LastChild => Right;
    IEnumerable<IGreenNode> IGreenSyntax.Children => Children;

    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
    public GreenCallExpression With(IGreenExpressionSyntax left, IGreenExpressionSyntax right) =>
        ReferenceEquals(left, this.Left)
        && ReferenceEquals(right, this.Right)
            ? this
            : new GreenCallExpression(left, right);
    public override CallExpression GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenLambdaExpression : AGreenExpressionSyntax
{
    public readonly GreenTokenIdentifier ParameterName;
    public readonly GreenTokenDoubleRightArrow DoubleRightArrow;
    public readonly IGreenExpressionSyntax Expression;

    public GreenLambdaExpression(GreenTokenIdentifier parameterName, GreenTokenDoubleRightArrow doubleRightArrow, IGreenExpressionSyntax expression)
    {
        ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
        DoubleRightArrow = doubleRightArrow ?? throw new ArgumentNullException(nameof(doubleRightArrow));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return ParameterName;
            yield return DoubleRightArrow;
            yield return Expression;
        }
    }
    public override IGreenNode FirstChild => ParameterName;
    public override IGreenNode LastChild => Expression;
    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public GreenLambdaExpression WithExpression(IGreenExpressionSyntax expression) => With(ParameterName, DoubleRightArrow, Expression);
    public GreenLambdaExpression With(GreenTokenIdentifier parameterName, GreenTokenDoubleRightArrow doubleRightArrow, IGreenExpressionSyntax expression) =>
        ReferenceEquals(parameterName, this.ParameterName)
        && ReferenceEquals(doubleRightArrow, this.DoubleRightArrow)
        && ReferenceEquals(expression, this.Expression)
            ? this
            : new GreenLambdaExpression(parameterName, doubleRightArrow, expression);
    public override LambdaExpression GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenParenthesisExpression : AGreenExpressionSyntax
{
    public readonly GreenTokenParenthesisOpen ParenthesisOpen;
    public readonly IGreenExpressionSyntax Expression;
    public readonly GreenTokenParenthesisClose ParenthesisClose;

    public GreenParenthesisExpression(GreenTokenParenthesisOpen parenthesisOpen, IGreenExpressionSyntax expression, GreenTokenParenthesisClose parenthesisClose)
    {
        ParenthesisOpen = parenthesisOpen ?? throw new ArgumentNullException(nameof(parenthesisOpen));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        ParenthesisClose = parenthesisClose ?? throw new ArgumentNullException(nameof(parenthesisClose));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return ParenthesisOpen;
            yield return Expression;
            yield return ParenthesisClose;
        }
    }
    public override IGreenNode FirstChild => ParenthesisOpen;
    public override IGreenNode LastChild => ParenthesisClose;
    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);

    public GreenParenthesisExpression WithExpression(IGreenExpressionSyntax expression) =>
        With(ParenthesisOpen, expression, ParenthesisClose);
    public GreenParenthesisExpression With(GreenTokenParenthesisOpen parenthesisOpen, IGreenExpressionSyntax expression, GreenTokenParenthesisClose parenthesisClose) =>
        ReferenceEquals(parenthesisOpen, this.ParenthesisOpen)
        && ReferenceEquals(expression, this.Expression)
        && ReferenceEquals(parenthesisClose, this.ParenthesisClose)
            ? this
            : new GreenParenthesisExpression(parenthesisOpen, expression, parenthesisClose);
    public override ParenthesisExpression GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenDefinitionSyntax : AGreenSyntax
{
    public readonly GreenTokenDef Def;
    public readonly GreenTokenIdentifier Name;
    public readonly GreenTokenAssign Assign;
    public readonly IGreenExpressionSyntax Value;

    public GreenDefinitionSyntax(GreenTokenDef def, GreenTokenIdentifier name, GreenTokenAssign assign, IGreenExpressionSyntax value)
    {
        Def = def ?? throw new ArgumentNullException(nameof(def));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Assign = assign ?? throw new ArgumentNullException(nameof(assign));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Def;
            yield return Name;
            yield return Assign;
            yield return Value;
        }
    }
    public override GreenTokenDef FirstChild => Def;
    public override IGreenExpressionSyntax LastChild => Value;
    public override DefinitionSyntax GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenFileSyntax : AGreenSyntax
{
    public readonly ImmutableArray<GreenDefinitionSyntax> Definitions;
    public readonly GreenTokenEndOfFile EndOfFile;

    public GreenFileSyntax(ImmutableArray<GreenDefinitionSyntax> definitions, GreenTokenEndOfFile endOfFile)
    {
        Definitions = definitions;
        EndOfFile = endOfFile ?? throw new ArgumentNullException(nameof(endOfFile));
    }

    public override IEnumerable<IGreenNode> Children => Definitions.Append<IGreenNode>(EndOfFile);
    public override IGreenNode FirstChild => Definitions.Length > 0 ? Definitions[0] : EndOfFile;
    public override IGreenNode LastChild => EndOfFile;
    public override FileSyntax GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed class GreenTopLevelExpressionSyntax : AGreenSyntax
{
    public readonly IGreenExpressionSyntax Expression;
    public readonly GreenTokenEndOfFile EndOfFile;

    public GreenTopLevelExpressionSyntax(IGreenExpressionSyntax expression, GreenTokenEndOfFile endOfFile)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        EndOfFile = endOfFile ?? throw new ArgumentNullException(nameof(endOfFile));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Expression;
            yield return EndOfFile;
        }
    }

    public override IGreenNode FirstChild => Expression;
    public override IGreenNode LastChild => EndOfFile;

    public override TopLevelExpressionSyntax GetRed(ISyntax? parent, int offset) => new (parent, offset, this);
}
#endregion
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
        T Visit(Project project);
        T Visit(VariableExpression variableExpression);
        T Visit(CallExpression callExpression);
        T Visit(LambdaExpression lambdaExpression);
        T Visit(ParenthesisExpression parenthesisExpression);
        T Visit(FileSyntax file);
        T Visit(DefinitionSyntax definition);
        T Accept(TopLevelExpressionSyntax topLevelExpressionSyntax);
    }
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
    }
    interface IVisitor<out T, in TContext>
    {
        T Visit(VariableExpression variableExpression, TContext ctx);
        T Visit(CallExpression callExpression, TContext ctx);
        T Visit(LambdaExpression lambdaExpression, TContext ctx);
        T Visit(ParenthesisExpression parenthesisExpression, TContext ctx);
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
public sealed class TokenParenthesisOpen : AToken
{
    public TokenParenthesisOpen(ISyntax? parent, int offset, GreenTokenParenthesisOpen green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenParenthesisOpen Green { get; }
}
public sealed class TokenParenthesisClose : AToken
{
    public TokenParenthesisClose(ISyntax? parent, int offset, GreenTokenParenthesisClose green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenParenthesisClose Green { get; }
}
public sealed class TokenDoubleRightArrow : AToken
{
    public TokenDoubleRightArrow(ISyntax? parent, int offset, GreenTokenDoubleRightArrow green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenDoubleRightArrow Green { get; }
}
public sealed class TokenDef : AToken
{
    public TokenDef(ISyntax? parent, int offset, GreenTokenDef green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenDef Green { get; }
}
public sealed class TokenAssign : AToken
{
    public TokenAssign(ISyntax? parent, int offset, GreenTokenAssign green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenAssign Green { get; }
}
public sealed class TokenEndOfFile : AToken
{
    public TokenEndOfFile(ISyntax? parent, int offset, GreenTokenEndOfFile green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenEndOfFile Green { get; }
}
#endregion
#region Syntax
public sealed class VariableExpression : ASyntax, IExpressionSyntax
{
    public override GreenVariableExpression Green { get; }
    private TokenIdentifier? _identifier;

    public VariableExpression(ISyntax? parent, int offset, GreenVariableExpression green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    public TokenIdentifier Identifier => _identifier ??= Green.Identifier.GetRed(this, GetOffsetAt(0));
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext ctx) => visitor.Visit(this, ctx);
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Identifier;
        }
    }
}
public sealed class CallExpression : ASyntax, IExpressionSyntax
{
    public override GreenCallExpression Green { get; }

    public CallExpression(ISyntax? parent, int offset, GreenCallExpression green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    private IExpressionSyntax? _left;
    public IExpressionSyntax Left => _left ??= Green.Left.GetRed(this, GetOffsetAt(0));
    private IExpressionSyntax? _right;
    public IExpressionSyntax Right => _right ??= Green.Right.GetRed(this, GetOffsetAt(1));
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext ctx) => visitor.Visit(this, ctx);
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Left;
            yield return Right;
        }
    }
}
public sealed class LambdaExpression : ASyntax, IExpressionSyntax
{
    public override GreenLambdaExpression Green { get; }

    public LambdaExpression(ISyntax? parent, int offset, GreenLambdaExpression green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    private TokenIdentifier? _parameterName;
    public TokenIdentifier ParameterName => _parameterName ??= Green.ParameterName.GetRed(this, GetOffsetAt(0));
    private TokenDoubleRightArrow? _doubleRightArrow;
    public TokenDoubleRightArrow DoubleRightArrow => _doubleRightArrow ??= Green.DoubleRightArrow.GetRed(this, GetOffsetAt(1));
    private IExpressionSyntax? _expression;
    public IExpressionSyntax Expression => _expression ??= Green.Expression.GetRed(this, GetOffsetAt(2));
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext ctx) => visitor.Visit(this, ctx);
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return ParameterName;
            yield return DoubleRightArrow;
            yield return Expression;
        }
    }
}
public sealed class ParenthesisExpression : ASyntax, IExpressionSyntax
{
    public override GreenParenthesisExpression Green { get; }

    public ParenthesisExpression(ISyntax? parent, int offset, GreenParenthesisExpression green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    private TokenParenthesisOpen? _parenthesisOpen;
    public TokenParenthesisOpen ParenthesisOpen => _parenthesisOpen ??= Green.ParenthesisOpen.GetRed(this, GetOffsetAt(0));
    private IExpressionSyntax? _expression;
    public IExpressionSyntax Expression => _expression ??= Green.Expression.GetRed(this, GetOffsetAt(1));
    private TokenParenthesisClose? _parenthesisClose;
    public TokenParenthesisClose ParenthesisClose => _parenthesisClose ??= Green.ParenthesisClose.GetRed(this, GetOffsetAt(2));
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext ctx) => visitor.Visit(this, ctx);
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return ParenthesisOpen;
            yield return Expression;
            yield return ParenthesisClose;
        }
    }
}
public sealed class DefinitionSyntax : ASyntax
{
    public override GreenDefinitionSyntax Green { get; }

    public DefinitionSyntax(ISyntax? parent, int offset, GreenDefinitionSyntax green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    private TokenDef? _def;
    public TokenDef Def => _def ??= Green.Def.GetRed(this, GetOffsetAt(0));
    private TokenIdentifier? _name;
    public TokenIdentifier Name => _name ??= Green.Name.GetRed(this, GetOffsetAt(1));
    private TokenAssign? _assign;
    public TokenAssign Assign => _assign ??= Green.Assign.GetRed(this, GetOffsetAt(2));
    private IExpressionSyntax? _value;
    public IExpressionSyntax Value => _value ??= Green.Value.GetRed(this, GetOffsetAt(3));
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);

    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Def;
            yield return Name;
            yield return Assign;
            yield return Value;
        }
    }
}
public sealed class FileSyntax : ASyntax
{
    public override GreenFileSyntax Green { get; }

    public FileSyntax(ISyntax? parent, int offset, GreenFileSyntax green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    private ImmutableArray<DefinitionSyntax> _definitions;
    public ImmutableArray<DefinitionSyntax> Definitions
    {
        get
        {
            if (_definitions == null)
                _definitions = Green.Definitions.Select((g, i) => g.GetRed(this, GetOffsetAt(i))).ToImmutableArray();
            return _definitions;
        }
    }
    private TokenEndOfFile? _endOfFile;
    public TokenEndOfFile EndOfFile => _endOfFile ??= Green.EndOfFile.GetRed(this, GetOffsetAt(Green.Definitions.Length));

    public override IEnumerable<INode> Children => Definitions.Append<INode>(EndOfFile);
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
}
public sealed class TopLevelExpressionSyntax : ASyntax
{
    public TopLevelExpressionSyntax(ISyntax? parent, int offset, GreenTopLevelExpressionSyntax green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    private IExpressionSyntax? _expression;
    public IExpressionSyntax Expression => _expression ??= Green.Expression.GetRed(this, GetOffsetAt(0));
    public TokenEndOfFile? _endOfFile;
    public TokenEndOfFile EndOfFile => _endOfFile ??= Green.EndOfFile.GetRed(this, GetOffsetAt(1));
    public override GreenTopLevelExpressionSyntax Green { get; }
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Expression;
            yield return EndOfFile;
        }
    }

    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Accept(this);
}
#endregion
#endregion

public sealed class Project : IGreenSyntax, ISyntax
{
    #region Syntax
    private readonly GreenFileSyntax GreenFile;

    public int Length => GreenFile.Length;
    public string Generating => GreenFile.Generating;
    public IGreenToken LastToken => GreenFile.LastToken;
    public IGreenToken FirstToken => GreenFile.FirstToken;
    public ISyntax? Parent => null;
    public IGreenNode Green => this;
    public int Offset => 0;
    public int GetLengthAt(int index) => 0;

    private FileSyntax? _file;

    public FileSyntax File => (_file ??= new FileSyntax(this, Offset, GreenFile));
    IEnumerable<IGreenNode> IGreenSyntax.Children
    {
        get
        {
            yield return GreenFile;
        }
    }
    IEnumerable<INode> ISyntax.Children
    {
        get
        {
            yield return File;
        }
    }

    INode IGreenNode.GetRed(ISyntax? parent, int offset) => this;
    ISyntax IGreenSyntax.GetRed(ISyntax? parent, int offset) => this;

    public override string ToString() => Generating;
    public T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    #endregion

    private Project(GreenFileSyntax greenFile, ImmutableArray<IDiagnostic> parserDiagnostics)
    {
        GreenFile = greenFile ?? throw new ArgumentNullException(nameof(greenFile));
        _parserDiagnostics = parserDiagnostics;
    }

    private readonly ImmutableArray<IDiagnostic> _parserDiagnostics;
    public static readonly Project Empty = new(
        new GreenFileSyntax(ImmutableArray<GreenDefinitionSyntax>.Empty, GreenTokenEndOfFile.Instance),
        ImmutableArray<IDiagnostic>.Empty);
    public Project SetFileText(string text)
    {
        var parserDiagnostics = new DiagnosticsBag();
        var syntax = Parser.ParseFile(text, parserDiagnostics);
        return new Project(syntax, parserDiagnostics.ToImmutableArray());
    }

    // Compiler
    private readonly CompilerCache _compiler = new ();
    internal IStructural? UnsafeCompile(DefinitionSyntax definition)
    {
        if (HasErrors)
            return null;
        return _compiler.Compile(definition);
    }

    // Errors
    public bool HasErrors => GetAllErrors().Any();
    public IEnumerable<IDiagnostic> GetAllErrors() => GetAllErrorsBlocks().SelectMany(x => x);
    private IEnumerable<IEnumerable<IDiagnostic>> GetAllErrorsBlocks()
    {
        yield return _parserDiagnostics;
        yield return GetErrors(this);
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
        var leftCompiled = new CompiledLambda(_compiler.Compile(redLeftExpr.Expression).Evaluate());
        var rightCompiled = new CompiledLambda(_compiler.Compile(redRightExpr.Expression).Evaluate());
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
