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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ILN0009_NoCompoundOperatorOnArrayFix))]
[Shared]
public sealed class ILN0009_NoCompoundOperatorOnArrayFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0009"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var node = root!.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

        if (node is AssignmentExpressionSyntax assignment)
        {
            context.RegisterCodeFix(CodeAction.Create("Expand compound operator",
                                                      ct => ExpandCompoundOperatorAsync(context.Document, assignment, ct), "ILN0009"), diagnostic);
        }
    }

    private static async Task<Document> ExpandCompoundOperatorAsync(Document doc, AssignmentExpressionSyntax assignment, CancellationToken ct)
    {
        var root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);

        var binaryKind = GetBinaryExpressionKind(assignment.Kind());
        if (binaryKind == SyntaxKind.None)
            return doc;

        // Build: target = target op right
        var binaryExpr = SyntaxFactory.BinaryExpression(binaryKind, assignment.Left.WithoutTrivia(), assignment.Right.WithoutTrivia());
        var newAssignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, assignment.Left.WithoutTrivia(), binaryExpr).WithTriviaFrom(assignment);

        var newRoot = root!.ReplaceNode(assignment, newAssignment);
        return doc.WithSyntaxRoot(newRoot);
    }

    private static SyntaxKind GetBinaryExpressionKind(SyntaxKind compoundKind)
        => compoundKind switch
        {
            SyntaxKind.AddAssignmentExpression => SyntaxKind.AddExpression,
            SyntaxKind.SubtractAssignmentExpression => SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyAssignmentExpression => SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideAssignmentExpression => SyntaxKind.DivideExpression,
            SyntaxKind.ModuloAssignmentExpression => SyntaxKind.ModuloExpression,
            SyntaxKind.AndAssignmentExpression => SyntaxKind.BitwiseAndExpression,
            SyntaxKind.OrAssignmentExpression => SyntaxKind.BitwiseOrExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression => SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.LeftShiftAssignmentExpression => SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftAssignmentExpression => SyntaxKind.RightShiftExpression,
            _ => SyntaxKind.None
        };
}
