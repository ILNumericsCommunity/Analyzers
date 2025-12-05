using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0003_SignatureFlavorTests
{
    [Fact]
    public async Task Suggests_InOut_For_Params_And_Fixes_To_In()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public void M([|Array<double>|] a) { }
}";
        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public void M(InArray<double> a) { }
}";
        await new CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Test
        {
            TestCode = test,
            FixedCode = fixedCode,
            ExpectedDiagnostics = { CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Diagnostic("ILN0003").WithSpan(7,18,7,31) }
        }.RunAsync();
    }

    [Fact]
    public async Task Suggests_Ret_For_Return_And_Fixes()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public [|Array<double>|] M() { return zeros<double>(3,4); }
}";
        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public RetArray<double> M() { return zeros<double>(3,4); }
}";
        await new CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Test
        {
            TestCode = test,
            FixedCode = fixedCode,
            ExpectedDiagnostics = { CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Diagnostic("ILN0003R").WithSpan(7,8,7,21) }
        }.RunAsync();
    }
}
