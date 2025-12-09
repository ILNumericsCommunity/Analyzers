using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0006_LocalMemberAssignmentAnalyzerTests
{
    [Fact]
    public async Task Allows_Assignment_Via_a_And_Assign()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class C {
    private readonly Array<double> _A = localMember<double>();

    public C(InArray<double> x) {
        _A.a = check(x);
        _A.Assign(check(x));
    }
}
";

        await new CSharpVerifier<ILN0006_LocalMemberAssignmentAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_Direct_Field_Assignment()
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

        await new CSharpVerifier<ILN0006_LocalMemberAssignmentAnalyzer, EmptyCodeFixProvider>.Test
        {
            TestCode = test,
            ExpectedDiagnostics =
            {
                CSharpVerifier<ILN0006_LocalMemberAssignmentAnalyzer, EmptyCodeFixProvider>.Diagnostic("ILN0006").WithSpan(12, 9, 12, 11)
            }
        }.RunAsync();
    }
}
