using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0005_LocalMemberForFieldsAnalyzerTests
{
    [Fact]
    public async Task Flags_Field_Without_localMember_Initializer()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class ClassWithArrayField
{
    private Array<double> {|ILN0005:_A|};
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, ILN0005_LocalMemberForFieldsFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Does_Not_Flag_Field_With_localMember_Initializer()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class ClassWithArrayField
{
    private readonly Array<double> _A = localMember<double>();
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, EmptyCodeFixProvider>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Allows_Assignment_Via_a_And_Assign()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class ClassWithArrayField {
    private readonly Array<double> _A = localMember<double>();

    public ClassWithArrayField(InArray<double> x) {
        _A.a = check(x);
        _A.Assign(check(x));
    }
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, ILN0005_LocalMemberForFieldsFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_Direct_Field_Assignment()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class ClassWithArrayField {
    private readonly Array<double> _A = localMember<double>();

    public ClassWithArrayField(InArray<double> x) {
        {|ILN0005A:_A|} = check(x);
    }
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, ILN0005_LocalMemberForFieldsFix>.Test { TestCode = test }.RunAsync();
    }
}
