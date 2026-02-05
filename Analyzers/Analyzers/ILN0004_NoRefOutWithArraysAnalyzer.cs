using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0004_NoRefOutWithArraysAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new("ILN0004", "Avoid 'out/ref' with IL arrays", "Parameter '{0}' uses '{1}' (prefer 'OutArray<>' and remove 'out/ref')",
                                                           "ILNumerics", DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Inspect parameters to disallow C# ref/out with ILNumerics array types
        context.RegisterSymbolAction(AnalyzeParam, SymbolKind.Parameter);
    }

    private static void AnalyzeParam(SymbolAnalysisContext ctx)
    {
        var p = (IParameterSymbol) ctx.Symbol;

        // Only parameters with C# ref/out modifiers are interesting
        if (!p.RefKind.HasFlag(RefKind.Ref) && !p.RefKind.HasFlag(RefKind.Out))
            return;

        if (p.Type is INamedTypeSymbol t && ILNTypes.IsAnyIln(t))
        {
            var kind = p.RefKind.ToString().ToLowerInvariant();
            ctx.ReportDiagnostic(Diagnostic.Create(Rule, p.Locations[0], p.Name, kind));
        }
    }
}
