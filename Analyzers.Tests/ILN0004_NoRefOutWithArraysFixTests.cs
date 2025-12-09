using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0004_NoRefOutWithArraysFixTests
{
    [Fact]
    public async Task Fixes_Out_Array_To_OutArray()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public void M(out Array<double> a) { a = zeros<double>(3,4); }
}";
        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public void M(OutArray<double> a) { a = zeros<double>(3,4); }
}";
        await new CSharpVerifier<ILN0004_NoRefOutWithArraysAnalyzer, ILN0004_NoRefOutFix>.Test
        {
            TestCode = test,
            FixedCode = fixedCode
        }.RunAsync();
    }

    [Fact]
    public async Task Fixes_Ref_Array_To_OutArray()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public void M(ref Array<double> a) { }
}";
        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public void M(OutArray<double> a) { }
}";
        await new CSharpVerifier<ILN0004_NoRefOutWithArraysAnalyzer, ILN0004_NoRefOutFix>.Test
        {
            TestCode = test,
            FixedCode = fixedCode
        }.RunAsync();
    }
}
