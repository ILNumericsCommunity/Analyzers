using System.Threading.Tasks;
using ILNumerics.Community.Analyzers.Analyzers;
using ILNumerics.Community.Analyzers.CodeFixes;
using Xunit;

namespace ILNumerics.Community.Analyzers.Tests;

public class ILN0001_NoVarForArraysAnalyzerTests
{
    [Fact]
    public async Task Flags_Var_For_Array()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class C {
    void M() {
        [|var|] [|x = zeros<double>(3,4)|];
    }
}";
        await new CSharpVerifier<ILN0001_NoVarForArraysAnalyzer, ILN0001_NoVarForArraysFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Allows_Var_For_Linq_ToArray()
    {
        var test = @"
using System.Collections.Generic;
using System.Linq;
using ILNumerics;

class Sample {
    public IEnumerable<double> SomeFunction() => new double[] { 1, 2 };
}

class C {
    void M(Sample sample) {
        var elements = sample.SomeFunction().ToArray();
        double[] typedElements = elements;
    }
}";
        await new CSharpVerifier<ILN0001_NoVarForArraysAnalyzer, ILN0001_NoVarForArraysFix>.Test { TestCode = test }.RunAsync();
    }

    [Fact]
    public async Task Flags_Var_When_ToArray_Returns_Iln_Array()
    {
        var test = @"
using ILNumerics;
using static ILNumerics.ILMath;

class Sequence {
    public Array<double> ToArray() => zeros<double>(2);
}

class Sample {
    public Sequence SomeFunction() => new Sequence();
}

class C {
    void M(Sample sample) {
        [|var|] [|elements = sample.SomeFunction().ToArray()|];
    }
}";
        await new CSharpVerifier<ILN0001_NoVarForArraysAnalyzer, ILN0001_NoVarForArraysFix>.Test { TestCode = test }.RunAsync();
    }
}
