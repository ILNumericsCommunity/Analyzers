using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0006_GetValueForScalarAccessAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new("ILN0006", "Use GetValue()/SetValue() for scalar element access", "Use '{0}.GetValue(...)' for scalar element access",
                                                           "ILNumerics", DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeCast, SyntaxKind.CastExpression);
    }

    private static void AnalyzeCast(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not CastExpressionSyntax cast)
            return;

        if (cast.Expression is not ElementAccessExpressionSyntax elementAccess)
            return;

        var model = ctx.SemanticModel;

        var instanceType = ModelExtensions.GetTypeInfo(model, elementAccess.Expression, ctx.CancellationToken).Type as INamedTypeSymbol;
        if (instanceType is null || !ILNTypes.IsArray(instanceType))
            return;

        var castType = ModelExtensions.GetTypeInfo(model, cast.Type, ctx.CancellationToken).Type;
        if (castType is null)
            return;

        // Determine element type of the indexer: use symbol info on the entire element access.
        var valueType = ModelExtensions.GetTypeInfo(model, elementAccess, ctx.CancellationToken).Type;
        if (valueType is null)
            return;

        // Match explicit cast to the array element type (i.e., cast target equals the generic argument T)
        var expectedElementType = instanceType is { TypeArguments.Length: 1 } ? instanceType.TypeArguments[0] : null;
        if (expectedElementType is null)
            return;

        if (!SymbolEqualityComparer.Default.Equals(castType, expectedElementType))
            return;

        var instanceText = elementAccess.Expression.ToString();
        ctx.ReportDiagnostic(Diagnostic.Create(Rule, cast.GetLocation(), instanceText));
    }
}
