using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0005_LocalMemberForFieldsFixTests
{
    [Fact]
    public async Task Fixes_Private_Field_To_Readonly_LocalMember()
    {
        var test = @"
using ILNumerics;

public sealed class C {
    private Array<double> [|_A|];
}
";

        var fixedCode = @"
using ILNumerics;

public sealed class C {
    private readonly Array<double> _A = localMember<double>();
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, ILN0005_LocalMemberForFieldsFix>.Test { TestCode = test, FixedCode = fixedCode }
            .RunAsync();
    }

    [Fact]
    public async Task Preserves_Visibility_And_Generic_Type_Arguments()
    {
        var test = @"
using ILNumerics;

public sealed class C<T> {
    public Array<T> [|_A|];
}
";

        var fixedCode = @"
using ILNumerics;

public sealed class C<T> {
    public readonly Array<T> _A = localMember<T>();
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, ILN0005_LocalMemberForFieldsFix>.Test { TestCode = test, FixedCode = fixedCode }
            .RunAsync();
    }
}
