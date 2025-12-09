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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ILN0005_LocalMemberForFieldsFix))]
[Shared]
public sealed class ILN0005_LocalMemberForFieldsFix : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0005", "ILN0005A"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var span = diagnostic.Location.SourceSpan;

        if (diagnostic.Id == "ILN0005")
        {
            // Find the field declarator corresponding to the diagnostic
            if (root.FindNode(span) is not VariableDeclaratorSyntax declarator)
                return;

            var fieldDecl = declarator.FirstAncestorOrSelf<FieldDeclarationSyntax>();
            if (fieldDecl is null)
                return;

            // This code fix will also apply the ILN0005A-style .a assignments for this field
            context.RegisterCodeFix(CodeAction.Create("Use localMember<T>() and readonly field",
                                                      ct => ApplyLocalMemberFixAsync(context.Document, fieldDecl, declarator, ct),
                                                      "ILN0005_UseLocalMember"), diagnostic);
        }
        else if (diagnostic.Id == "ILN0005A")
        {
            // Find the assignment expression that writes directly to the field
            var node = root.FindNode(span);
            var assignment = node.FirstAncestorOrSelf<AssignmentExpressionSyntax>();
            if (assignment is null)
                return;

            context.RegisterCodeFix(CodeAction.Create("Assign via .a property",
                                                      ct => UseDotAAssignmentAsync(context.Document, assignment, ct),
                                                      "ILN0005A_UseDotA"), diagnostic);
        }
    }

    private static async Task<Document> ApplyLocalMemberFixAsync(Document document, FieldDeclarationSyntax fieldDecl, VariableDeclaratorSyntax declarator,
                                                                 CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        // Ensure 'readonly' modifier is present (preserve visibility and other modifiers)
        var modifiers = fieldDecl.Modifiers;
        if (!modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)))
            modifiers = modifiers.Add(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

        // Build localMember<elementType>() invocation
        var typeSyntax = fieldDecl.Declaration.Type as GenericNameSyntax ??
                         (fieldDecl.Declaration.Type as QualifiedNameSyntax)?.Right as GenericNameSyntax;

        // If we cannot determine generic args, fall back to 'double'
        TypeSyntax elementType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
        if (typeSyntax is { TypeArgumentList.Arguments.Count: 1 })
            elementType = typeSyntax.TypeArgumentList.Arguments[0];

        var localMemberIdentifier = SyntaxFactory.IdentifierName("localMember");
        var genericLocalMember = SyntaxFactory.GenericName(localMemberIdentifier.Identifier)
                                              .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(elementType)));

        var invocation = SyntaxFactory.InvocationExpression(genericLocalMember);
        var initializer = SyntaxFactory.EqualsValueClause(invocation);

        // Attach initializer to the specific variable declarator
        var newDeclarator = declarator.WithInitializer(initializer);

        var declaration = fieldDecl.Declaration.WithVariables(SyntaxFactory.SeparatedList(fieldDecl.Declaration.Variables.Select(v => v == declarator ? newDeclarator : v)));
        var newFieldDecl = fieldDecl.WithDeclaration(declaration).WithModifiers(modifiers);

        var newRoot = root.ReplaceNode(fieldDecl, newFieldDecl);

        // Resolve the field symbol using the original tree before we replace it
        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel is null)
            return document.WithSyntaxRoot(newRoot);

        var fieldSymbol = semanticModel.GetDeclaredSymbol(declarator, ct) as IFieldSymbol;
        if (fieldSymbol is null)
            return document.WithSyntaxRoot(newRoot);

        var newDocument = document.WithSyntaxRoot(newRoot);

        // Reuse the same logic as ILN0005A to rewrite assignments to this field
        return await RewriteAssignmentsToDotAAsync(newDocument, fieldSymbol, ct).ConfigureAwait(false);
    }

    // Rewrite all assignments to the given field symbol to use the .a property (e.g. _A = v -> _A.a = v)
    private static async Task<Document> RewriteAssignmentsToDotAAsync(Document document, IFieldSymbol fieldSymbol, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return document;

        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        var assignments = root.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(a =>
            {
                if (a.Left is not IdentifierNameSyntax id)
                    return false;

                var symbol = semanticModel.GetSymbolInfo(id, ct).Symbol;
                return SymbolEqualityComparer.Default.Equals(symbol, fieldSymbol);
            })
            .ToArray();

        if (assignments.Length == 0)
            return document;

        var newRoot = root;

        foreach (var assignment in assignments)
        {
            var id = (IdentifierNameSyntax)assignment.Left;

            var memberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                id,
                SyntaxFactory.Token(SyntaxKind.DotToken),
                SyntaxFactory.IdentifierName("a"));

            var newAssignment = assignment.WithLeft(memberAccess);
            newRoot = newRoot.ReplaceNode(assignment, newAssignment);
        }

        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> UseDotAAssignmentAsync(Document document, AssignmentExpressionSyntax assignment, CancellationToken ct)
    {
        // Resolve the field symbol from the left-hand side, then delegate to the shared rewriter
        var semanticModel = await document.GetSemanticModelAsync(ct).ConfigureAwait(false);
        if (semanticModel is null)
            return document;

        if (assignment.Left is not IdentifierNameSyntax id)
            return document;

        var fieldSymbol = semanticModel.GetSymbolInfo(id, ct).Symbol as IFieldSymbol;
        if (fieldSymbol is null)
            return document;

        return await RewriteAssignmentsToDotAAsync(document, fieldSymbol, ct).ConfigureAwait(false);
    }
}
