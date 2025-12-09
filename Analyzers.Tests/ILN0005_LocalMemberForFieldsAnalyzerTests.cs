using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0005_LocalMemberForFieldsAnalyzerTests
{
    [Fact]
    public async Task Flags_Field_Without_localMember_Initializer()
    {
        var test = @"
using ILNumerics;

public sealed class StateSpacePlant
{
    private readonly Array<double> [|_A|];
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, EmptyCodeFixProvider>.Test
        {
            TestCode = test,
            ExpectedDiagnostics =
            {
                CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, EmptyCodeFixProvider>.Diagnostic("ILN0005").WithSpan(6, 33, 6, 35)
            }
        }.RunAsync();
    }

    [Fact]
    public async Task Does_Not_Flag_Field_With_localMember_Initializer()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class StateSpacePlant
{
    private readonly Array<double> _A = localMember<double>();
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }
}
