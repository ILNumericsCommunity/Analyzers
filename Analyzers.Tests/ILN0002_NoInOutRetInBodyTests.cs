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
    // property with get+set: always Warning
    [|OutArray<double>|] [|MyProperty|] { get; set; }
    void M() {
        // local
        [|RetArray<double>|] [|localVar = null|];
    }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_GetSet_Properties_As_Warning()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    [|InArray<double>|] [|InGetSet|] { get; set; }
    RetArray<double> RetGetSet { get; set; }
    [|OutArray<double>|] [|OutGetSet|] { get; set; }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Allows_Ret_GetOnly_Property()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    RetArray<double> RetGetOnly { get; }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Allows_In_SetOnly_Property()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    InArray<double> InSetOnly { set { } }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Allows_Ret_InitOnly_Property()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    RetArray<double> RetInitOnly { get; init; }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_In_InitOnly_Property()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    [|InArray<double>|] [|InInitOnly|] { get; init; }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Allows_In_InitOnly_Property()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    InArray<double> InInitOnly { init { } }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_Out_Property_As_Warning()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    [|OutArray<double>|] [|OutGetOnly|] { get; }
    [|OutArray<double>|] [|OutSetOnly|] { set { } }
}";
        await new CSharpVerifier<ILN0002_NoInOutRetInBodyAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }
}
