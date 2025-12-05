using ILNumerics.Core.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ILNumerics.Community.Analyzers.Tests;

public static class CSharpVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return new DiagnosticResult(diagnosticId, DiagnosticSeverity.Warning);
    }

    #region Nested Type: Test

    public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        public Test()
        {
            TestState.ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
            TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(Array<>).Assembly.Location)); // ILNumerics.Core
            TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(LicObject).Assembly.Location)); // ILNumerics.Core.Runtime
            TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(typeof(ILMath).Assembly.Location)); // ILNumerics.Computing
        }
    }

    #endregion
}
