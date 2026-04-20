using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0009_NoCompoundOperatorOnArrayTests
{
    private static CSharpVerifier<ILN0009_NoCompoundOperatorOnArrayAnalyzer, ILN0009_NoCompoundOperatorOnArrayFix>.Test CreateTest(string testCode, string? fixedCode = null)
    {
        var test = new CSharpVerifier<ILN0009_NoCompoundOperatorOnArrayAnalyzer, ILN0009_NoCompoundOperatorOnArrayFix>.Test
        {
            TestCode = testCode
        };

        if (fixedCode is not null)
            test.FixedCode = fixedCode;

        return test;
    }

    [Fact]
    public async Task Flags_Warning_For_Compound_On_Array_Variable()
    {
        var test = @"
using ILNumerics;

class C {
    void M(Array<double> A) {
        {|#0:A += 2|};
    }
}";
        var t = CreateTest(test);
        t.ExpectedDiagnostics.Add(new DiagnosticResult(ILN0009_NoCompoundOperatorOnArrayAnalyzer.Rule)
                                  .WithLocation(0).WithArguments("+=", "A"));

        await t.RunAsync();
    }

    [Fact]
    public async Task Flags_Error_For_Compound_On_Array_With_Indexer()
    {
        var test = @"
using ILNumerics;

class C {
    void M(Array<double> A) {
        {|#0:A[0] += 2|};
    }
}";
        var t = CreateTest(test);
        t.ExpectedDiagnostics.Add(new DiagnosticResult(ILN0009_NoCompoundOperatorOnArrayAnalyzer.Rule)
                                  .WithLocation(0).WithSeverity(DiagnosticSeverity.Error).WithArguments("+=", "A[0]"));

        await t.RunAsync();
    }

    [Fact]
    public async Task No_Diagnostic_For_Non_ILNumerics_Type()
    {
        var test = @"
class C {
    void M() {
        int x = 1;
        x += 2;
    }
}";
        await CreateTest(test).RunAsync();
    }

    [Fact]
    public async Task Codefix_Expands_Compound_On_Variable()
    {
        var test = @"
using ILNumerics;

class C {
    void M(Array<double> A) {
        {|#0:A += 2|};
    }
}";
        var fix = @"
using ILNumerics;

class C {
    void M(Array<double> A) {
        A = (A + 2);
    }
}";
        var t = CreateTest(test, fix);
        t.ExpectedDiagnostics.Add(new DiagnosticResult(ILN0009_NoCompoundOperatorOnArrayAnalyzer.Rule)
                                  .WithLocation(0).WithArguments("+=", "A"));

        await t.RunAsync();
    }

    [Fact]
    public async Task Codefix_Expands_Compound_On_Indexer()
    {
        var test = @"
using ILNumerics;

class C {
    void M(Array<double> A) {
        {|#0:A[0] += 2|};
    }
}";
        var fix = @"
using ILNumerics;

class C {
    void M(Array<double> A) {
        A[0] = (A[0] + 2);
    }
}";
        var t = CreateTest(test, fix);
        t.ExpectedDiagnostics.Add(new DiagnosticResult(ILN0009_NoCompoundOperatorOnArrayAnalyzer.Rule)
                                  .WithLocation(0).WithSeverity(DiagnosticSeverity.Error).WithArguments("+=", "A[0]"));

        await t.RunAsync();
    }

    [Fact]
    public async Task Flags_SubtractAssignment_On_Array()
    {
        var test = @"
using ILNumerics;

class C {
    void M(Array<double> A) {
        {|#0:A -= 1|};
    }
}";
        var t = CreateTest(test);
        t.ExpectedDiagnostics.Add(new DiagnosticResult(ILN0009_NoCompoundOperatorOnArrayAnalyzer.Rule)
                                  .WithLocation(0).WithArguments("-=", "A"));

        await t.RunAsync();
    }

    [Fact]
    public async Task Flags_MultiplyAssignment_On_Array_With_Indexer()
    {
        var test = @"
using ILNumerics;

class C {
    void M(Array<double> A) {
        {|#0:A[0] *= 3|};
    }
}";
        var t = CreateTest(test);
        t.ExpectedDiagnostics.Add(new DiagnosticResult(ILN0009_NoCompoundOperatorOnArrayAnalyzer.Rule)
                                  .WithLocation(0).WithSeverity(DiagnosticSeverity.Error).WithArguments("*=", "A[0]"));

        await t.RunAsync();
    }
}
