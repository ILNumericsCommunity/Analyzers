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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ILN0007_UseIsNullForNullCheckFix))]
[Shared]
public sealed class ILN0007_UseIsNullForNullCheckFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0007"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var node = root!.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        if (node is BinaryExpressionSyntax binary)
            context.RegisterCodeFix(CodeAction.Create("Use isnull()", ct => FixBinaryExpressionAsync(context.Document, binary, ct), "ILN0007"), diagnostic);
    }

    private static async Task<Document> FixBinaryExpressionAsync(Document doc, BinaryExpressionSyntax binary, CancellationToken ct)
    {
        var root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

        // Determine which side is the array and which is null
        ExpressionSyntax arrayExpr;
        if (IsNullLiteral(binary.Right))
            arrayExpr = binary.Left;
        else
            arrayExpr = binary.Right;

        // Build isnull(arr) invocation
        var isnullInvocation = SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName("isnull"),
                                                                  SyntaxFactory.ArgumentList(SyntaxFactory
                                                                                                 .SingletonSeparatedList(SyntaxFactory.Argument(arrayExpr.WithoutTrivia()))));

        ExpressionSyntax replacement;
        if (binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            // arr != null -> !isnull(arr)
            replacement = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, isnullInvocation);
        }
        else
        {
            // arr == null -> isnull(arr)
            replacement = isnullInvocation;
        }

        replacement = replacement.WithTriviaFrom(binary);
        var newRoot = root!.ReplaceNode(binary, replacement);
        return doc.WithSyntaxRoot(newRoot);
    }

    private static bool IsNullLiteral(ExpressionSyntax expr) => expr is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);
}
