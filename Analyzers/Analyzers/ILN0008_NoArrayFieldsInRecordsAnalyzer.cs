using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0008_NoArrayFieldsInRecordsAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new("ILN0008", "ILNumerics Array/Cell/Logicalal fields must not be used in records",
                                                           "Field '{0}' of type '{1}' must not be declared in a record; use a class instead", "ILNumerics",
                                                           DiagnosticSeverity.Error, true,
                                                           "ILNumerics Array, Cell and Logical fields require manual lifetime management via localMember/localCell/localLogical() which is incompatible with record semantics.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        if (ctx.Symbol is not IFieldSymbol field)
            return;

        // Only care about ILNumerics Array/Cell types
        if (field.Type is not INamedTypeSymbol fieldType || !ILNTypes.IsArray(fieldType))
            return;

        // Check if the containing type is a record
        if (field.ContainingType is not INamedTypeSymbol containingType || !containingType.IsRecord)
            return;

        ctx.ReportDiagnostic(Diagnostic.Create(Rule, field.Locations[0], field.Name, fieldType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }
}
