using LambdaCalc.Syntax;

namespace LambdaCalc.Diagnostics;

public sealed class UnknownVariableDiagnostic : IDiagnostic
{
    public UnknownVariableDiagnostic(VariableExpression node)
    {
        Node = node ?? throw new ArgumentNullException(nameof(node));
    }

    public VariableExpression Node { get; }
    public Location Location => Node.GetLocation();
    public string Text => $"No definition for '{Node.Identifier.Value}'.";

    public override string ToString() => $"{Location}: {Text}";
}
