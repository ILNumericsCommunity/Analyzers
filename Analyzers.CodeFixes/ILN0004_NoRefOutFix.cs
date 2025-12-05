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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ILN0004_NoRefOutFix))]
[Shared]
public sealed class ILN0004_NoRefOutFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0004"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var node = root!.FindNode(diagnostic.Location.SourceSpan);

        var param = node.FirstAncestorOrSelf<ParameterSyntax>();
        if (param is null)
            return;

        context.RegisterCodeFix(CodeAction.Create("Convert to OutArray<> parameter", ct => FixAsync(context.Document, param, ct), "ILN0004"), diagnostic);
    }

    private static async Task<Document> FixAsync(Document doc, ParameterSyntax param, CancellationToken ct)
    {
        // Remove ref/out modifiers
        var newModifiers = new SyntaxTokenList(param.Modifiers.Where(m => m.Kind() != SyntaxKind.OutKeyword && m.Kind() != SyntaxKind.RefKeyword));
        var newType = param.Type;

        // We cannot know T here. Leave type as-is and rely on user to change to OutArray<T> if not already.
        // If type is Array<T>, suggest OutArray<T> by replacing 'Array' with 'OutArray'.
        if (newType is GenericNameSyntax g && g.Identifier.Text == "Array")
            newType = g.WithIdentifier(SyntaxFactory.Identifier("OutArray"));

        var newParam = param.WithModifiers(newModifiers).WithType(newType!);
        var root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

        return doc.WithSyntaxRoot(root!.ReplaceNode(param, newParam));
    }
}
