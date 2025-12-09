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
using static ILNumerics.ILMath;

public sealed class ClassWithArrayField {
    private Array<double> {|ILN0005:_A|};
}
";

        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class ClassWithArrayField {
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
using static ILNumerics.ILMath;

public sealed class ClassWithArrayField<T> {
    public Array<T> {|ILN0005:_A|};
}
";

        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class ClassWithArrayField<T> {
    public readonly Array<T> _A = localMember<T>();
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, ILN0005_LocalMemberForFieldsFix>.Test { TestCode = test, FixedCode = fixedCode }
            .RunAsync();
    }

    [Fact]
    public async Task Rewrites_Field_Assignment_To_DotA()
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

        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

public sealed class ClassWithArrayField {
    private readonly Array<double> _A = localMember<double>();

    public ClassWithArrayField(InArray<double> x) {
        _A.a = check(x);
    }
}
";

        await new CSharpVerifier<ILN0005_LocalMemberForFieldsAnalyzer, ILN0005_LocalMemberForFieldsFix>.Test { TestCode = test, FixedCode = fixedCode }
            .RunAsync();
    }
}
