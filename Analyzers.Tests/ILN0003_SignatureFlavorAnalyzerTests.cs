using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0003_SignatureFlavorAnalyzerTests
{
    [Fact]
    public async Task Suggests_InOut_For_Params()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public void M([|Array<double>|] a) { }
}";
        await new CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Test
        {
            TestCode = test,
            ExpectedDiagnostics =
            {
                CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Diagnostic("ILN0003").WithSpan(7, 18, 7, 31)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task Suggests_Ret_For_Return()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public [|Array<double>|] M() { return zeros<double>(3,4); }
}";
        await new CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Test
        {
            TestCode = test,
            ExpectedDiagnostics =
            {
                CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Diagnostic("ILN0003R").WithSpan(7, 8, 7, 21)
            }
        }.RunAsync();
    }
}
