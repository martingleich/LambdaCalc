namespace LambdaCalc.Syntax;
#nullable enable

public sealed partial class TokenAssign : AToken
{
    public TokenAssign(ISyntax? parent, int offset, GreenTokenAssign green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenAssign Green { get; }
}
public sealed partial class TokenBracketClose : AToken
{
    public TokenBracketClose(ISyntax? parent, int offset, GreenTokenBracketClose green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenBracketClose Green { get; }
}
public sealed partial class TokenBracketOpen : AToken
{
    public TokenBracketOpen(ISyntax? parent, int offset, GreenTokenBracketOpen green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenBracketOpen Green { get; }
}
public sealed partial class TokenComma : AToken
{
    public TokenComma(ISyntax? parent, int offset, GreenTokenComma green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenComma Green { get; }
}
public sealed partial class TokenDef : AToken
{
    public TokenDef(ISyntax? parent, int offset, GreenTokenDef green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenDef Green { get; }
}
public sealed partial class TokenDots : AToken
{
    public TokenDots(ISyntax? parent, int offset, GreenTokenDots green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenDots Green { get; }
}
public sealed partial class TokenDoubleRightArrow : AToken
{
    public TokenDoubleRightArrow(ISyntax? parent, int offset, GreenTokenDoubleRightArrow green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenDoubleRightArrow Green { get; }
}
public sealed partial class TokenEndOfFile : AToken
{
    public TokenEndOfFile(ISyntax? parent, int offset, GreenTokenEndOfFile green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenEndOfFile Green { get; }
}
public sealed partial class TokenParenthesisClose : AToken
{
    public TokenParenthesisClose(ISyntax? parent, int offset, GreenTokenParenthesisClose green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenParenthesisClose Green { get; }
}
public sealed partial class TokenParenthesisOpen : AToken
{
    public TokenParenthesisOpen(ISyntax? parent, int offset, GreenTokenParenthesisOpen green) : base(parent, offset)
    {
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }
    public override GreenTokenParenthesisOpen Green { get; }
}
