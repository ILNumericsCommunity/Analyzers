using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0005_LocalMemberForFieldsAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new("ILN0005",
                                                           "ILNumerics fields should use localMember<T>() pattern",
                                                           "Field '{0}' of type '{1}' should be initialized via localMember<T>()",
                                                           "ILNumerics",
                                                           DiagnosticSeverity.Warning,
                                                           true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Check field declarations for the required localMember<T>() initialization pattern
        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        if (ctx.Symbol is not IFieldSymbol field)
            return;

        if (field.Type is not INamedTypeSymbol t || !IlnTypes.IsArray(t))
            return;

        // Only care about instance fields
        if (field.IsStatic)
            return;

        // If field has an initializer, ensure it's localMember<>()
        var declSyntaxRef = field.DeclaringSyntaxReferences.FirstOrDefault();
        if (declSyntaxRef == null)
            return;

        var syntax = declSyntaxRef.GetSyntax(ctx.CancellationToken) as VariableDeclaratorSyntax;
        if (syntax?.Initializer is { } init)
        {
            if (init.Value is InvocationExpressionSyntax invocation &&
                invocation.Expression is GenericNameSyntax gName &&
                gName.Identifier.Text == "localMember")
            {
                // Pattern already followed; do not report.
                return;
            }

            // Has some other initializer -> suggest localMember
            ctx.ReportDiagnostic(Diagnostic.Create(Rule, syntax.GetLocation(), field.Name, t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));

            return;
        }

        // No initializer: we can still flag.
        ctx.ReportDiagnostic(Diagnostic.Create(Rule, syntax?.GetLocation() ?? field.Locations[0], field.Name, t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }
}
