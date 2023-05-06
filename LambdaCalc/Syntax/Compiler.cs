using System.Collections.Immutable;

namespace LambdaCalc.Syntax;

public static class Compiler
{
    public static CompiledLambda? Compile(this DefinitionSyntax definition) => definition.GetProject()?.UnsafeCompile(definition)?.Evaluate() is { } value ? new CompiledLambda(value) : null;
}

public sealed class CompiledLambda
{
    private readonly StructuralLambda Value;

    internal CompiledLambda(StructuralLambda value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public CompiledLambda Invoke(CompiledLambda arg) => new (Value.Call(arg.Value).Evaluate());
    public CompiledLambda Invoke(params CompiledLambda[] args) => new (args.Aggregate(Value, (a, b) => a.Call(b.Value).Evaluate()));
    public bool Equals(CompiledLambda other) => Value.Equals(other.Value);

    public IGreenSyntax ToSyntax() =>
        AddRequiredWhitespaceVisitor.Perform(
        AddRequiredParenthesisVisitor.Perform(
        Value.ToSyntax(1, ImmutableDictionary<StructuralParameterId, IGreenExpressionSyntax>.Empty)));
    public override string ToString() => ToSyntax().Generating;
}

interface IStructural
{
    IStructural Replace(StructuralParameterId parameter, IStructural argument);
    StructuralLambda Evaluate();
    bool Equals(IStructural other);
    IGreenExpressionSyntax ToSyntax(int depth, ImmutableDictionary<StructuralParameterId, IGreenExpressionSyntax> names);
}

readonly record struct StructuralParameterId(int Id)
{
    public override string ToString() => Id.ToString();
    public override int GetHashCode() => Id.GetHashCode();
}
sealed record StructuralLambda(
    StructuralParameterId? SelfParameter,
    StructuralParameterId Parameter,
    IStructural Expression) : IStructural
{
    public IStructural Call(IStructural argument)
    {
        IStructural eval = Expression.Replace(Parameter, argument);
        if (SelfParameter is { } value)
            eval = eval.Replace(value, this);
        return eval;
    }

    public StructuralLambda WithExpression(IStructural expression) =>
        ReferenceEquals(expression, Expression)
        ? this : this with { Expression = expression };
    IStructural IStructural.Replace(StructuralParameterId parameter, IStructural argument) => Replace(parameter, argument);
    public StructuralLambda Replace(StructuralParameterId parameter, IStructural argument) => parameter != Parameter ? WithExpression(Expression.Replace(parameter, argument)) : this;
    public StructuralLambda Evaluate() => this;
    public bool Equals(IStructural other) => Equals(other as StructuralLambda);
    public bool Equals(StructuralLambda? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        var pseudoVar = new StructuralParameter(Parameter);
        var a = this.Expression;
        var b = other.Expression.Replace(other.Parameter, pseudoVar);
        return a.Equals(b);
    }

    public IGreenExpressionSyntax ToSyntax(int depth, ImmutableDictionary<StructuralParameterId, IGreenExpressionSyntax> names)
    {
        var identifierExpression = GreenVariableExpression.New(depth.ToString());
        var childNames = names.SetItem(Parameter, identifierExpression);
        if (SelfParameter is { } selfParameter)
        {
            var selfIdentifierExpression = GreenVariableExpression.New($"self{depth}");
            childNames.SetItem(selfParameter, selfIdentifierExpression);
        }
        return new GreenLambdaExpression(identifierExpression.Identifier,
            GreenTokenDoubleRightArrow.Instance,
            Expression.ToSyntax(depth + 1, childNames));
    }
    public override string ToString() => $"({Parameter}=>{Expression})";

    public StructuralLambda? EvaluateOrNull() => Evaluate();
}

sealed record StructuralCall(IStructural Left, IStructural Right) : IStructural
{
    public static IStructural Create(params IStructural[] args)
    {
        if (args.Length == 0)
            throw new ArgumentException($"Must pass at least on argument.");
        var result = args[0];
        for (int i = 1; i < args.Length; ++i)
            result = new StructuralCall(result, args[i]);
        return result;
    }
    public StructuralCall With(IStructural left, IStructural right) =>
        ReferenceEquals(Left, left) && ReferenceEquals(Right, right)
        ? this : this with { Left = left, Right = right };
    IStructural IStructural.Replace(StructuralParameterId parameter, IStructural argument) => Replace(parameter, argument);
    public StructuralCall Replace(StructuralParameterId parameter, IStructural argument) => With(
        Left.Replace(parameter, argument),
        Right.Replace(parameter, argument));

    public StructuralLambda Evaluate()
    {
        // Recursive version
        // return Left.Evaluate().Call(Right.Evaluate()).Evaluate();
        // Iterative version
        var resultCall = this;
        IStructural result;
        do
        {
            result = resultCall.Left.Evaluate().Call(resultCall.Right.Evaluate());
            resultCall = result as StructuralCall;
        } while (resultCall != null);
        return result.Evaluate();
    }
    
