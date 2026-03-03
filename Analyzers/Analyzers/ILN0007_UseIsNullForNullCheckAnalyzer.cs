using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0007_UseIsNullForNullCheckAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new("ILN0007", "Use isnull() for null checks on ILNumerics arrays",
                                                           "Use 'isnull({0})' instead of '{0} {1} null' for ILNumerics arrays", "ILNumerics", DiagnosticSeverity.Warning, true,
                                                           "ILNumerics arrays should use ILMath.isnull() for null checks instead of == null or != null.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node is not BinaryExpressionSyntax binary)
            return;

        // Check for arr == null or arr != null (or null == arr, null != arr)
        ExpressionSyntax? arrayExpr = null;
        string operatorText;

        if (binary.IsKind(SyntaxKind.EqualsExpression))
            operatorText = "==";
        else if (binary.IsKind(SyntaxKind.NotEqualsExpression))
            operatorText = "!=";
        else
            return;

        if (IsNullLiteral(binary.Right))
            arrayExpr = binary.Left;
        else if (IsNullLiteral(binary.Left))
            arrayExpr = binary.Right;

        if (arrayExpr is null)
            return;

        var model = ctx.SemanticModel;
        var typeInfo = model.GetTypeInfo(arrayExpr, ctx.CancellationToken);
        if (typeInfo.Type is not INamedTypeSymbol namedType || !ILNTypes.IsAnyIln(namedType))
            return;

        var arrayName = arrayExpr.ToString();
        ctx.ReportDiagnostic(Diagnostic.Create(Rule, binary.GetLocation(), arrayName, operatorText));
    }

    private static bool IsNullLiteral(ExpressionSyntax expr) => expr is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);
}
