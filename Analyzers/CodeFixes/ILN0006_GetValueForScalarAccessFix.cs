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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ILN0006_GetValueForScalarAccessFix))]
[Shared]
public sealed class ILN0006_GetValueForScalarAccessFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0006"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        var cast = node as CastExpressionSyntax ?? node.FirstAncestorOrSelf<CastExpressionSyntax>();
        if (cast is null)
            return;

        // We expect: (T) <elementAccess>
        var elementAccess = cast.Expression as ElementAccessExpressionSyntax;
        if (elementAccess is null)
            return;

        context.RegisterCodeFix(CodeAction.Create("Use GetValue(...)", ct => ReplaceWithGetValueAsync(context.Document, cast, elementAccess, ct), "ILN0006_UseGetValue"),
                                diagnostic);
    }

    private static async Task<Document> ReplaceWithGetValueAsync(Document document, CastExpressionSyntax cast, ElementAccessExpressionSyntax elementAccess, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        // Build: <expr>.GetValue(<indices...>)
        var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, AddParensIfNeeded(elementAccess.Expression),
                                                                SyntaxFactory.IdentifierName("GetValue"));

        var invocation = SyntaxFactory.InvocationExpression(memberAccess, SyntaxFactory.ArgumentList(elementAccess.ArgumentList.Arguments));
        invocation = invocation.WithLeadingTrivia(cast.GetLeadingTrivia()).WithTrailingTrivia(cast.GetTrailingTrivia());

        var newRoot = root.ReplaceNode(cast, invocation);

        return document.WithSyntaxRoot(newRoot);
    }

    private static ExpressionSyntax AddParensIfNeeded(ExpressionSyntax expr)
        => expr is IdentifierNameSyntax or MemberAccessExpressionSyntax or ElementAccessExpressionSyntax or InvocationExpressionSyntax
            ? expr
            : SyntaxFactory.ParenthesizedExpression(expr);
}
