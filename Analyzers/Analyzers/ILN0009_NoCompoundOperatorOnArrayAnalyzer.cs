using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0009_NoCompoundOperatorOnArrayAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new("ILN0009", "Avoid compound operators on ILNumerics arrays",
                                                           "Avoid compound operator '{0}' on ILNumerics array '{1}'; use expanded form instead", "ILNumerics",
                                                           DiagnosticSeverity.Warning, true,
                                                           "Compound operators (+=, -=, /=, *=, etc.) on ILNumerics arrays cause subtle bugs. "
                                                           + "When used with indexers, index arguments are disposed after the first evaluation, "
                                                           + "leading to exceptions or incorrect behavior. Use the expanded form instead.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeCompoundAssignment, OperationKind.CompoundAssignment);
    }

    private static void AnalyzeCompoundAssignment(OperationAnalysisContext ctx)
    {
        var op = (ICompoundAssignmentOperation) ctx.Operation;
        var targetType = op.Target.Type as INamedTypeSymbol;

        if (targetType is null || !ILNTypes.IsAnyIln(targetType))
            return;

        var syntax = op.Syntax as AssignmentExpressionSyntax;
        var operatorToken = syntax?.OperatorToken.Text ?? op.OperatorKind.ToString();
        var targetText = syntax?.Left.ToString() ?? op.Target.Syntax.ToString();

        // Compound operators with indexers are always wrong (index args get disposed);
        // without indexers they are technically OK but still discouraged.
        var effectiveSeverity = op.Target is IPropertyReferenceOperation { Property.IsIndexer: true } ? DiagnosticSeverity.Error : DiagnosticSeverity.Warning;

        ctx.ReportDiagnostic(Diagnostic.Create(Rule, op.Syntax.GetLocation(), effectiveSeverity, null, null, operatorToken, targetText));
    }
}
