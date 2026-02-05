using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0005_LocalMemberForFieldsAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor AssignRule = new("ILN0005A", "ILNumerics fields should be assigned via '.a' or Assign()",
                                                                 "Field '{0}' should be assigned via '{0}.a = ...' or '{0}.Assign(...)'", "ILNumerics", DiagnosticSeverity.Warning,
                                                                 true);

    public static readonly DiagnosticDescriptor Rule = new("ILN0005", "ILNumerics fields should use localMember<T>() pattern",
                                                           "Field '{0}' of type '{1}' should be initialized via localMember<T>()", "ILNumerics", DiagnosticSeverity.Info, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, AssignRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Check field declarations for the required localMember<T>() initialization pattern
        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);

        // Track assignments and method calls that may write to ILNumerics Array<T> fields
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment, OperationKind.CompoundAssignment);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeField(SymbolAnalysisContext ctx)
    {
        if (ctx.Symbol is not IFieldSymbol field)
            return;

        if (field.Type is not INamedTypeSymbol t || !ILNTypes.IsArray(t))
            return;

        // Only care about instance fields
        if (field.IsStatic)
            return;

        // If field has an initializer, ensure it's localMember<>()
        var declSyntaxRef = field.DeclaringSyntaxReferences.FirstOrDefault();
        if (declSyntaxRef == null)
            return;

        var syntax = declSyntaxRef.GetSyntax(ctx.CancellationToken) as VariableDeclaratorSyntax;
        var initializer = syntax?.Initializer;
        if (syntax != null && initializer != null)
        {
            if (initializer.Value is InvocationExpressionSyntax invocation && invocation.Expression is GenericNameSyntax gName && gName.Identifier.Text == "localMember")
            {
                // Pattern already followed (do not report).
                return;
            }

            // Has some other initializer -> suggest localMember
            ctx.ReportDiagnostic(Diagnostic.Create(Rule, syntax.GetLocation(), field.Name, t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));

            return;
        }

        // No initializer: we can still flag.
        ctx.ReportDiagnostic(Diagnostic.Create(Rule, syntax?.GetLocation() ?? field.Locations[0], field.Name, t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }

    private static void AnalyzeAssignment(OperationAnalysisContext ctx)
    {
        var op = (IAssignmentOperation) ctx.Operation;
        var target = op.Target;

        // Allow pattern: _A.a = ...
        if (target is IPropertyReferenceOperation propRef && propRef.Instance is IFieldReferenceOperation fieldPropRef && IsIlnArrayField(fieldPropRef.Field)
            && propRef.Property.Name == "a")
            return;

        // If assignment goes directly to the field, flag it
        if (target is IFieldReferenceOperation fieldRef && IsIlnArrayField(fieldRef.Field))
            ctx.ReportDiagnostic(Diagnostic.Create(AssignRule, target.Syntax.GetLocation(), fieldRef.Field.Name));
    }

    private static void AnalyzeInvocation(OperationAnalysisContext ctx)
    {
        // This method is registered to allow future extension for detecting
        // other assignment patterns (e.g., via extension methods).
        // Currently, _A.Assign(...) is implicitly allowed by not being flagged
        // in AnalyzeAssignment. This hook exists for completeness.
    }

    private static bool IsIlnArrayField(IFieldSymbol field) => !field.IsStatic && field.Type is INamedTypeSymbol t && ILNTypes.IsArray(t);
}
