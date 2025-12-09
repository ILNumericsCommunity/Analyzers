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
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0005"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var span = diagnostic.Location.SourceSpan;

        // Find the field declarator corresponding to the diagnostic
        if (root?.FindNode(span) is not VariableDeclaratorSyntax declarator)
            return;

        var fieldDecl = declarator.FirstAncestorOrSelf<FieldDeclarationSyntax>();
        if (fieldDecl is null)
            return;

        context.RegisterCodeFix(CodeAction.Create("Use localMember<T>() and readonly field", ct => ApplyFixAsync(context.Document, fieldDecl, declarator, ct), "ILN0005_UseLocalMember"), diagnostic);
    }

    private static async Task<Document> ApplyFixAsync(Document document, FieldDeclarationSyntax fieldDecl, VariableDeclaratorSyntax declarator, CancellationToken ct)
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

        return document.WithSyntaxRoot(newRoot);
    }
}
