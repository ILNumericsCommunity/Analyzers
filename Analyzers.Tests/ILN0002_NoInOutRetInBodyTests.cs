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
    [|InArray<double>|] [|myField|];
    // property
    [|OutArray<double>|] [|MyProperty|] { get; set; }
    void M() {
        // local
        [|RetArray<double>|] [|localVar = null|];
    }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }
}
