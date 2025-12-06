using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ILNumerics.Community.Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ILN0001_NoVarForArraysFix))]
[Shared]
public sealed class ILN0001_NoVarForArraysFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0001"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var node = root!.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        var decl = node.FirstAncestorOrSelf<VariableDeclarationSyntax>();
        if (decl is null || !decl.Type.IsVar)
            return;

        context.RegisterCodeFix(CodeAction.Create("Use explicit (local) ILNumerics type", ct => UseExplicitAsync(context.Document, decl, ct), "ILN0001"), diagnostic);
    }

    private static async Task<Document> UseExplicitAsync(Document doc, VariableDeclarationSyntax decl, CancellationToken ct)
    {
        var model = await doc.GetSemanticModelAsync(ct).ConfigureAwait(false);

        var variable = decl.Variables.FirstOrDefault();
        if (variable?.Initializer?.Value is null)
            return doc;

        var namedType = model!.GetTypeInfo(variable.Initializer.Value, ct).Type as INamedTypeSymbol;
        if (namedType is null)
            return doc;

        // Prefer ILNumerics local types: Array<T>, Logical, Cell
        var preferredType = CreateLocalIlnTypeSyntax(namedType);
        var explicitType = preferredType.WithTriviaFrom(decl.Type);

        var newDecl = decl.WithType(explicitType);
        var root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

        return doc.WithSyntaxRoot(root!.ReplaceNode(decl, newDecl));
    }

    private static TypeSyntax? CreateLocalIlnTypeSyntax(INamedTypeSymbol type)
    {
        // Prefer ILNumerics local types: Array<T>, Logical, Cell
        if (type.ContainingNamespace?.ToDisplayString() != "ILNumerics")
            return null;

        // In/Out/Ret/Array<T> => Array<T>
        if (Is(type, "Array`1") || Is(type, "InArray`1") || Is(type, "OutArray`1") || Is(type, "RetArray`1"))
        {
            if (type.TypeArguments.Length == 1)
            {
                var elemName = type.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                return SyntaxFactory.ParseTypeName($"Array<{elemName}>");
            }
        }

        // In/Out/Ret/Logical => Logical
        if (Is(type, "Logical") || Is(type, "InLogical") || Is(type, "OutLogical") || Is(type, "RetLogical"))
            return SyntaxFactory.IdentifierName("Logical");

        // In/Out/Ret/Cell => Cell
        if (Is(type, "Cell") || Is(type, "InCell") || Is(type, "OutCell") || Is(type, "RetCell"))
            return SyntaxFactory.IdentifierName("Cell");

        // Fallback to the exact inferred type name (minimally qualified)
        return SyntaxFactory.ParseTypeName(type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
    }

    private static bool Is(INamedTypeSymbol t, string metadataName) => t.MetadataName == metadataName;
}
