using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace ILNumerics.Community.Analyzers.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ILN0001_NoVarForArraysAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor Rule = new("ILN0001",
                                                           "Don't use 'var' for ILNumerics arrays",
                                                           "Use explicit '{0}' instead of 'var' for ILNumerics arrays",
                                                           "ILNumerics",
                                                           DiagnosticSeverity.Error,
                                                           true,
                                                           "Implicitly-typed locals ('var') are forbidden for ILNumerics arrays to prevent RetArray reuse bugs.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Analyze all local variable declarations
        context.RegisterOperationAction(AnalyzeDeclaration, OperationKind.VariableDeclarationGroup);
    }

    private static void AnalyzeDeclaration(OperationAnalysisContext ctx)
    {
        var group = (IVariableDeclarationGroupOperation) ctx.Operation;
        foreach (var decl in group.Declarations)
        {
            foreach (var v in decl.Declarators)
            {
                if (v.Symbol is ILocalSymbol ls)
                {
                    // Find out if the declaration actually used the 'var' keyword
                    var declarator = v.Syntax as VariableDeclaratorSyntax;
                    var decleration = declarator?.Parent as VariableDeclarationSyntax;
                    var id = decleration?.Type as IdentifierNameSyntax;
                    var isVar = id is not null && id.Identifier.Text == "var" && id.IsVar;

                    // Try to infer the concrete ILNumerics type from the initializer or the symbol type
                    INamedTypeSymbol? ilnType = null;
                    if (v.Initializer is IVariableInitializerOperation init && init.Value?.Type is INamedTypeSymbol rhsType)
                        ilnType = rhsType;
                    else if (ls.Type is INamedTypeSymbol symType)
                        ilnType = symType;

                    if (isVar && ilnType is not null && IlnTypes.IsAnyIln(ilnType))
                    {
                        var preferredName = GetPreferredLocalIlnTypeName(ilnType);

                        // Report on the variable identifier
                        ctx.ReportDiagnostic(Diagnostic.Create(Rule, v.Syntax.GetLocation(), preferredName));

                        // Also report on the preceding 'var' keyword for better visibility
                        if (id is not null)
                            ctx.ReportDiagnostic(Diagnostic.Create(Rule, id.GetLocation(), preferredName));
                    }
                }
            }
        }
    }

    private static string GetPreferredLocalIlnTypeName(INamedTypeSymbol type)
    {
        // Prefer ILNumerics local types: Array<T>, Logical, Cell
        if (type.ContainingNamespace?.ToDisplayString() == "ILNumerics")
        {
            // In/Out/Ret/Array<T> => Array<T>
            if ((type.MetadataName == "Array`1" || type.MetadataName == "InArray`1" || type.MetadataName == "OutArray`1" || type.MetadataName == "RetArray`1")
                && type.TypeArguments.Length == 1)
            {
                var elemName = type.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                return $"Array<{elemName}>";
            }

            // In/Out/Ret/Logical => Logical
            if (type.MetadataName == "Logical" || type.MetadataName == "InLogical" || type.MetadataName == "OutLogical" || type.MetadataName == "RetLogical")
                return "Logical";

            // In/Out/Ret/Cell => Cell
            if (type.MetadataName == "Cell" || type.MetadataName == "InCell" || type.MetadataName == "OutCell" || type.MetadataName == "RetCell")
                return "Cell";
        }

        // Fallback to the exact inferred type name (minimally qualified)
        return type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }
}
