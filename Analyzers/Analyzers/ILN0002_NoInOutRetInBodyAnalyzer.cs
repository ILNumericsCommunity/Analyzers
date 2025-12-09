using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0002_NoInOutRetInBodyAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new("ILN0002",
                                                           "Only In/Out/Ret in method signatures",
                                                           "Type '{0}' should only appear in method signatures (not in locals/fields/properties)",
                                                           "ILNumerics",
                                                           DiagnosticSeverity.Warning,
                                                           true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Inspect fields, properties and local variables for illegal In/Out/Ret usage
        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        context.RegisterOperationAction(AnalyzeLocals, OperationKind.VariableDeclarationGroup);
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        var f = (IFieldSymbol) ctx.Symbol;
        if (f.Type is INamedTypeSymbol t && (IlnTypes.IsIlnIn(t) || IlnTypes.IsIlnOut(t) || IlnTypes.IsIlnRet(t)))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule, f.Locations[0], t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }

    private static void AnalyzeProperty(SymbolAnalysisContext ctx)
    {
        var p = (IPropertySymbol) ctx.Symbol;
        if (p.Type is INamedTypeSymbol t && (IlnTypes.IsIlnIn(t) || IlnTypes.IsIlnOut(t) || IlnTypes.IsIlnRet(t)))
        {
            var hasGet = p.GetMethod != null;
            var hasSet = p.SetMethod != null;
            var isInitOnly = p.SetMethod?.IsInitOnly == true; // Roslyn exposes init-only via SetMethod

            // Allow: get-only properties of Ret*
            if (IlnTypes.IsIlnRet(t) && hasGet && !hasSet)
                return;

            // Allow: set-only properties of In*
            if (IlnTypes.IsIlnIn(t) && !hasGet && hasSet)
                return;

            // Allow: init-only properties of In* (usually have get + init set)
            if (IlnTypes.IsIlnIn(t) && isInitOnly)
                return;

            // All other cases (including Out*, any Ret* with a setter, any In* with getter that is not init-only) are reported
            ctx.ReportDiagnostic(Diagnostic.Create(Rule, p.Locations[0], t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }
    }

    private static void AnalyzeLocals(OperationAnalysisContext ctx)
    {
        var group = (IVariableDeclarationGroupOperation) ctx.Operation;
        foreach (var decl in group.Declarations)
        {
            foreach (var v in decl.Declarators)
            {
                if (v.Symbol?.Type is INamedTypeSymbol t && (IlnTypes.IsIlnIn(t) || IlnTypes.IsIlnOut(t) || IlnTypes.IsIlnRet(t)))
                    ctx.ReportDiagnostic(Diagnostic.Create(Rule, v.Syntax.GetLocation(), t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
            }
        }
    }
}
