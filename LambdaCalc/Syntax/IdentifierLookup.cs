namespace LambdaCalc.Syntax;

public static class IdentifierLookup
{
    public static TokenIdentifier? GetDefinition(this VariableExpression variable) => variable.Identifier.GetDefinition();
    public static TokenIdentifier? GetDefinition(this TokenIdentifier variable) => GetDefinitionSearch(variable, variable.Parent);
    private static TokenIdentifier? GetDefinitionSearch(TokenIdentifier variable, ISyntax? context)
    {
        if (context is null)
            return null;
        if (context is LambdaExpression lambda)
        {
            if (lambda.ParameterName.Value == variable.Value)
                return lambda.ParameterName;
        }
        else if (context is DefinitionSyntax definition)
        {
            if (definition.Name.Value == variable.Value)
            {
                return definition.Name;
            }
            else
            {
                if (definition.Parent is FileSyntax file)
                {
                    foreach (var def in file.Definitions)
                    {
                        if (def == definition)
                            break;
                        if (def.Name.Value == variable.Value)
                            return def.Name;
                    }
                }
            }
        }
        else if (context is TopLevelExpressionSyntax topLevelExpression)
        {
            foreach (var def in topLevelExpression.Project.File.Definitions)
            {
                if (def.Name.Value == variable.Value)
                    return def.Name;
            }
        }
        return GetDefinitionSearch(variable, context.Parent);
    }
    public static IEnumerable<TokenIdentifier> GetAllReferences(this TokenIdentifier identifier) => 
        identifier.GetRoot()
        .AllNodesOfType<TokenIdentifier>()
        .Where(r => r.GetDefinition() == identifier);
}
internal abstract class DefaultSyntaxVisitor<T> : ISyntax.IVisitor<T>
{
    protected abstract T Default(ISyntax syntax);
    public virtual T Accept(TopLevelExpressionSyntax topLevelExpressionSyntax) => Default(topLevelExpressionSyntax);
    public virtual T Visit(VariableExpression variableExpression) => Default(variableExpression);
    public virtual T Visit(CallExpression callExpression) => Default(callExpression);
    public virtual T Visit(LambdaExpression lambdaExpression) => Default(lambdaExpression);
    public virtual T Visit(ParenthesisExpression parenthesisExpression) => Default(parenthesisExpression);
    public virtual T Visit(FileSyntax file) => Default(file);
    public virtual T Visit(DefinitionSyntax definition) => Default(definition);
    public virtual T Visit(ListContentHeadAppend listContentHeadAppend) => Default(listContentHeadAppend);
    public virtual T Visit(ListContentHeadValue listContentHeadValue) => Default(listContentHeadValue);
    public virtual T Visit(ListContentTail listContentTail) => Default(listContentTail);
    public virtual T Visit(ListContent listContent) => Default(listContent);
    public virtual T Visit(ListExpression listExpression) => Default(listExpression);
    public virtual T Visit(TopLevelExpressionSyntax topLevelExpressionSyntax) => Default(topLevelExpressionSyntax);
}

public static class ScopeLookup
{
    class GetScopeVisitor : DefaultSyntaxVisitor<(TokenIdentifier, ISyntax)?>
    {
        public static readonly GetScopeVisitor Instance = new();
        protected override (TokenIdentifier, ISyntax)? Default(ISyntax syntax) => syntax.Parent?.Accept(this);
        public override (TokenIdentifier, ISyntax)? Visit(DefinitionSyntax definition) => (definition.Name, definition);
        public override (TokenIdentifier, ISyntax)? Visit(LambdaExpression lambdaExpression) => (lambdaExpression.ParameterName, lambdaExpression);
    }
    class GetParentScopeVisitor : DefaultSyntaxVisitor<ISyntax?>
    {
        public static readonly GetParentScopeVisitor Instance = new();
        protected override ISyntax? Default(ISyntax syntax) => syntax.Parent?.Accept(GetScopeVisitor.Instance)?.Item2;
        public override ISyntax? Visit(DefinitionSyntax definition)
        {
            if (definition.Parent is FileSyntax file)
            {
                ISyntax? prev = null;
                foreach (var def in file.Definitions)
                {
                    if (def == definition)
                        break;
                    prev = def;
                }
                return prev;
            }
            else
            {
                return null;
            } 
        }
        public override ISyntax? Visit(TopLevelExpressionSyntax topLevelExpressionSyntax)
        {
            return topLevelExpressionSyntax.Project.File.Definitions[topLevelExpressionSyntax.Project.File.Definitions.Length - 1];
        }
    }
    public static IEnumerable<TokenIdentifier> GetScope(ISyntax syntax)
    {
        ISyntax? cursor = syntax;
        while (cursor?.Accept(GetScopeVisitor.Instance) is { } scope)
        {
            yield return scope.Item1;
            cursor = scope.Item2.Accept(GetParentScopeVisitor.Instance);
        }
    }
}
