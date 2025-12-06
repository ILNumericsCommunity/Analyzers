using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0003_SignatureFlavorAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor ParamRule = new("ILN0003",
                                                                "Prefer InArray/OutArray in public signatures",
                                                                "Parameter '{0}' uses '{1}', consider '{2}' for ILNumerics APIs",
                                                                "ILNumerics",
                                                                DiagnosticSeverity.Info,
                                                                true);

    public static readonly DiagnosticDescriptor ReturnRule = new("ILN0003R",
                                                                 "Prefer RetArray return type",
                                                                 "Method returns '{0}', consider returning 'RetArray<>' for ILNumerics APIs",
                                                                 "ILNumerics",
                                                                 DiagnosticSeverity.Info,
                                                                 true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ParamRule, ReturnRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext ctx)
    {
        if (ctx.Symbol is not IMethodSymbol m)
            return;

        if (m.MethodKind != MethodKind.Ordinary)
            return;

        if (m.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
            return;

        // Parameters
        foreach (var p in m.Parameters)
        {
            if (p.Type is INamedTypeSymbol t && IlnTypes.IsArray(t))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(ParamRule, p.Locations[0], p.Name, t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), "InArray<>/OutArray<>"));
            }
        }

        // Return type
        if (m.ReturnType is INamedTypeSymbol rt && IlnTypes.IsArray(rt))
            ctx.ReportDiagnostic(Diagnostic.Create(ReturnRule, m.Locations[0], rt.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }
}
