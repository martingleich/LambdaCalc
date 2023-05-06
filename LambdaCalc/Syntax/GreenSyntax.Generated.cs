using System.Collections.Immutable;
namespace LambdaCalc.Syntax;
#nullable enable


public sealed partial class GreenCallExpression : AGreenExpressionSyntax
{
    public readonly IGreenExpressionSyntax Left;
	public readonly IGreenExpressionSyntax Right;

    public GreenCallExpression(IGreenExpressionSyntax _Left, IGreenExpressionSyntax _Right)
    {
        Left = _Left ?? throw new ArgumentNullException(nameof(_Left));
		Right = _Right ?? throw new ArgumentNullException(nameof(_Right));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Left;
			yield return Right;
        }
    }
    public override IGreenExpressionSyntax FirstChild => Left;
    public override IGreenExpressionSyntax LastChild => Right;

    public override CallExpression GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public sealed partial class GreenDefinitionSyntax : AGreenSyntax
{
    public readonly GreenTokenDef Def;
	public readonly GreenTokenIdentifier Name;
	public readonly GreenTokenAssign Assign;
	public readonly IGreenExpressionSyntax Value;

    public GreenDefinitionSyntax(GreenTokenDef _Def, GreenTokenIdentifier _Name, GreenTokenAssign _Assign, IGreenExpressionSyntax _Value)
    {
        Def = _Def ?? throw new ArgumentNullException(nameof(_Def));
		Name = _Name ?? throw new ArgumentNullException(nameof(_Name));
		Assign = _Assign ?? throw new ArgumentNullException(nameof(_Assign));
		Value = _Value ?? throw new ArgumentNullException(nameof(_Value));
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

    public DefinitionSyntax GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}

public sealed partial class GreenFileSyntax : AGreenSyntax
{
    public readonly ImmutableArray<GreenDefinitionSyntax> Definitions;
	public readonly GreenTokenEndOfFile EndOfFile;

    public GreenFileSyntax(ImmutableArray<GreenDefinitionSyntax> _Definitions, GreenTokenEndOfFile _EndOfFile)
    {
        Definitions = _Definitions;
		EndOfFile = _EndOfFile ?? throw new ArgumentNullException(nameof(_EndOfFile));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            foreach(var x in Definitions) yield return x;
			yield return EndOfFile;
        }
    }
    public override IGreenNode FirstChild => (Definitions.Length > 0 ? Definitions[0] : EndOfFile);
    public override GreenTokenEndOfFile LastChild => EndOfFile;

    public FileSyntax GetRed(Project project, int offset) => new(project, offset, this);
}

public sealed partial class GreenLambdaExpression : AGreenExpressionSyntax
{
    public readonly GreenTokenIdentifier ParameterName;
	public readonly GreenTokenDoubleRightArrow DoubleRightArrow;
	public readonly IGreenExpressionSyntax Expression;

    public GreenLambdaExpression(GreenTokenIdentifier _ParameterName, GreenTokenDoubleRightArrow _DoubleRightArrow, IGreenExpressionSyntax _Expression)
    {
        ParameterName = _ParameterName ?? throw new ArgumentNullException(nameof(_ParameterName));
		DoubleRightArrow = _DoubleRightArrow ?? throw new ArgumentNullException(nameof(_DoubleRightArrow));
		Expression = _Expression ?? throw new ArgumentNullException(nameof(_Expression));
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
    public override GreenTokenIdentifier FirstChild => ParameterName;
    public override IGreenExpressionSyntax LastChild => Expression;

    public override LambdaExpression GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public sealed partial class GreenListContent : AGreenSyntax
{
    public readonly IGreenListContentHead Head;
	public readonly ImmutableArray<GreenListContentTail> Rest;

    public GreenListContent(IGreenListContentHead _Head, ImmutableArray<GreenListContentTail> _Rest)
    {
        Head = _Head ?? throw new ArgumentNullException(nameof(_Head));
		Rest = _Rest;
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Head;
			foreach(var x in Rest) yield return x;
        }
    }
    public override IGreenListContentHead FirstChild => Head;
    public override IGreenNode LastChild => (Rest.Length > 0 ? Rest[^1] : Head);

    public ListContent GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}

public sealed partial class GreenListContentHeadAppend : AGreenSyntax
{
    public readonly IGreenExpressionSyntax Value;
	public readonly GreenTokenDots Dots;

    public GreenListContentHeadAppend(IGreenExpressionSyntax _Value, GreenTokenDots _Dots)
    {
        Value = _Value ?? throw new ArgumentNullException(nameof(_Value));
		Dots = _Dots ?? throw new ArgumentNullException(nameof(_Dots));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Value;
			yield return Dots;
        }
    }
    public override IGreenExpressionSyntax FirstChild => Value;
    public override GreenTokenDots LastChild => Dots;

