using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0006_GetValueForScalarAccessFixTests
{
    [Fact]
    public async Task Fixes_ExplicitCast_ElementAccess_To_GetValue2D()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> f) {
        double x = [|(double) f[1, 2]|];
    }
}";

        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> f) {
        double x = f.GetValue(1, 2);
    }
}";

        await new CSharpVerifier<ILN0006_GetValueForScalarAccessAnalyzer, ILN0006_GetValueForScalarAccessFix>.Test
        {
            TestCode = test,
            FixedCode = fixedCode
        }.RunAsync();
    }

    [Fact]
    public async Task Fixes_ExplicitCast_ElementAccess_To_GetValue3D()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> f) {
        double x = [|(double) f[1, 2, 3]|];
    }
}";

        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> f) {
        double x = f.GetValue(1, 2, 3);
    }
}";

        await new CSharpVerifier<ILN0006_GetValueForScalarAccessAnalyzer, ILN0006_GetValueForScalarAccessFix>.Test
        {
            TestCode = test,
            FixedCode = fixedCode
        }.RunAsync();
    }
}
