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
    public void M(Array<double> {|ILN0003:a|}) { }
}";
        await new CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Suggests_Ret_For_Return()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public Array<double> {|ILN0003R:M|}() { return zeros<double>(3,4); }
}";
        await new CSharpVerifier<ILN0003_SignatureFlavorAnalyzer, ILN0003_SignatureFlavorFix>.Test { TestCode = test }.RunAsync();
    }
}
