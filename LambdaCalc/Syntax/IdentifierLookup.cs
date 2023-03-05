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
            if (topLevelExpression.Parent is Project project)
            {
                foreach (var def in project.File.Definitions)
                {
                    if (def.Name.Value == variable.Value)
                        return def.Name;
                }
            }
        }
        return GetDefinitionSearch(variable, context.Parent);
    }
    public static IEnumerable<TokenIdentifier> GetAllReferences(this TokenIdentifier identifier) => 
        identifier.GetRoot()
        .AllNodesOfType<TokenIdentifier>()
        .Where(r => r.GetDefinition() == identifier);
}
