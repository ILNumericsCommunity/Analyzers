using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0003_SignatureFlavorAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor ParamRule = new("ILN0003", "Prefer InArray/OutArray in public signatures",
                                                                "Parameter '{0}' uses '{1}', consider '{2}' for ILNumerics APIs", "ILNumerics", DiagnosticSeverity.Info, true);

    public static readonly DiagnosticDescriptor ReturnRule = new("ILN0003R", "Prefer RetArray return type",
                                                                 "Method returns '{0}', consider returning 'RetArray<>' for ILNumerics APIs", "ILNumerics", DiagnosticSeverity.Info,
                                                                 true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ParamRule, ReturnRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Look at method symbols to enforce signature conventions
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext ctx)
    {
        if (ctx.Symbol is not IMethodSymbol m)
            return;

        // Only analyze ordinary (non-ctor, non-property-accessor) methods
        if (m.MethodKind != MethodKind.Ordinary)
            return;

        // Restrict to public and internal APIs that form the surface area
        if (m.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
            return;

        // Parameters: flag plain Array<T> and suggest InArray/OutArray instead
        foreach (var p in m.Parameters)
        {
            if (p.Type is INamedTypeSymbol t && ILNTypes.IsArray(t))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(ParamRule, p.Locations[0], p.Name, t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), "InArray<>/OutArray<>"));
            }
        }

        // Return type: flag plain Array<T> and suggest RetArray instead
        if (m.ReturnType is INamedTypeSymbol rt && ILNTypes.IsArray(rt))
            ctx.ReportDiagnostic(Diagnostic.Create(ReturnRule, m.Locations[0], rt.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }
}
