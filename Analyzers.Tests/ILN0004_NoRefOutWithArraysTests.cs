using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0004_NoRefOutWithArraysTests
{
    [Fact]
    public async Task Flags_Out_And_Fixes_To_OutArray_NoModifier()
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
            FixedCode = fixedCode,
            ExpectedDiagnostics = { CSharpVerifier<ILN0004_NoRefOutWithArraysAnalyzer, ILN0004_NoRefOutFix>.Diagnostic("ILN0004").WithSpan(6,37,6,38) }
        }.RunAsync();
    }

    [Fact]
    public async Task Flags_Ref_And_Fixes_To_OutArray_NoModifier()
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
            FixedCode = fixedCode,
            ExpectedDiagnostics = { CSharpVerifier<ILN0004_NoRefOutWithArraysAnalyzer, ILN0004_NoRefOutFix>.Diagnostic("ILN0004").WithSpan(6,37,6,38) }
        }.RunAsync();
    }
}