    public ListContentHeadAppend GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}

public sealed partial class GreenListContentHeadValue : AGreenSyntax
{
    public readonly IGreenExpressionSyntax Value;

    public GreenListContentHeadValue(IGreenExpressionSyntax _Value)
    {
        Value = _Value ?? throw new ArgumentNullException(nameof(_Value));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Value;
        }
    }
    public override IGreenExpressionSyntax FirstChild => Value;
    public override IGreenExpressionSyntax LastChild => Value;

    public ListContentHeadValue GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}

public sealed partial class GreenListContentTail : AGreenSyntax
{
    public readonly GreenTokenComma Comma;
	public readonly IGreenExpressionSyntax Value;

    public GreenListContentTail(GreenTokenComma _Comma, IGreenExpressionSyntax _Value)
    {
        Comma = _Comma ?? throw new ArgumentNullException(nameof(_Comma));
		Value = _Value ?? throw new ArgumentNullException(nameof(_Value));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Comma;
			yield return Value;
        }
    }
    public override GreenTokenComma FirstChild => Comma;
    public override IGreenExpressionSyntax LastChild => Value;

    public ListContentTail GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}

public sealed partial class GreenListExpression : AGreenExpressionSyntax
{
    public readonly GreenTokenBracketOpen BracketOpen;
	public readonly GreenListContent? Content;
	public readonly GreenTokenBracketClose BracketClose;

    public GreenListExpression(GreenTokenBracketOpen _BracketOpen, GreenListContent? _Content, GreenTokenBracketClose _BracketClose)
    {
        BracketOpen = _BracketOpen ?? throw new ArgumentNullException(nameof(_BracketOpen));
		Content = _Content;
		BracketClose = _BracketClose ?? throw new ArgumentNullException(nameof(_BracketClose));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return BracketOpen;
			if(Content is not null) yield return Content;
			yield return BracketClose;
        }
    }
    public override GreenTokenBracketOpen FirstChild => BracketOpen;
    public override GreenTokenBracketClose LastChild => BracketClose;

    public override ListExpression GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public sealed partial class GreenParenthesisExpression : AGreenExpressionSyntax
{
    public readonly GreenTokenParenthesisOpen ParenthesisOpen;
	public readonly IGreenExpressionSyntax Expression;
	public readonly GreenTokenParenthesisClose ParenthesisClose;

    public GreenParenthesisExpression(GreenTokenParenthesisOpen _ParenthesisOpen, IGreenExpressionSyntax _Expression, GreenTokenParenthesisClose _ParenthesisClose)
    {
        ParenthesisOpen = _ParenthesisOpen ?? throw new ArgumentNullException(nameof(_ParenthesisOpen));
		Expression = _Expression ?? throw new ArgumentNullException(nameof(_Expression));
		ParenthesisClose = _ParenthesisClose ?? throw new ArgumentNullException(nameof(_ParenthesisClose));
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
    public override GreenTokenParenthesisOpen FirstChild => ParenthesisOpen;
    public override GreenTokenParenthesisClose LastChild => ParenthesisClose;

    public override ParenthesisExpression GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public sealed partial class GreenTopLevelExpressionSyntax : AGreenSyntax
{
    public readonly IGreenExpressionSyntax Expression;
	public readonly GreenTokenEndOfFile EndOfFile;

    public GreenTopLevelExpressionSyntax(IGreenExpressionSyntax _Expression, GreenTokenEndOfFile _EndOfFile)
    {
        Expression = _Expression ?? throw new ArgumentNullException(nameof(_Expression));
		EndOfFile = _EndOfFile ?? throw new ArgumentNullException(nameof(_EndOfFile));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Expression;
			yield return EndOfFile;
        }
    }
    public override IGreenExpressionSyntax FirstChild => Expression;
    public override GreenTokenEndOfFile LastChild => EndOfFile;

    public TopLevelExpressionSyntax GetRed(Project project, int offset) => new(project, offset, this);
}

public sealed partial class GreenVariableExpression : AGreenExpressionSyntax
{
    public readonly GreenTokenIdentifier Identifier;

    public GreenVariableExpression(GreenTokenIdentifier _Identifier)
    {
        Identifier = _Identifier ?? throw new ArgumentNullException(nameof(_Identifier));
    }

    public override IEnumerable<IGreenNode> Children
    {
        get
        {
            yield return Identifier;
        }
    }
    public override GreenTokenIdentifier FirstChild => Identifier;
    public override GreenTokenIdentifier LastChild => Identifier;

    public override VariableExpression GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}
