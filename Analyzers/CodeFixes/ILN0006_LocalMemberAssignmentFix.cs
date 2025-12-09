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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ILN0006_LocalMemberAssignmentFix))]
[Shared]
public sealed class ILN0006_LocalMemberAssignmentFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0006"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var span = diagnostic.Location.SourceSpan;

        // Find the assignment expression that writes directly to the field
        var node = root.FindNode(span);
        var assignment = node.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
        if (assignment is null)
            return;

        context.RegisterCodeFix(CodeAction.Create("Assign via .a property", ct => UseDotAAssignmentAsync(context.Document, assignment, ct), "ILN0006_UseDotA"), diagnostic);
    }

    private static async Task<Document> UseDotAAssignmentAsync(Document document, AssignmentExpressionSyntax assignment, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        // Replace `_A = value` with `_A.a = value`
        if (assignment.Left is IdentifierNameSyntax id)
        {
            var memberAccess = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, id, SyntaxFactory.Token(SyntaxKind.DotToken), SyntaxFactory.IdentifierName("a"));
            var newAssignment = assignment.WithLeft(memberAccess);
            
            var newRoot = root.ReplaceNode(assignment, newAssignment);

            return document.WithSyntaxRoot(newRoot);
        }

        return document;
    }
}
