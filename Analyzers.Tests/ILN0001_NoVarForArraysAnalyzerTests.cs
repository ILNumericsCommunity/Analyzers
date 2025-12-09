using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0001_NoVarForArraysAnalyzerTests
{
    [Fact]
    public async Task Flags_Var_For_Array()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M() {
        [|var|] [|x = zeros<double>(3,4)|];
    }
}";
        await new CSharpVerifier<ILN0001_NoVarForArraysAnalyzer, ILN0001_NoVarForArraysFix>.Test { TestCode = test }.RunAsync();
    }
}
