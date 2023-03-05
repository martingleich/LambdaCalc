namespace LambdaCalc.Diagnostics;

public sealed class ParserErrorDiagnostic : IDiagnostic
{
    public ParserErrorDiagnostic(Location location, string expected)
    {
        Location = location;
        Expected = expected ?? throw new ArgumentNullException(nameof(expected));
    }

    public Location Location { get; }
    public string Expected { get; }
    public string Text => $"Expected '{Expected}'";

    public override string ToString() => $"{Location}: {Text}";
}
