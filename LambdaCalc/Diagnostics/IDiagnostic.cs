using System.Collections;

namespace LambdaCalc.Diagnostics;

public interface IDiagnostic
{
    Location Location { get; }
    string Text { get; }
}

public sealed class DiagnosticsBag : IReadOnlyCollection<IDiagnostic>
{
    private readonly List<IDiagnostic> _diagnostics = new();

    public DiagnosticsBag() { }
    public void Add(IDiagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }
    public int Count => _diagnostics.Count;

    public IEnumerator<IDiagnostic> GetEnumerator()
    {
        foreach (var x in _diagnostics)
            yield return x;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public override string ToString() => $"Count: {Count}";
}
