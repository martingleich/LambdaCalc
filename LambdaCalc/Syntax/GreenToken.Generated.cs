namespace LambdaCalc.Syntax;
#nullable enable

public sealed partial class GreenTokenAssign : AGreenToken
{
    public GreenTokenAssign(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenAssign Instance = new (default);
    public static GreenTokenAssign New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = "=";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenAssign GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed partial class GreenTokenBracketClose : AGreenToken
{
    public GreenTokenBracketClose(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenBracketClose Instance = new (default);
    public static GreenTokenBracketClose New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = "]";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenBracketClose GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed partial class GreenTokenBracketOpen : AGreenToken
{
    public GreenTokenBracketOpen(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenBracketOpen Instance = new (default);
    public static GreenTokenBracketOpen New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = "[";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenBracketOpen GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed partial class GreenTokenComma : AGreenToken
{
    public GreenTokenComma(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenComma Instance = new (default);
    public static GreenTokenComma New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = ",";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenComma GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed partial class GreenTokenDef : AGreenToken
{
    public GreenTokenDef(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenDef Instance = new (default);
    public static GreenTokenDef New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = "def";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenDef GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed partial class GreenTokenDots : AGreenToken
{
    public GreenTokenDots(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenDots Instance = new (default);
    public static GreenTokenDots New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = "..";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenDots GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed partial class GreenTokenDoubleRightArrow : AGreenToken
{
    public GreenTokenDoubleRightArrow(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenDoubleRightArrow Instance = new (default);
    public static GreenTokenDoubleRightArrow New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = "=>";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenDoubleRightArrow GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed partial class GreenTokenEndOfFile : AGreenToken
{
    public GreenTokenEndOfFile(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenEndOfFile Instance = new (default);
    public static GreenTokenEndOfFile New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = "";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenEndOfFile GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed partial class GreenTokenParenthesisClose : AGreenToken
{
    public GreenTokenParenthesisClose(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenParenthesisClose Instance = new (default);
    public static GreenTokenParenthesisClose New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = ")";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenParenthesisClose GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
public sealed partial class GreenTokenParenthesisOpen : AGreenToken
{
    public GreenTokenParenthesisOpen(IGreenToken? leadingWhitespace) : base(leadingWhitespace) { }
    public static readonly GreenTokenParenthesisOpen Instance = new (default);
    public static GreenTokenParenthesisOpen New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = "(";
    protected override string OnlyGenerating => FixedGenerating;
    public override TokenParenthesisOpen GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}
