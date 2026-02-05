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
        var preferredTypeName = ILNTypes.GetPreferredLocalTypeName(namedType);
        var explicitType = SyntaxFactory.ParseTypeName(preferredTypeName).WithTriviaFrom(decl.Type);

        var newDecl = decl.WithType(explicitType);
        var root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

        return doc.WithSyntaxRoot(root!.ReplaceNode(decl, newDecl));
    }
}
