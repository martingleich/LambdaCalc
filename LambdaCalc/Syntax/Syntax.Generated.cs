using System.Collections.Immutable;
namespace LambdaCalc.Syntax;
#nullable enable


public sealed partial class CallExpression : ASyntax, IExpressionSyntax
{
    public override GreenCallExpression Green { get; }
    
    public CallExpression(ISyntax? parent, int offset, GreenCallExpression green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private IExpressionSyntax? _Left;
    public IExpressionSyntax Left => _Left ??= Green.Left.GetRed(this, GetOffsetAt(0));
	
    private IExpressionSyntax? _Right;
    public IExpressionSyntax Right => _Right ??= Green.Right.GetRed(this, GetOffsetAt(1));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Left;
			yield return Right;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public sealed partial class DefinitionSyntax : ASyntax
{
    public override GreenDefinitionSyntax Green { get; }
    
    public DefinitionSyntax(ISyntax? parent, int offset, GreenDefinitionSyntax green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private TokenDef? _Def;
    public TokenDef Def => _Def ??= Green.Def.GetRed(this, GetOffsetAt(0));
	
    private TokenIdentifier? _Name;
    public TokenIdentifier Name => _Name ??= Green.Name.GetRed(this, GetOffsetAt(1));
	
    private TokenAssign? _Assign;
    public TokenAssign Assign => _Assign ??= Green.Assign.GetRed(this, GetOffsetAt(1 + 1));
	
    private IExpressionSyntax? _Value;
    public IExpressionSyntax Value => _Value ??= Green.Value.GetRed(this, GetOffsetAt(1 + 1 + 1));
    
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
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
}

public sealed partial class FileSyntax : ASyntax, IRootSyntax
{
    public override GreenFileSyntax Green { get; }
    
    public Project Project { get; }
    public FileSyntax(Project project, int offset, GreenFileSyntax green) : base(null, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
        Project = project ?? throw new ArgumentNullException(nameof(project));
    }

    
    private ImmutableArray<DefinitionSyntax>? _Definitions;
    public ImmutableArray<DefinitionSyntax> Definitions => _Definitions ??= Green.Definitions.Select((g, i) => g.GetRed(this, GetOffsetAt(0 + i))).ToImmutableArray();
	
    private TokenEndOfFile? _EndOfFile;
    public TokenEndOfFile EndOfFile => _EndOfFile ??= Green.EndOfFile.GetRed(this, GetOffsetAt(Green.Definitions.Length));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            foreach(var x in Definitions) yield return x;
			yield return EndOfFile;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
}

public sealed partial class LambdaExpression : ASyntax, IExpressionSyntax
{
    public override GreenLambdaExpression Green { get; }
    
    public LambdaExpression(ISyntax? parent, int offset, GreenLambdaExpression green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private TokenIdentifier? _ParameterName;
    public TokenIdentifier ParameterName => _ParameterName ??= Green.ParameterName.GetRed(this, GetOffsetAt(0));
	
    private TokenDoubleRightArrow? _DoubleRightArrow;
    public TokenDoubleRightArrow DoubleRightArrow => _DoubleRightArrow ??= Green.DoubleRightArrow.GetRed(this, GetOffsetAt(1));
	
    private IExpressionSyntax? _Expression;
    public IExpressionSyntax Expression => _Expression ??= Green.Expression.GetRed(this, GetOffsetAt(1 + 1));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return ParameterName;
			yield return DoubleRightArrow;
			yield return Expression;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public sealed partial class ListContent : ASyntax
{
    public override GreenListContent Green { get; }
    
    public ListContent(ISyntax? parent, int offset, GreenListContent green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private IListContentHead? _Head;
    public IListContentHead Head => _Head ??= Green.Head.GetRed(this, GetOffsetAt(0));
	
    private ImmutableArray<ListContentTail>? _Rest;
    public ImmutableArray<ListContentTail> Rest => _Rest ??= Green.Rest.Select((g, i) => g.GetRed(this, GetOffsetAt(1 + i))).ToImmutableArray();
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Head;
			foreach(var x in Rest) yield return x;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
}

public sealed partial class ListContentHeadAppend : ASyntax
{
    public override GreenListContentHeadAppend Green { get; }
    
    public ListContentHeadAppend(ISyntax? parent, int offset, GreenListContentHeadAppend green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private IExpressionSyntax? _Value;
    public IExpressionSyntax Value => _Value ??= Green.Value.GetRed(this, GetOffsetAt(0));
	
    private TokenDots? _Dots;
    public TokenDots Dots => _Dots ??= Green.Dots.GetRed(this, GetOffsetAt(1));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Value;
			yield return Dots;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
}

public sealed partial class ListContentHeadValue : ASyntax
{
    public override GreenListContentHeadValue Green { get; }
    
    public ListContentHeadValue(ISyntax? parent, int offset, GreenListContentHeadValue green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private IExpressionSyntax? _Value;
    public IExpressionSyntax Value => _Value ??= Green.Value.GetRed(this, GetOffsetAt(0));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Value;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
}

public sealed partial class ListContentTail : ASyntax
{
    public override GreenListContentTail Green { get; }
    
    public ListContentTail(ISyntax? parent, int offset, GreenListContentTail green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private TokenComma? _Comma;
    public TokenComma Comma => _Comma ??= Green.Comma.GetRed(this, GetOffsetAt(0));
	
    private IExpressionSyntax? _Value;
    public IExpressionSyntax Value => _Value ??= Green.Value.GetRed(this, GetOffsetAt(1));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Comma;
			yield return Value;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
}

public sealed partial class ListExpression : ASyntax, IExpressionSyntax
{
    public override GreenListExpression Green { get; }
    
    public ListExpression(ISyntax? parent, int offset, GreenListExpression green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private TokenBracketOpen? _BracketOpen;
    public TokenBracketOpen BracketOpen => _BracketOpen ??= Green.BracketOpen.GetRed(this, GetOffsetAt(0));
	
    private ListContent? _Content;
    public ListContent? Content => Green.Content is {} greenValue ? (_Content ??= greenValue.GetRed(this, GetOffsetAt(1))) : null;
	
    private TokenBracketClose? _BracketClose;
    public TokenBracketClose BracketClose => _BracketClose ??= Green.BracketClose.GetRed(this, GetOffsetAt((Green.Content != null ? 1 : 0) + 1));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return BracketOpen;
			if(Content is not null) yield return Content;
			yield return BracketClose;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public sealed partial class ParenthesisExpression : ASyntax, IExpressionSyntax
{
    public override GreenParenthesisExpression Green { get; }
    
    public ParenthesisExpression(ISyntax? parent, int offset, GreenParenthesisExpression green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private TokenParenthesisOpen? _ParenthesisOpen;
    public TokenParenthesisOpen ParenthesisOpen => _ParenthesisOpen ??= Green.ParenthesisOpen.GetRed(this, GetOffsetAt(0));
	
    private IExpressionSyntax? _Expression;
    public IExpressionSyntax Expression => _Expression ??= Green.Expression.GetRed(this, GetOffsetAt(1));
	
    private TokenParenthesisClose? _ParenthesisClose;
    public TokenParenthesisClose ParenthesisClose => _ParenthesisClose ??= Green.ParenthesisClose.GetRed(this, GetOffsetAt(1 + 1));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return ParenthesisOpen;
			yield return Expression;
			yield return ParenthesisClose;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}

public sealed partial class TopLevelExpressionSyntax : ASyntax, IRootSyntax
{
    public override GreenTopLevelExpressionSyntax Green { get; }
    
    public Project Project { get; }
    public TopLevelExpressionSyntax(Project project, int offset, GreenTopLevelExpressionSyntax green) : base(null, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
        Project = project ?? throw new ArgumentNullException(nameof(project));
    }

    
    private IExpressionSyntax? _Expression;
    public IExpressionSyntax Expression => _Expression ??= Green.Expression.GetRed(this, GetOffsetAt(0));
	
    private TokenEndOfFile? _EndOfFile;
    public TokenEndOfFile EndOfFile => _EndOfFile ??= Green.EndOfFile.GetRed(this, GetOffsetAt(1));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Expression;
			yield return EndOfFile;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
}

public sealed partial class VariableExpression : ASyntax, IExpressionSyntax
{
    public override GreenVariableExpression Green { get; }
    
    public VariableExpression(ISyntax? parent, int offset, GreenVariableExpression green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }

    
    private TokenIdentifier? _Identifier;
    public TokenIdentifier Identifier => _Identifier ??= Green.Identifier.GetRed(this, GetOffsetAt(0));
    
    public override IEnumerable<INode> Children
    {
        get
        {
            yield return Identifier;
        }
    }
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);
}
