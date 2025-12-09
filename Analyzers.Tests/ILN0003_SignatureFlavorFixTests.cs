using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0003_SignatureFlavorFixTests
{
    [Fact]
    public async Task Fixes_Param_To_InArray()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public void M(Array<double> {|ILN0003:a|}) { }
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
            FixedCode = fixedCode
        }.RunAsync();
    }

    [Fact]
    public async Task Fixes_Return_Type_To_RetArray()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    public Array<double> {|ILN0003R:M|}() { return zeros<double>(3,4); }
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
            FixedCode = fixedCode
        }.RunAsync();
    }
}
