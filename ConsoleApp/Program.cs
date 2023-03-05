using Distributions;
using LambdaCalc;
using LambdaCalc.Syntax;
using System.Security.Cryptography;

namespace ConsoleApp
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var project = Project.Empty.SetFileText(@"
def I = x => x
def BoolT = x => y => x
def BoolF = x => y => y
def BoolNot = x => x BoolF BoolT
def BoolAnd = x => y => x y x
def BoolOr  = x => y => x x y
def BoolEqual    = x => x I BoolNot
def BoolNotEqual = x => x BoolNot I
");
            var result1 = project.CheckEqual("BoolNot BoolF", "BoolT");
            var result2 = project.CheckEqual("BoolOr BoolF BoolF", "BoolT");

            var randomExpressions = 
               (from x in ProgramList.CreateInstance().GetStructureLambdaExpressionSyntax(3)
               select AddRequiredWhitespaceVisitor.Perform(AddRequiredParenthesisVisitor.Perform(x)))
                .AssignRandomNames(Distribution.UniformLowerCaseLetterString);

            var rnd = RandomNumberGenerator.Create();
            while (true)
            {
                var txt = randomExpressions.Sample(rnd);
                Console.WriteLine("======================================================================================");
                Console.WriteLine(txt);
            }
        }
    }
}