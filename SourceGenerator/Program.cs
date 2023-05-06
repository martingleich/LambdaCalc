using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace SourceGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tokens = new List<FixedToken>();
            var typeTokenParenthesisOpen = tokens.AddAndGet(new("(", "TokenParenthesisOpen")).NodeType;
            var typeTokenParenthesisClose = tokens.AddAndGet(new(")", "TokenParenthesisClose")).NodeType;
            var typeTokenBracketOpen = tokens.AddAndGet(new("[", "TokenBracketOpen")).NodeType;
            var typeTokenBracketClose = tokens.AddAndGet(new("]", "TokenBracketClose")).NodeType;
            var typeTokenDots = tokens.AddAndGet(new("..", "TokenDots")).NodeType;
            var typeTokenComma = tokens.AddAndGet(new(",", "TokenComma")).NodeType;
            var typeTokenDoubleRightArrow = tokens.AddAndGet(new("=>", "TokenDoubleRightArrow")).NodeType;
            var typeTokenDef = tokens.AddAndGet(new("def", "TokenDef")).NodeType;
            var typeTokenAssign = tokens.AddAndGet(new("=", "TokenAssign")).NodeType;
            var typeTokenEndOfFile = tokens.AddAndGet(new("", "TokenEndOfFile")).NodeType;
            var typeTokenIdentifier = new NodeType("TokenIdentifier", "GreenTokenIdentifier");

            WriteTokens(args, tokens);

            var typeExpressionSyntax = new NodeType("IExpressionSyntax", "IGreenExpressionSyntax");
            var typeListContentHead = new NodeType("IListContentHead", "IGreenListContentHead");
            var syntax = new List<Syntax>();
            var typeParenthesisExpression = syntax.AddAndGet(new Syntax("ParenthesisExpression", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("ParenthesisOpen", typeTokenParenthesisOpen),
                new SyntaxMemberNormal("Expression", typeExpressionSyntax),
                new SyntaxMemberNormal("ParenthesisClose", typeTokenParenthesisClose)), true)).NodeType;
            var typeListContentHeadAppend = syntax.AddAndGet(new Syntax("ListContentHeadAppend", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("Value", typeExpressionSyntax),
                new SyntaxMemberNormal("Dots", typeTokenDots)), false)).NodeType;
            var typeListContentHeadValue = syntax.AddAndGet(new Syntax("ListContentHeadValue", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("Value", typeExpressionSyntax)), false)).NodeType;
            var typeListContentTail = syntax.AddAndGet(new Syntax("ListContentTail", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("Comma", typeTokenComma),
                new SyntaxMemberNormal("Value", typeExpressionSyntax)), false)).NodeType;
            var typeListContent = syntax.AddAndGet(new Syntax("ListContent", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("Head", typeListContentHead),
                new SyntaxMemberArray("Rest", typeListContentTail)), false)).NodeType;
            var typeListExpression = syntax.AddAndGet(new Syntax("ListExpression", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("BracketOpen", typeTokenBracketOpen),
                new SyntaxMemberNullable("Content", typeListContent),
                new SyntaxMemberNormal("BracketClose", typeTokenBracketClose)), true)).NodeType;
            var typeDefinitionSyntax = syntax.AddAndGet(new Syntax("DefinitionSyntax", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("Def", typeTokenDef),
                new SyntaxMemberNormal("Name", typeTokenIdentifier),
                new SyntaxMemberNormal("Assign", typeTokenAssign),
                new SyntaxMemberNormal("Value", typeExpressionSyntax)), false)).NodeType;
            var typeFileSyntax = syntax.AddAndGet(new Syntax("FileSyntax", MyList.New<SyntaxMember>(
                new SyntaxMemberArray("Definitions", typeDefinitionSyntax),
                new SyntaxMemberNormal("EndOfFile", typeTokenEndOfFile)), false, true)).NodeType;
            var typeLambdaExpression = syntax.AddAndGet(new Syntax("LambdaExpression", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("ParameterName", typeTokenIdentifier),
                new SyntaxMemberNormal("DoubleRightArrow", typeTokenDoubleRightArrow),
                new SyntaxMemberNormal("Expression", typeExpressionSyntax)), true)).NodeType;
            var typeCallExpression = syntax.AddAndGet(new Syntax("CallExpression", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("Left", typeExpressionSyntax),
                new SyntaxMemberNormal("Right", typeExpressionSyntax)), true)).NodeType;
            var typeVariableExpression = syntax.AddAndGet(new Syntax("VariableExpression", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("Identifier", typeTokenIdentifier)), true)).NodeType;
            var typeTopLevelExpression = syntax.AddAndGet(new Syntax("TopLevelExpressionSyntax", MyList.New<SyntaxMember>(
                new SyntaxMemberNormal("Expression", typeExpressionSyntax),
                new SyntaxMemberNormal("EndOfFile", typeTokenEndOfFile)), false, true)).NodeType;
            WriteSyntax(args, syntax);
        }

        private static void WriteSyntax(string[] args, IEnumerable<Syntax> syntax)
        {
            using var greenFile = System.IO.File.Open(System.IO.Path.Combine(args[0], "GreenSyntax.Generated.cs"), FileMode.Create, FileAccess.Write);
            using var redFile = System.IO.File.Open(System.IO.Path.Combine(args[0], "Syntax.Generated.cs"), FileMode.Create, FileAccess.Write);
            using var textGreen = new StreamWriter(greenFile);
            using var textRed = new StreamWriter(redFile);
            textGreen.WriteLine("using System.Collections.Immutable;");
            textGreen.WriteLine("namespace LambdaCalc.Syntax;");
            textGreen.WriteLine("#nullable enable");
            textGreen.WriteLine();
            textRed.WriteLine("using System.Collections.Immutable;");
            textRed.WriteLine("namespace LambdaCalc.Syntax;");
            textRed.WriteLine("#nullable enable");
            textRed.WriteLine();
            foreach (var syn in syntax.OrderBy(x => x.Name))
            {
                textGreen.WriteLine(syn.WriteGreen());
                textRed.WriteLine(syn.WriteRed());
            }
        }
        private static void WriteTokens(string[] args, IEnumerable<FixedToken> tokens)
        {
            using var greenFile = System.IO.File.Open(System.IO.Path.Combine(args[0], "GreenToken.Generated.cs"), FileMode.Create, FileAccess.Write);
            using var redFile = System.IO.File.Open(System.IO.Path.Combine(args[0], "Token.Generated.cs"), FileMode.Create, FileAccess.Write);
            using var textGreen = new StreamWriter(greenFile);
            using var textRed = new StreamWriter(redFile);
            textGreen.WriteLine("namespace LambdaCalc.Syntax;");
            textGreen.WriteLine("#nullable enable");
            textGreen.WriteLine();
            textRed.WriteLine("namespace LambdaCalc.Syntax;");
            textRed.WriteLine("#nullable enable");
            textRed.WriteLine();
            foreach (var token in tokens.OrderBy(x => x.Name))
            {
                textGreen.WriteLine(token.WriteGreen());
                textRed.WriteLine(token.WriteRed());
            }
        }
    }

    record class FixedToken(string Generating, string Name)
    {
        public string WriteGreen()
        {
            return $@"public sealed partial class {GreenName} : AGreenToken
{{
    public {GreenName}(IGreenToken? leadingWhitespace) : base(leadingWhitespace) {{ }}
    public static readonly {GreenName} Instance = new (default);
    public static {GreenName} New(IGreenToken? leadingWhitespace) => new (leadingWhitespace);
    public static readonly string FixedGenerating = ""{Generating}"";
    protected override string OnlyGenerating => FixedGenerating;
    public override {RedName} GetRed(ISyntax? parent, int offset) => new(parent, offset, this);
}}";
        }

        public string GreenName => $"Green{Name}";
        public string RedName => $"{Name}";
        public NodeType NodeType => new (RedName, GreenName);
        public string WriteRed()
        {
            return $@"public sealed partial class {RedName} : AToken
{{
    public {RedName}(ISyntax? parent, int offset, {GreenName} green) : base(parent, offset)
    {{
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }}
    public override {GreenName} Green {{ get; }}
}}";
        }
    }

    public readonly struct NodeType : IEquatable<NodeType>
    {
        public readonly string Red;
        public readonly string Green;

        public NodeType(string name) : this(name, name)
        {
        }
        public NodeType(string red, string green)
        {
            Red = red ?? throw new ArgumentNullException(nameof(red));
            Green = green ?? throw new ArgumentNullException(nameof(green));
        }

        public static NodeType GetCommonType(NodeType a, NodeType b) =>
            a == b
            ? a
            : new NodeType("INode", "IGreenNode");

        public override bool Equals(object? obj)
        {
            return obj is NodeType type && Equals(type);
        }

        public bool Equals(NodeType other)
        {
            return Red == other.Red &&
                   Green == other.Green;
        }

        public static bool operator ==(NodeType left, NodeType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NodeType left, NodeType right)
        {
            return !(left == right);
        }

        public override int GetHashCode() => HashCode.Combine(Red, Green);
    }
    static class MyList
    {
        public static MyList<T> New<T>(params T[] args) where T : notnull => new (args.ToImmutableArray(), 0);
    }
    readonly struct MyList<T> : IEnumerable<T> where T : notnull
    {
        private readonly ImmutableArray<T> _values;
        private readonly int _cursor;

        public MyList(ImmutableArray<T> values, int cursor)
        {
            _values = values;
            _cursor = cursor;
        }

        public MyList<T> Take(int count)
        {
            var builder = ImmutableArray.CreateBuilder<T>();
            for (int i = 0; i <= count; ++i)
                builder.Add(_values[i]);
            return new MyList<T>(builder.ToImmutable(), 0);
        }
        public bool Take([NotNullWhen(true)] out T? value, out MyList<T> remainder)
        {
            if (_cursor < _values.Length)
            {
                value = _values[_cursor];
                remainder = new MyList<T>(_values, _cursor + 1);
                return true;
            }
            else
            {
                value = default;
                remainder = default;
                return false;
            }
        }
        public MyList<T> Reverse()
        {
            var builder = ImmutableArray.CreateBuilder<T>();
            for (int i = _values.Length - 1; i >= _cursor; --i)
                builder.Add(_values[i]);
            return new MyList<T>(builder.ToImmutable(), 0);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = _cursor; i < _values.Length; ++i)
                yield return _values[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Length => _values.Length - _cursor;
    }

    readonly struct FirstLastResult
    {
        public static FirstLastResult FirstFromList(MyList<SyntaxMember> members)
        {
            if (members.Take(out var first, out var successors))
                return first.First(successors);
            throw new InvalidOperationException();
        }
        public static FirstLastResult LastFromList(MyList<SyntaxMember> members)
        {
            if (members.Reverse().Take(out var first, out var predecessors))
                return first.Last(predecessors);
            throw new InvalidOperationException();
        }
        public readonly NodeType Type;
        public readonly string Computation;

        public FirstLastResult(NodeType type, string computation)
        {
            Type = type;
            Computation = computation ?? throw new ArgumentNullException(nameof(computation));
        }
    }
    abstract record class SyntaxMember(string Name, NodeType Type)
    {
        public abstract string GreenDeclaration { get; }
        public abstract string GreenConstructorArgument { get; }
        public abstract string ChildrenStatement { get; }
        public abstract string GreenConstructorAssignemnt { get; }
        public abstract string RedNumChildren { get; }
        public abstract string RedDeclaration(MyList<SyntaxMember> predecessors);
        public abstract FirstLastResult First(MyList<SyntaxMember> successors);
        public abstract FirstLastResult Last(MyList<SyntaxMember> predecessors);
        public string RedType => Type.Red;
        public string GreenType => Type.Green;

        public static string TotalNumOfChildren(MyList<SyntaxMember> nodes)
        {
            if (nodes.Length == 0)
                return "0";
            else
                return string.Join(" + ", nodes.Select(p => p.RedNumChildren));
        }
    }

    sealed record class SyntaxMemberNormal : SyntaxMember
    {
        public override string GreenDeclaration => $"public readonly {GreenType} {Name};";
        public override string GreenConstructorArgument => $"{GreenType} _{Name}";
        public override string GreenConstructorAssignemnt => $"{Name} = _{Name} ?? throw new ArgumentNullException(nameof(_{Name}));";
        public override string ChildrenStatement => $"yield return {Name};";
        public override string RedDeclaration(MyList<SyntaxMember> predecessors) => @$"
    private {RedType}? _{Name};
    public {RedType} {Name} => _{Name} ??= Green.{Name}.GetRed(this, GetOffsetAt({TotalNumOfChildren(predecessors)}));";

        public override FirstLastResult First(MyList<SyntaxMember> remainder) => new (Type, Name);
        public override FirstLastResult Last(MyList<SyntaxMember> predecessors) => new (Type, Name);
        public override string RedNumChildren => "1";

        public SyntaxMemberNormal(string Name, NodeType Type) : base(Name, Type)
        {
        }
    }
    sealed record class SyntaxMemberNullable : SyntaxMember
    {
        public override string GreenDeclaration => $"public readonly {GreenType}? {Name};";
        public override string GreenConstructorArgument => $"{GreenType}? _{Name}";
        public override string GreenConstructorAssignemnt => $"{Name} = _{Name};";
        public override string ChildrenStatement => $"if({Name} is not null) yield return {Name};";
        public override string RedDeclaration(MyList<SyntaxMember> predecessors) => @$"
    private {RedType}? _{Name};
    public {RedType}? {Name} => Green.{Name} is {{}} greenValue ? (_{Name} ??= greenValue.GetRed(this, GetOffsetAt({TotalNumOfChildren(predecessors)}))) : null;";
        public override FirstLastResult First(MyList<SyntaxMember> remainder)
        {
            if (remainder.Take(out var value, out var remainder2))
            {
                var x = value.First(remainder2);
                return new FirstLastResult(NodeType.GetCommonType(Type, x.Type), $"{Name} ?? {x.Computation}");
            }
            throw new InvalidOperationException();
        }
        public override FirstLastResult Last(MyList<SyntaxMember> predecessors)
        {
            if (predecessors.Take(out var value, out var predecessors2))
            {
                var x = value.Last(predecessors2);
                return new FirstLastResult(NodeType.GetCommonType(Type, x.Type), $"{Name} ?? {x.Computation}");
            }
            throw new InvalidOperationException();
        }

        public override string RedNumChildren => $"(Green.{Name} != null ? 1 : 0)";

        public SyntaxMemberNullable(string Name, NodeType Type) : base(Name, Type)
        {
        }
    }
    sealed record class SyntaxMemberArray : SyntaxMember
    {
        public override string GreenDeclaration => $"public readonly ImmutableArray<{GreenType}> {Name};";
        public override string GreenConstructorArgument => $"ImmutableArray<{GreenType}> _{Name}";
        public override string GreenConstructorAssignemnt => $"{Name} = _{Name};";
        public override string ChildrenStatement => $"foreach(var x in {Name}) yield return x;";
        public override string RedDeclaration(MyList<SyntaxMember> predecessors) => @$"
    private ImmutableArray<{RedType}>? _{Name};
    public ImmutableArray<{RedType}> {Name} => _{Name} ??= Green.{Name}.Select((g, i) => g.GetRed(this, GetOffsetAt({TotalNumOfChildren(predecessors)} + i))).ToImmutableArray();";
        public override FirstLastResult First(MyList<SyntaxMember> remainder)
        {
            if (remainder.Take(out var value, out var remainder2))
            {
                var x = value.First(remainder2);
                return new(NodeType.GetCommonType(x.Type, Type), $"({Name}.Length > 0 ? {Name}[0] : {x.Computation})");
            }
            throw new InvalidOperationException();
        }
        public override FirstLastResult Last(MyList<SyntaxMember> predecessors)
        {
            if (predecessors.Take(out var value, out var predecessors2))
            {
                var x = value.Last(predecessors2);
                return new(NodeType.GetCommonType(x.Type, Type), $"({Name}.Length > 0 ? {Name}[^1] : {x.Computation})");
            }
            throw new InvalidOperationException();
        }
        public override string RedNumChildren => $"Green.{Name}.Length";

        public SyntaxMemberArray(string Name, NodeType Type) : base(Name, Type)
        {
        }
    }
    public static class ListExt
    {
        public static T AddAndGet<T>(this IList<T> self, T value)
        {
            self.Add(value);
            return value;
        }
    }
    record class Syntax(string Name, MyList<SyntaxMember> Members, bool ExpressionSyntax, bool IsRoot=false)
    {
        public NodeType NodeType => new(RedName, GreenName);
        string GreenName => $"Green{Name}";
        string RedName => Name;
        private static string IndentedLines<T>(int depth, IEnumerable<T> lines) => string.Join($"{Environment.NewLine}{new string('\t', depth)}", lines);
        private static string CommaSeperated<T>(IEnumerable<T> lines) => string.Join($", ", lines);
        public string WriteGreen()
        {
            string baseClass = ExpressionSyntax ? "AGreenExpressionSyntax" : "AGreenSyntax";
            string expressionSyntax = ExpressionSyntax
                ? $@"
    public override T Accept<T>(IGreenExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public override T Accept<T, TContext>(IGreenExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);"
    : "";
            var first = FirstLastResult.FirstFromList(Members);
            var last = FirstLastResult.LastFromList(Members);
            string getRed;
            if (ExpressionSyntax)
                getRed = $"public override {RedName} GetRed(ISyntax? parent, int offset) => new(parent, offset, this);";
            else if (IsRoot)
                getRed = $"public {RedName} GetRed(Project project, int offset) => new(project, offset, this);";
            else
                getRed = $"public {RedName} GetRed(ISyntax? parent, int offset) => new(parent, offset, this);";
            return @$"
public sealed partial class {GreenName} : {baseClass}
{{
    {IndentedLines(1, Members.Select(m => m.GreenDeclaration))}

    public {GreenName}({CommaSeperated(Members.Select(m => m.GreenConstructorArgument))})
    {{
        {IndentedLines(2, Members.Select(m => m.GreenConstructorAssignemnt))}
    }}

    public override IEnumerable<IGreenNode> Children
    {{
        get
        {{
            {IndentedLines(3, Members.Select(m => m.ChildrenStatement))}
        }}
    }}
    public override {first.Type.Green} FirstChild => {first.Computation};
    public override {last.Type.Green} LastChild => {last.Computation};

    {getRed}{expressionSyntax}
}}";

        }
        public string WriteRed()
        {
            string baseClass = ExpressionSyntax ? "ASyntax, IExpressionSyntax" : (IsRoot ? "ASyntax, IRootSyntax" : "ASyntax");
            string expressionSyntax = ExpressionSyntax
                ? $@"
    public T Accept<T>(IExpressionSyntax.IVisitor<T> visitor) => visitor.Visit(this);
    public T Accept<T, TContext>(IExpressionSyntax.IVisitor<T, TContext> visitor, TContext context) => visitor.Visit(this, context);"
    : "";
            string parentType = IsRoot ? "Project" : "ISyntax?";
            string constructor = IsRoot ?
    $@"public Project Project {{ get; }}
    public {RedName}(Project project, int offset, {GreenName} green) : base(null, offset)
    {{
        Green = green ?? throw new ArgumentNullException(nameof(green));
        Project = project ?? throw new ArgumentNullException(nameof(project));
    }}" : $@"public {RedName}(ISyntax? parent, int offset, {GreenName} green) : base(parent, offset)
    {{
        Green = green ?? throw new ArgumentNullException(nameof(green));
    }}";
            return @$"
public sealed partial class {RedName} : {baseClass}
{{
    public override {GreenName} Green {{ get; }}
    
    {constructor}

    {IndentedLines(1, Members.Select((m, i) => m.RedDeclaration(Members.Take(i - 1).Reverse())))}
    
    public override IEnumerable<INode> Children
    {{
        get
        {{
            {IndentedLines(3, Members.Select(m => m.ChildrenStatement))}
        }}
    }}
    public override T Accept<T>(ISyntax.IVisitor<T> visitor) => visitor.Visit(this);{expressionSyntax}
}}";
        }
    }
}