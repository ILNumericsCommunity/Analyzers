using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0006_LocalMemberAssignmentAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new("ILN0006",
                                                           "ILNumerics fields should be assigned via '.a' or Assign()",
                                                           "Field '{0}' should be assigned via '{0}.a = ...' or '{0}.Assign(...)'",
                                                           "ILNumerics",
                                                           DiagnosticSeverity.Warning,
                                                           true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Track assignments and method calls that may write to ILNumerics Array<T> fields
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment, OperationKind.CompoundAssignment);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeAssignment(OperationAnalysisContext ctx)
    {
        var op = (IAssignmentOperation) ctx.Operation;
        var target = op.Target;

        // Allow pattern: _A.a = ...
        if (target is IPropertyReferenceOperation propRef &&
            propRef.Instance is IFieldReferenceOperation fieldPropRef &&
            IsIlnArrayField(fieldPropRef.Field) &&
            propRef.Property.Name == "a")
            return;

        // If assignment goes directly to the field, flag it
        if (target is IFieldReferenceOperation fieldRef && IsIlnArrayField(fieldRef.Field))
            ctx.ReportDiagnostic(Diagnostic.Create(Rule, target.Syntax.GetLocation(), fieldRef.Field.Name));
    }

    private static void AnalyzeInvocation(OperationAnalysisContext ctx)
    {
        var invocation = (IInvocationOperation) ctx.Operation;

        // Allow: _A.Assign(...)
        if (invocation.Instance is IFieldReferenceOperation fieldRef &&
            IsIlnArrayField(fieldRef.Field) &&
            invocation.TargetMethod.Name == "Assign")
        {
            return;
        }
    }

    private static bool IsIlnArrayField(IFieldSymbol field)
    {
        return !field.IsStatic && field.Type is INamedTypeSymbol t && IlnTypes.IsArray(t);
    }
}
