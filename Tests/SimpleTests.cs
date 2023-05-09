using Distributions;

namespace Tests;
public static class StatisicParserTests
{
    [Fact]
    public static void RandomTest()
    {
        var randomExpressions =
           (from x in ProgramList.CreateInstance().GetStructureLambdaExpressionSyntax(10)
            select AddRequiredWhitespaceVisitor.Perform(AddRequiredParenthesisVisitor.Perform(x)))
            .AssignRandomNames(Distribution.UniformLowerCaseLetterString);

        var rnd = Xorshift64.Create(1432543523412);
        for(int i = 0; i < 1000; ++i)
        {
            var txt = randomExpressions.Sample(rnd).ToString();
            var parsed = Parser.ParseExpression(txt, new LambdaCalc.Diagnostics.DiagnosticsBag());
            Assert.Equal(txt, parsed.ToString());
        }
    }
}

public static class SimpleTests
{
    private static readonly Project _project = Project.Empty.SetFileText(@"
def I = x => x
def BoolT = x => y => x
def BoolF = x => y => y
");
    [Theory]
    [InlineData("I", "Blub")]
    [InlineData("Blub", "I")]
    [InlineData("BoolT I I", "BoolT")]
    public static void Fail(string expected, string actual)
    {
        var result = _project.CheckEqual(expected, actual);
        Assert.False(result.RunResult);
    }
}
public static class SimpleTestsBool
{
    public static readonly string LibBool = @"
def I = x => x
def const = x => y => x

def BoolT = x => y => x
def BoolF = x => y => y
def BoolNot = x => x BoolF BoolT
def BoolAnd = x => x I (const x)
def BoolOr  = x => x (const x) I
def BoolEqual    = x => x I BoolNot
def BoolNotEqual = x => x BoolNot I
";
    private static readonly Project _project = Project.Empty.SetFileText(LibBool);
    [Theory]
    [InlineData("I I", "I")]

    [InlineData("BoolNot BoolT", "BoolF")]
    [InlineData("BoolNot BoolF", "BoolT")]

    [InlineData("BoolAnd BoolT BoolT", "BoolT")]
    [InlineData("BoolAnd BoolT BoolF", "BoolF")]
    [InlineData("BoolAnd BoolF BoolT", "BoolF")]
    [InlineData("BoolAnd BoolF BoolF", "BoolF")]

    [InlineData("BoolOr BoolT BoolT", "BoolT")]
    [InlineData("BoolOr BoolT BoolF", "BoolT")]
    [InlineData("BoolOr BoolF BoolT", "BoolT")]
    [InlineData("BoolOr BoolF BoolF", "BoolF")]

    [InlineData("BoolEqual BoolT BoolT", "BoolT")]
    [InlineData("BoolEqual BoolT BoolF", "BoolF")]
    [InlineData("BoolEqual BoolF BoolT", "BoolF")]
    [InlineData("BoolEqual BoolF BoolF", "BoolT")]

    [InlineData("BoolNotEqual BoolT BoolT", "BoolF")]
    [InlineData("BoolNotEqual BoolT BoolF", "BoolT")]
    [InlineData("BoolNotEqual BoolF BoolT", "BoolT")]
    [InlineData("BoolNotEqual BoolF BoolF", "BoolF")]
    public static void Okay(string expected, string actual)
    {
        var result = _project.CheckEqual(expected, actual);
        if (!result.RunResult)
            Assert.Fail(result.ToString());
    }
}
public static class SimpleTestsList
{
    private static readonly Project _project = Project.Empty.SetFileText($@"
{SimpleTestsBool.LibBool}

def ListEmpty =           x => y => x
def ListHead  = h => v => x => y => y h v
def ListIsEmpty = x => x BoolT (h => v => BoolF)
def ListMap = f => x => x x (h => v => ListHead (ListMap f h) (f v))
def ListFilter = f => x => x x (h => v => (r => f x (ListHead r v) r) (ListFilter f h))
def ListAll = f => x => x BoolT (h => v => f v (ListAll f h) BoolF)
def ListAny = f => x => x BoolF (h => v => f v BoolT (ListAny f h))
");
    [Theory]
    [InlineData("ListIsEmpty ListEmpty", "BoolT")]
    [InlineData("ListIsEmpty (ListHead ListEmpty I)", "BoolF")]
    [InlineData("ListMap BoolNot ListEmpty", "ListEmpty")]
    [InlineData("ListMap BoolNot (ListHead (ListHead ListEmpty BoolT) BoolF)", "(ListHead (ListHead ListEmpty BoolF) BoolT)")]
    [InlineData("ListAll I ListEmpty", "BoolT")]
    [InlineData("ListAll I (ListHead (ListHead ListEmpty BoolT) BoolF)", "BoolF")]
    [InlineData("ListAll I (ListHead (ListHead ListEmpty BoolT) BoolT)", "BoolT")]
    [InlineData("ListAny I ListEmpty", "BoolF")]
    [InlineData("ListAny I (ListHead (ListHead ListEmpty BoolF) BoolF)", "BoolF")]
    [InlineData("ListAny I (ListHead (ListHead ListEmpty BoolT) BoolF)", "BoolT")]
    public static void Okay(string expected, string actual)
    {
        var result = _project.CheckEqual(expected, actual);
        if (!result.RunResult)
            Assert.Fail(result.ToString());
    }
    [Theory]
    [InlineData("ListIsEmpty []", "BoolT")]
    [InlineData("ListIsEmpty [I]", "BoolF")]
    [InlineData("ListMap BoolNot []", "ListEmpty")]
    [InlineData("ListMap BoolNot [BoolT, BoolF]", "[BoolF, BoolT]")]
    [InlineData("ListAll I []", "BoolT")]
    [InlineData("ListAll I [BoolT, BoolF]", "BoolF")]
    [InlineData("ListAll I [BoolT, BoolT]", "BoolT")]
    [InlineData("ListAny I []", "BoolF")]
    [InlineData("ListAny I [BoolF, BoolF]", "BoolF")]
    [InlineData("ListAny I [BoolT, BoolF]", "BoolT")]
    [InlineData("[[BoolT, BoolT].., BoolF]", "[BoolT, BoolT, BoolF]")]
    public static void OkayListSyntax(string expected, string actual)
    {
        var result = _project.CheckEqual(expected, actual);
        if (!result.RunResult)
            Assert.Fail(result.ToString());
    }
}