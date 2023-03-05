namespace LambdaCalc.Diagnostics;

public readonly struct Location
{
    public readonly int Offset;
    public readonly int Length;
    public static Location FromOffsetLength(int offset, int length) =>new Location(offset, length);

    private Location(int offset, int length)
    {
        Offset = offset;
        Length = length;
    }

    public override string ToString() => $"{Offset}:{Length}";
}