    public IGreenExpressionSyntax ToSyntax(int depth, ImmutableDictionary<StructuralParameterId, IGreenExpressionSyntax> names) =>
        new GreenCallExpression(
            Left.ToSyntax(depth, names),
            Right.ToSyntax(depth, names));
    public override string ToString() => $"({Left} {Right})";

    public bool Equals(IStructural other) => Equals(other as StructuralCall);
}

sealed record StructuralParameter(StructuralParameterId Id) : IStructural
{
    public IStructural Replace(StructuralParameterId parameter, IStructural argument) => Id.Equals(parameter) ? argument : this;
    public StructuralLambda Evaluate() => throw new ApplicationException();
    public IGreenExpressionSyntax ToSyntax(int depth, ImmutableDictionary<StructuralParameterId, IGreenExpressionSyntax> names) => names[Id];
    public override string ToString() => $"{Id}";
    public bool Equals(IStructural other) => Equals(other as StructuralParameter);
}

sealed class CompilerCache : IExpressionSyntax.IVisitor<IStructural>
{
    private readonly Dictionary<TokenIdentifier, IStructural> _translatedDefinitions = new();
    private readonly Dictionary<TokenIdentifier, StructuralParameter> _parameters = new();
    private readonly Dictionary<TokenIdentifier, StructuralParameter> _recursionIdentifiers = new();
    private int NextId = 0;

    private readonly DefinitionSyntax _emptyListSyntax;
    private readonly DefinitionSyntax _headListSyntax;
    public IStructural EmptyList => Compile(_emptyListSyntax);
    public IStructural HeadList => Compile(_headListSyntax);

    public CompilerCache(DefinitionSyntax emptyListSyntax, DefinitionSyntax headListSyntax)
    {
        _emptyListSyntax = emptyListSyntax;
        _headListSyntax = headListSyntax;
    }

    private IStructural GetValue(TokenIdentifier identifier)
    {
        var definition = identifier.GetDefinition() ?? throw new InvalidOperationException();
        if (_recursionIdentifiers.TryGetValue(definition, out var value))
            return value;
        return definition.Parent switch
        {
            DefinitionSyntax defSyntax => Compile(defSyntax),
            LambdaExpression lambdaSyntax => GetParameter(lambdaSyntax),
            _ => throw new InvalidOperationException()
        };
    }
    private StructuralParameter GetParameter(LambdaExpression lambda)
    {
        if (!_parameters.TryGetValue(lambda.ParameterName, out var value))
            value = _parameters[lambda.ParameterName] = new StructuralParameter(new StructuralParameterId(++NextId));
        return value;
    }
    public IStructural Compile(DefinitionSyntax definition)
    {
        if (!_translatedDefinitions.TryGetValue(definition.Name, out var value))
            value = _translatedDefinitions[definition.Name] = this.Visit(definition);
        return value;
    }
    public IStructural Compile(IExpressionSyntax expression) => expression.Accept(this);

    private IStructural Visit(DefinitionSyntax definition)
    {
        _recursionIdentifiers[definition.Name] = new StructuralParameter(new StructuralParameterId(++NextId));
        var result = definition.Value.Accept(this);
        _recursionIdentifiers.Remove(definition.Name);
        return result;
    }
    public IStructural Visit(VariableExpression variableExpression) => GetValue(variableExpression.Identifier);
    public IStructural Visit(CallExpression callExpression)
    {
        var left = callExpression.Left.Accept(this);
        var right = callExpression.Right.Accept(this);
        return new StructuralCall(left, right);
    }
    public IStructural Visit(LambdaExpression lambdaExpression)
    {
        var self = lambdaExpression.Parent is DefinitionSyntax parentDef ? _recursionIdentifiers[parentDef.Name].Id : default(StructuralParameterId?);
        var parameter = GetParameter(lambdaExpression);
        var expression = lambdaExpression.Expression.Accept(this);
        return new StructuralLambda(self, parameter.Id, expression);
    }
    public IStructural Visit(ParenthesisExpression parenthesisExpression) => parenthesisExpression.Expression.Accept(this);

    public IStructural Visit(ListExpression expression) => expression.Content is { } content ? Visit(content) : EmptyList;

    private IStructural Visit(ListContent content)
    {
        var result = Visit(content.Head);
        foreach (var tail in content.Rest)
            result = AppendList(result, Visit(tail));
        return result;
    }

    private IStructural AppendList(IStructural list, IStructural value) => StructuralCall.Create(HeadList, list, value);

    private IStructural Visit(ListContentTail tail) => tail.Value.Accept(this);

    private IStructural Visit(IListContentHead head)
    {
        if (head is ListContentHeadValue headValue)
            return AppendList(EmptyList, headValue.Value.Accept(this));
        else if (head is ListContentHeadAppend headAppend)
            return headAppend.Value.Accept(this);
        else
            throw new NotImplementedException();
    }
}
