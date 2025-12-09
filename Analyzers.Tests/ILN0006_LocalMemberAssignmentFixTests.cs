using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0006_LocalMemberAssignmentFixTests
{
    [Fact]
    public async Task Rewrites_Field_Assignment_To_DotA()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class C {
    private readonly Array<double> _A = localMember<double>();

    public void M(InArray<double> x) {
        [|_A|] = check(x);
    }
}
";

        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class C {
    private readonly Array<double> _A = localMember<double>();

    public void M(InArray<double> x) {
        _A.a = check(x);
    }
}
";

        await new CSharpVerifier<ILN0006_LocalMemberAssignmentAnalyzer, ILN0006_LocalMemberAssignmentFix>.Test { TestCode = test, FixedCode = fixedCode }
            .RunAsync();
    }
}
