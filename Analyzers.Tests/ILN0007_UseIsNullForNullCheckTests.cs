using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0007_UseIsNullForNullCheckTests
{
    [Fact]
    public async Task Flags_EqualsNull_On_Array()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> arr) {
        if ([|arr == null|]) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_NotEqualsNull_On_Array()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> arr) {
        if ([|arr != null|]) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task DoesNotFlag_IsNull_Pattern_On_Array()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> arr) {
        if (arr is null) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task DoesNotFlag_IsNotNull_Pattern_On_Array()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> arr) {
        if (arr is not null) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Fixes_EqualsNull_To_IsNull()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> arr) {
        if ([|arr == null|]) { }
    }
}";
        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> arr) {
        if (isnull(arr)) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test, FixedCode = fixedCode }.RunAsync();
    }

    [Fact]
    public async Task Fixes_NotEqualsNull_To_NotIsNull()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> arr) {
        if ([|arr != null|]) { }
    }
}";
        var fixedCode = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> arr) {
        if (!isnull(arr)) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test, FixedCode = fixedCode }.RunAsync();
    }

    [Fact]
    public async Task DoesNotFlag_NonIlnType()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(string s) {
        if (s == null) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_NullOnLeftSide()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Array<double> arr) {
        if ([|null == arr|]) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_Logical_Type()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Logical arr) {
        if ([|arr == null|]) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_Cell_Type()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M(Cell arr) {
        if ([|arr == null|]) { }
    }
}";
        await new CSharpVerifier<ILN0007_UseIsNullForNullCheckAnalyzer, ILN0007_UseIsNullForNullCheckFix>.Test { TestCode = test }.RunAsync();
    }
}
