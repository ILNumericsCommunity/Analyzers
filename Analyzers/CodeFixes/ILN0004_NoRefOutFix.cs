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
    // This code fix responds to diagnostics produced by ILN0004
    public override ImmutableArray<string> FixableDiagnosticIds => ["ILN0004"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var diagnostic = context.Diagnostics.First();
        var node = root!.FindNode(diagnostic.Location.SourceSpan);

        // Expect the diagnostic to be reported on or within a parameter
        var param = node.FirstAncestorOrSelf<ParameterSyntax>();
        if (param is null)
            return;

        // Offer a single code action that converts to OutArray<>
        context.RegisterCodeFix(CodeAction.Create("Convert to OutArray<> parameter", ct => FixAsync(context.Document, param, ct), "ILN0004"), diagnostic);
    }

    private static async Task<Document> FixAsync(Document doc, ParameterSyntax param, CancellationToken ct)
    {
        var root = await doc.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null)
            return doc;

        // Remove ref/out modifiers from the parameter
        var newModifiers = new SyntaxTokenList(param.Modifiers.Where(m => m.Kind() != SyntaxKind.OutKeyword && m.Kind() != SyntaxKind.RefKeyword));
        var newType = param.Type;

        // If type is Array<T>, suggest OutArray<T> by replacing 'Array' with 'OutArray'.
        if (newType is GenericNameSyntax g && g.Identifier.Text == "Array")
            newType = g.WithIdentifier(SyntaxFactory.Identifier("OutArray")).WithTriviaFrom(param.Type!);

        var newParam = param.WithModifiers(newModifiers).WithType(newType!);

        // Update assignments to the former out/ref parameter to use the backing field 'a'
        if (param.Parent?.Parent is MethodDeclarationSyntax method)
        {
            var paramName = param.Identifier.Text;

            // Only run rewriter once, and only if there is at least one simple assignment
            if (method.Body != null &&
                method.Body.DescendantNodes()
                      .OfType<AssignmentExpressionSyntax>()
                      .Any(a => a.Left is IdentifierNameSyntax id && id.Identifier.Text == paramName))
            {
                // Replace direct assignments 'a = expr;' with 'a.a = expr;' in the method body
                var rewriter = new OutArrayAssignmentRewriter(paramName);
                var newBody = (BlockSyntax?)rewriter.Visit(method.Body);

                if (newBody != null)
                {
                    // Build new method with updated body AND updated parameter in one operation
                    // to avoid stale node references
                    var newParamList = method.ParameterList.ReplaceNode(param, newParam);
                    var newMethod = method.WithBody(newBody).WithParameterList(newParamList);
                    return doc.WithSyntaxRoot(root.ReplaceNode(method, newMethod));
                }
            }
        }

        // No method body rewriting needed, just replace the parameter
        return doc.WithSyntaxRoot(root.ReplaceNode(param, newParam));
    }

    // Rewrites simple assignments to the parameter into assignments to the OutArray backing field
    private sealed class OutArrayAssignmentRewriter : CSharpSyntaxRewriter
    {
        private readonly string _parameterName;

        public OutArrayAssignmentRewriter(string parameterName)
        {
            _parameterName = parameterName;
        }

        public override SyntaxNode? VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            // Look for assignments where the left side is the parameter identifier
            if (node.Left is IdentifierNameSyntax identifier && identifier.Identifier.Text == _parameterName)
            {
                // Transform 'a = expr;' into 'a.a = expr;'
                var newLeft = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                                   SyntaxFactory.IdentifierName(_parameterName),
                                                                   SyntaxFactory.IdentifierName("a"));

                return node.WithLeft(newLeft);
            }

            // Defer to base for all other assignments
            return base.VisitAssignmentExpression(node);
        }
    }
}