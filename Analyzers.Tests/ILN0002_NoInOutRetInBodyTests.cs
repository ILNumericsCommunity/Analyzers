using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0002_NoInOutRetInBodyTests
{
    [Fact]
    public async Task Flags_InOutRet_In_Locals_Fields_Properties()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    // field
    [|InArray<double>|] myField;
    // property
    [|OutArray<double>|] MyProperty { get; set; }
    void M() {
        // local
        [|RetArray<double>|] localVar = null;
    }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test
        {
            TestCode = test,
            ExpectedDiagnostics =
            {
                CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Diagnostic("ILN0002").WithSpan(7, 6, 7, 22),
                CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Diagnostic("ILN0002").WithSpan(9, 6, 9, 23),
                CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Diagnostic("ILN0002").WithSpan(12, 9, 12, 24)
            }
        }.RunAsync();
    }
}
